using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using StorageServer;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using Entity;
namespace server
{
    interface INetworkServer
    {
        public void SendMessageToChannel(string name, ConnectMessage message);
        public void ListenIncomeMessage();
    }
    public class StateObject {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public SslStream sslStream = null;
    }
    class ServerClient : INetworkServer{ 
        IPAddress Host;
        IPEndPoint endpoint;
        ChannelStorage<SslStream,EndpointEntity> storage = new ChannelStorage<SslStream,EndpointEntity>();
        SettingsChannelStorage<SettingsEntity> storageSettings = new SettingsChannelStorage<SettingsEntity>();
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
        static X509Certificate2 serverCertificate = null;
        public ServerClient(string host, int port){
            IPAddress.TryParse(host, out Host);
            endpoint = new IPEndPoint(Host, port);
            ListenTo();
        }
        public ServerClient(){
            IPAddress.TryParse("127.0.0.1", out IPAddress? Host);
            endpoint = new IPEndPoint(Host, 9090);
            byte answer = ListenTo();
            if (answer==0){
                serverCertificate = new X509Certificate2("/home/nikita/Old/ConsoleChat/keys/server.pfx","123");
                ListenIncomeMessage();
            }
        }
        public byte ListenTo(){
            try
            {
              Console.WriteLine("Start server...");
              serverSocket.Bind(endpoint);
              serverSocket.Listen();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
        public void SendMessageToChannel(string name, ConnectMessage message){
            List<EndpointEntity> listConnections = (List<EndpointEntity>)storage.ReadCertainRecords(name);
            foreach(EndpointEntity connection in listConnections){
                connection.endpoint.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(message)));
            }
        }

        void AcceptCallBack(IAsyncResult ar){
            allDone.Set();
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workSocket = handler;
            NetworkStream networkStream = new NetworkStream(handler);
            SslStream sslStream = new SslStream(networkStream);
            state.sslStream = sslStream;
            sslStream.BeginAuthenticateAsServer(serverCertificate, false,
            SslProtocols.Tls12, false, AuthenticateCallback, state);
        }
        void AuthenticateCallback(IAsyncResult ar){
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                state.sslStream.EndAuthenticateAsServer(ar);
                state.sslStream.BeginRead(state.buffer, 0, state.buffer.Length, RecieveCallBack, state);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SSL authentication failed: {ex.Message}{ex.Source}");
                state.sslStream.Dispose();
            }
        }
        void ResponseServer(SslStream handler,ConnectMessage message){
            if (message.status.Equals("CONNECT")){
                AddNewConnection(handler, message);
            }
            else if(message.status.Equals("MESSAGE")){
                SendMessageToChannel(message.channel,message);
            }
            else if(message.status.Equals("ADD")){
                AddNewConnection(handler, message);
            }
            else if(message.status.Equals("CHECK")){
                CheckChannel(handler, message);
            }
            else if(message.status.Equals("SHOW")){
                ShowUsers(handler, message);
            } 
            else if(message.status.Equals("PRIVATE CHAT")){
                ChangePrivateChat(handler, message);
            }
            else if(message.status.Equals("PUBLIC CHAT")){
                ChangePublicChat(handler, message);
            }
            else if(message.status.Equals("PUBLIC CHAT")){
                ChangePublicChat(handler, message);
            }
            else if(message.status.Equals("BAN")){
                BanUser(handler, message);
            }
        }
        void ResponseFailServer(SslStream client,ConnectMessage message){
            ConnectMessage answer = new ConnectMessage(){status="ERROR", message="You don't have privilege"};
            client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
        }
        void BanUser(SslStream client,ConnectMessage message){
            List<EndpointEntity> listConnections = (List<EndpointEntity>)storage.ReadCertainRecords(message.channel);
            SettingsEntity settings = (SettingsEntity)storageSettings.ReadCertainRecords(message.channel);
            if (!settings.UserBan.Contains(listConnections[message.NumberInformation].endpoint)){
                ConnectMessage answer = new ConnectMessage(){status="BAN", channel=message.channel}; // message for deleting chat from local storage to user was banned
                listConnections[message.NumberInformation].endpoint.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
                StringBuilder banMessage = new StringBuilder($"{listConnections[message.NumberInformation].name} was kicked from {message.channel}");
                message.message = banMessage.ToString();
                SendMessageToChannel(message.channel, message); // To notify all other users who was banned
                settings.UserBan.Add(listConnections[message.NumberInformation].endpoint); // add in black list
                storage.DeleteRecord(message.channel, (byte)message.NumberInformation); // delete from chat in order to he cannot get messages fro channel
            }
        }
        void ChangePrivateChat(SslStream client,ConnectMessage message){
            SettingsEntity settings = (SettingsEntity)storageSettings.ReadCertainRecords(message.channel);
            settings.privateChat = true;
            settings.password = message.listEntity[0];
            ConnectMessage answer = new ConnectMessage(){status="SETTED", message="Set to private chat"};
            client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
        }
        void ChangePublicChat(SslStream client,ConnectMessage message){
            SettingsEntity settings = (SettingsEntity)storageSettings.ReadCertainRecords(message.channel);
            settings.privateChat = false;
            settings.password = "";
            ConnectMessage answer = new ConnectMessage(){status="SETTED", message="Set to public chat"};
            client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
        }
        void RecieveCallBack(IAsyncResult ar){
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject) ar.AsyncState;
            SslStream handler = state.sslStream;
            int bytesRead = state.sslStream.EndRead(ar);
            if (bytesRead > 0){
                ConnectMessage message = JsonSerializer.Deserialize<ConnectMessage>(Encoding.Unicode.GetString(state.buffer,0,bytesRead));
                SettingsEntity? settingsEntity = (SettingsEntity)storageSettings.ReadCertainRecords(message.channel);
                List<EndpointEntity> endpoints = (List<EndpointEntity>)storage.ReadCertainRecords(message.channel);
                PrivilligeSystem system = new PrivilligeSystem(handler,message,
                endpoints, settingsEntity.admins);
                system.PrivillageAction(this.ResponseFailServer ,this.ResponseServer);
                state.sslStream.BeginRead(state.buffer, 0, state.buffer.Length, RecieveCallBack, state);
            }
        }
        void AddNewConnection(SslStream client, ConnectMessage message){
            if (storage.IsExist(message.channel)){
                ConnectMessage answer = new ConnectMessage(){status="FAILED"};
                client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
            }
            else{
                EndpointEntity endpoint = new EndpointEntity(){name=message.sender,
                endpoint=client};
                bool channelIsExist = storage.AddRecord(message.channel, endpoint);
                if (!channelIsExist){
                    List<int> admins = new List<int>(2){0};
                    List<SslStream> listBans = new List<SslStream>(){};
                    SettingsEntity settingsEntity = new SettingsEntity(){privateChat=false, admins=admins, UserBan=listBans};
                    storageSettings.AddRecord(message.channel, settingsEntity);
                    SettingsEntity? Entity = (SettingsEntity)storageSettings.ReadCertainRecords(message.channel);
                }
                ConnectMessage answer = new ConnectMessage(){status="CREATED"};
                client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
            }
        }
        void ShowUsers(SslStream client, ConnectMessage message){
            List<EndpointEntity> users = (List<EndpointEntity>)storage.ReadCertainRecords(message.channel);
            SettingsEntity settings = (SettingsEntity)storageSettings.ReadCertainRecords(message.channel);
            List<string> usersList =  storage.ConvertToStringChannel(users,settings.admins, convertTo);
            ConnectMessage answer = new ConnectMessage(){status="LIST", listEntity=usersList};
            client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
        }
        string convertTo(EndpointEntity user, int index, List<int> admins){
            if (admins.Contains(index)){
                return $"{user.name}(admin)";
            }
            else{
                return user.name;
            }

        }
        void CheckChannel(SslStream client, ConnectMessage message){
            if (storage.IsExist(message.channel)){
                SettingsEntity settingsChannel = (SettingsEntity)storageSettings.ReadCertainRecords(message.channel);
                if (!settingsChannel.privateChat){
                    EndpointEntity endpoint = new EndpointEntity(){name=message.sender,
                    endpoint=client};
                    storage.AddRecord(message.channel, endpoint);
                    ConnectMessage answer = new ConnectMessage(){status="CREATED"};
                    client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
                }
                else{
                    ConnectMessage answer = new ConnectMessage(){status="PASSWORD", channel=message.channel};
                    client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer))); 
                    byte[] buffer = new byte[1024];
                    int totalSize = client.Read(buffer);
                    ConnectMessage privateMessage = JsonSerializer.Deserialize<ConnectMessage>(Encoding.Unicode.GetString(buffer,0,totalSize));
                    if (privateMessage.message.Equals(settingsChannel.password)){
                        EndpointEntity endpoint = new EndpointEntity(){name=message.sender,
                        endpoint=client};
                        storage.AddRecord(message.channel, endpoint);
                        answer = new ConnectMessage(){status="CREATED"};
                        client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer))); 
                    }
                    else {
                        answer = new ConnectMessage(){status="FAILED"};
                        client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer))); 
                    }
                }
            }
            else{
                ConnectMessage answer = new ConnectMessage(){status="ERROR", message="You don't have privilege"};
                client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
            }
        }
        
        public async void ListenIncomeMessage(){
            Console.WriteLine("Listen...");
            while(true){
                allDone.Reset();
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack),serverSocket);
                allDone.WaitOne();
            }
        }
    }

}
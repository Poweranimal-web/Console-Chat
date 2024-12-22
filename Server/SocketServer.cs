using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using StorageServer;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
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
                serverCertificate = new X509Certificate2("D:/ConsoleChat/keys/server.pfx","123");
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
        }
        void ResponseFailServer(SslStream client,ConnectMessage message){
            ConnectMessage answer = new ConnectMessage(){status="ERROR", message="You don't have privilege"};
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
                EndpointEntity endpoint = new EndpointEntity(){name=message.channel,
                endpoint=client};
                bool channelIsExist = storage.AddRecord(message.channel, endpoint);
                if (!channelIsExist){
                    List<int> admins = new List<int>(2){0};
                    SettingsEntity settingsEntity = new SettingsEntity(){privateChat=false, admins=admins};
                    storageSettings.AddRecord(message.channel, settingsEntity);
                    SettingsEntity? Entity = (SettingsEntity)storageSettings.ReadCertainRecords(message.channel);
                }
                ConnectMessage answer = new ConnectMessage(){status="CREATED"};
                client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
            }
        }
        void ShowUsers(SslStream client, ConnectMessage message){
            
        }
        void CheckChannel(SslStream client, ConnectMessage message){
            if (storage.IsExist(message.channel)){
                EndpointEntity endpoint = new EndpointEntity(){name=message.sender,
                endpoint=client};
                storage.AddRecord(message.channel, endpoint);
                ConnectMessage answer = new ConnectMessage(){status="CREATED"};
                client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer)));
            }
            else{
                ConnectMessage answer = new ConnectMessage(){status="FAILED"};
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
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
    class ConnectMessage(){
        public string status{get;set;}
        public string? message{get;set;}
        public string? channel{get;set;}
        public string sender{get;set;}
        public string? IPsender{get;set;} 
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
        ChannelStorage<SslStream> storage = new ChannelStorage<SslStream>();
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
            List<SslStream> listConnections = (List<SslStream>)storage.ReadCertainRecords(name);
            foreach(SslStream connection in listConnections){
                connection.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(message)));
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
        void RecieveCallBack(IAsyncResult ar){
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject) ar.AsyncState;
            SslStream handler = state.sslStream;
            int bytesRead = state.sslStream.EndRead(ar);
            if (bytesRead > 0){
                ConnectMessage message = JsonSerializer.Deserialize<ConnectMessage>(Encoding.Unicode.GetString(state.buffer,0,bytesRead));
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
                state.sslStream.BeginRead(state.buffer, 0, state.buffer.Length, RecieveCallBack, state);
            }
        }
        void AddNewConnection(SslStream client, ConnectMessage message){
            storage.AddRecord(message.channel,client);
            ConnectMessage answer = new ConnectMessage(){status="OK"};
            client.Write(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer))); 
        }
        void CheckChannel(SslStream client, ConnectMessage message){
            if (storage.IsExist(message.channel)){
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
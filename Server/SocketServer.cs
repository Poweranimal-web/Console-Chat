using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using StorageServer;
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
    }
    class ServerClient : INetworkServer{ 
        IPAddress Host;
        IPEndPoint endpoint;
        IStorage<Socket> storage = new ChannelStorage<Socket>();
        Dictionary<string,Socket[]> connections = new Dictionary<string,Socket[]>();  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
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
            List<Socket> listConnections = (List<Socket>)storage.ReadCertainRecords(name);
            foreach(Socket connection in listConnections){
                connection.Send(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(message)));
            }
        }

        void AcceptCallBack(IAsyncResult ar){
            allDone.Set();
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer,0,state.buffer.Length,0, new AsyncCallback(RecieveCallBack), state);
        }
        void AddNewConnection(Socket client, ConnectMessage message){
            storage.AddRecord(message.channel,client);
            ConnectMessage answer = new ConnectMessage(){status="OK"};
            client.Send(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(answer))); 
        }
        void RecieveCallBack(IAsyncResult ar){
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0){
                ConnectMessage message = JsonSerializer.Deserialize<ConnectMessage>(Encoding.Unicode.GetString(state.buffer,0,bytesRead));
                if (message.status.Equals("CONNECT")){
                    AddNewConnection(handler, message);
                }
                else if(message.status.Equals("MESSAGE")){
                    SendMessageToChannel(message.channel,message);
                }
                handler.BeginReceive(state.buffer,0,state.buffer.Length,0, new AsyncCallback(RecieveCallBack), state);
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
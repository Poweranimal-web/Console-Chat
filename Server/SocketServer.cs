using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace server
{
    interface INetworkServer
    {
        public void SendMessageToChannel();
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
    }
    class ServerClient : INetworkServer{ 
        IPAddress Host;
        IPEndPoint endpoint; 
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
              serverSocket.Listen(100);
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
        public void SendMessageToChannel(){

        }
        void AcceptCallBack(IAsyncResult ar){
            allDone.Set();
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer,0,state.buffer.Length,0, new AsyncCallback(RecieveCallBack), state);
        }
        void RecieveCallBack(IAsyncResult ar){
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0){
                Console.WriteLine(Encoding.Default.GetString(state.buffer));
                handler.BeginReceive(state.buffer,0,state.buffer.Length,0, new AsyncCallback(RecieveCallBack), state);
            }
        }
        
        public void ListenIncomeMessage(){
            Console.WriteLine("Listen...");
            while(true){
                allDone.Reset();
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack),serverSocket);
                allDone.WaitOne();
            }


        }
    }

}
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace server
{
    interface INetworkServer
    {
        public void SendMessageToChannel();
        public void ListenIncomeMessage();
    }
    class ServerClient : INetworkServer{ 
        IPAddress Host;
        IPEndPoint endpoint; 
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
        public void ListenIncomeMessage(){
            Console.WriteLine("Listen...");
            while (true)
            {
                Socket client = serverSocket.Accept();
                byte[] buffer = new byte[1024];
                int length = client.Receive(buffer, buffer.Length, SocketFlags.None);
                if (length > 0){
                    string res = Encoding.Default.GetString(buffer);
                    Console.WriteLine(res);
                }
                continue;
            }

        }
    }

}
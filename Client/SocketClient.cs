using System.Net;
using System.Net.Sockets;
using System.Text;
namespace client
{
    interface INetworkClient
    {
        public void SendMessageToChannel();
        public void RecieveMessage();
    }
    class ChatClient : INetworkClient{ 
        IPAddress Host;
        IPEndPoint endpoint; 
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
        public ChatClient(string host, int port){
            
            IPAddress.TryParse(host, out Host);
            endpoint = new IPEndPoint(Host, port);
            ConnectTo();
        }
        public ChatClient(){
            IPAddress.TryParse("127.0.0.1", out IPAddress? Host);
            endpoint = new IPEndPoint(Host, 9090);
            byte res = ConnectTo();
            if (res == 0){
                SendMessageToChannel();
            }
        }
        public byte ConnectTo(){
            try
            {
              clientSocket.Connect(endpoint);
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
        public void SendMessageToChannel(){

            byte[] response = Encoding.Default.GetBytes("Hello world");
            Console.WriteLine(response.Length);
            clientSocket.Send(response, response.Length, SocketFlags.None);
        }
        public void RecieveMessage(){

        }
    }

}
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Storage;
using ConsoleControl;
using Model;
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
        Thread receiveMessageThread;
        ManualResetEvent allDone = new ManualResetEvent(false);
        byte[] buffer = new byte[1024];
        ConnectMessage? Message = null;
        StringBuilder? textMessage = null;
        StringBuilder? nameOwnEndpoint;
        StringBuilder channelCurrent = new StringBuilder();
        ChatsStorage chatsStorage = new ChatsStorage();
        RenderListChats renderListChats = new RenderListChats(); 
        Command command = new Command();
        RenderMessages renderSentMessage = new RenderMessages();
        byte posCursor = 0;
        StringBuilder? bufferText;
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
                receiveMessageThread = new Thread(RecieveMessage);
                receiveMessageThread.Start();
                ConnectToServer();
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
            ConnectMessage newMessage = new ConnectMessage(){status="MESSAGE", channel=channelCurrent.ToString(), 
            message=textMessage.ToString(), sender=nameOwnEndpoint.ToString()};
            renderSentMessage.RenderMessage(newMessage);
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
            clientSocket.Send(response, response.Length, SocketFlags.None);   
        }
        public void ConnectToServer(){
            Console.Write("Enter your name of channel: ");
            nameOwnEndpoint = new StringBuilder(Console.ReadLine());
            ConnectMessage message = new ConnectMessage(){status="CONNECT", channel=nameOwnEndpoint.ToString()};
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(message));
            clientSocket.Send(response, response.Length, SocketFlags.None);
            Console.WriteLine("Wait...");
            allDone.WaitOne();
            if (Message is not null && Message.status.Equals("OK")){
                Console.WriteLine("Launched");
                command.RunConsole(ref clientSocket, ref textMessage, ref channelCurrent,ref posCursor,ref bufferText ,chatsStorage, this);
            }
            
        }
        public void RecieveMessage(){
            RenderMessages render = new RenderMessages();
            while(true){
                    int dataLength = clientSocket.Receive(buffer);
                    Message = JsonSerializer.Deserialize<ConnectMessage>(Encoding.Unicode.GetString(buffer,0,dataLength));
                    if (dataLength > 0 && Message.status.Equals("MESSAGE")){
                        render.RenderRecievedMessage(Message, bufferText);
                    }
                    else{
                        allDone.Set();
                    }
            }
            

        }
    }

}
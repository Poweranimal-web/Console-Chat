using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Storage;
using ConsoleControl;
using Model;
using System.Security.Authentication;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
        ManualResetEvent allDone = new ManualResetEvent(false); // Variable for controling threads in app
        byte[] buffer = new byte[1024];
        ConnectMessage? Message = null; // Variable for storing message that came from server
        StringBuilder? textMessage = null; // Variable stores value of text message for sending 
        StringBuilder channelCurrent = new StringBuilder();
        ChatsStorage chatsStorage = new ChatsStorage();
        Command command = new Command();
        RenderMessages renderSentMessage = new RenderMessages();
        byte posCursor = 0;
        StringBuilder? bufferText;
        StringBuilder? nickname;
        StringBuilder? IP;
        NetworkStream networkStream;   
        SslStream sslStream;
        public ChatClient(string host, int port){
            IPAddress.TryParse(host, out Host);
            endpoint = new IPEndPoint(Host, port);
            ConnectTo();
        }
        public static bool ValidateServerCertificate(
        object sender,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            return false;
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
              networkStream = new NetworkStream(clientSocket);
              sslStream = new SslStream(networkStream, false, (sender, cert, chain, errors)=> true);
              sslStream.AuthenticateAsClient("127.0.0.1", null, SslProtocols.Tls12, 
              checkCertificateRevocation: false);
              Console.WriteLine("SSL authentication succeeded.");
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
            message=textMessage.ToString(), sender=nickname.ToString(), IPsender=IP.ToString()};
            renderSentMessage.RenderMessage(newMessage);
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
            sslStream.Write(response,0,response.Length);   
        }
        public void CheckChannelExist(string channelName){
            allDone.Reset();
            ConnectMessage newMessage = new ConnectMessage(){status="CHECK", channel=channelName, 
            sender=nickname.ToString()};
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
            sslStream.Write(response,0,response.Length);   
        }
        public void ShowAllUsersRequest(){
            ConnectMessage newMessage = new ConnectMessage(){status="SHOW", channel=channelCurrent.ToString(), 
            sender=nickname.ToString()};
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
            sslStream.Write(response,0,response.Length);
        }
        public void SendPasswordRequest(StringBuilder password){
            allDone.Reset();
            ConnectMessage newMessage = new ConnectMessage(){status="PASSWORD",  message=password.ToString()};
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
            sslStream.Write(response,0,response.Length);
        }
        public void ChangePrivacySettingsRequest(bool privateChat,string? password = null){
            if (privateChat){
                List<string> listEntity = new List<string>(1){password};
                ConnectMessage newMessage = new ConnectMessage(){status="PRIVATE CHAT", 
                channel=channelCurrent.ToString(), sender=nickname.ToString(), listEntity=listEntity};
                byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
                sslStream.Write(response,0,response.Length);
            }
            else{
                ConnectMessage newMessage = new ConnectMessage(){status="PUBLIC CHAT", 
                channel=channelCurrent.ToString(), sender=nickname.ToString()};
                byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
                sslStream.Write(response,0,response.Length);
            }
        }
        public void AddToChannel(){
            allDone.Reset();
            ConnectMessage newMessage = new ConnectMessage(){status="ADD", 
            channel=channelCurrent.ToString(), sender=nickname.ToString()};
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
            sslStream.Write(response,0,response.Length);
        }
        public void TestRequest(){
            ConnectMessage newMessage = new ConnectMessage(){status="UPDATE PRIVACY", 
            channel=channelCurrent.ToString(), sender=nickname.ToString()};
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(newMessage));
            sslStream.Write(response,0,response.Length);
        }
        public void ConnectToServer(){
            Console.Write("Enter your nickname: ");
            nickname = new StringBuilder(Console.ReadLine());
            IP = new StringBuilder(clientSocket.LocalEndPoint.ToString());
            ConnectMessage message = new ConnectMessage(){status="CONNECT", channel=nickname.ToString(),
            sender=nickname.ToString()};
            byte[] response = Encoding.Unicode.GetBytes(JsonSerializer.Serialize(message));
            sslStream.Write(response,0,response.Length);
            Console.WriteLine("Wait...");
            allDone.WaitOne();
            if (Message is not null && Message.status.Equals("CREATED")){
                Console.WriteLine("Launched");
                channelCurrent = nickname;
                command.RunConsole(ref clientSocket, ref textMessage, ref channelCurrent,
                ref posCursor,ref bufferText,ref allDone,ref Message,chatsStorage, this);
            }
            else {
                allDone.Reset();
                ConnectToServer();
            }
        }
        public void RecieveMessage(){
            RenderMessages render = new RenderMessages();
            while(true){
                    int dataLength = sslStream.Read(buffer);
                    Message = JsonSerializer.Deserialize<ConnectMessage>(Encoding.Unicode.GetString(buffer,0,dataLength));
                    if (dataLength > 0 && Message.status.Equals("MESSAGE") && !Message.IPsender.ToString().Equals(IP.ToString())){
                        render.RenderRecievedMessage(Message, bufferText);
                    }
                    else if (Message.status.Equals("OK")) {
                        allDone.Set();
                    }
                    else if (Message.status.Equals("CREATED")) {
                        allDone.Set();
                    }
                    else if (Message.status.Equals("FAILED")) {
                        allDone.Set();
                    }
                    else if (Message.status.Equals("ERROR")){
                        render.RenderRecievedMessage(Message, bufferText);
                    }
                    else if (Message.status.Equals("LIST")){
                        render.RenderListMessage(Message, bufferText);
                    }
                    else if (Message.status.Equals("SETTED")){
                        render.RenderRecievedMessage(Message, bufferText);
                    }
                    else if (Message.status.Equals("SETTED")){
                        render.RenderRecievedMessage(Message, bufferText);
                    }
                    else if (Message.status.Equals("PASSWORD")){
                        allDone.Set();
                    }
            }
            

        }
    }

}
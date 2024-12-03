using System.Text;
using System.Net.Sockets;
using Storage;
using Model;
using client;
namespace ConsoleControl{
class Command{
    public void RunConsole(ref Socket socket,ref StringBuilder message, ref StringBuilder channel, ChatsStorage chatStorage, ChatClient client){
        bool isWorking = true;
        while (isWorking){
            Console.Write("Enter command: ");
            StringBuilder command = new StringBuilder(Console.ReadLine());
            switch(command.ToString()){
                    case "exit":
                        socket.Disconnect(false);
                        isWorking = false;
                        break;
                    case "chats":
                        break;
                    case "message":
                        Console.Write("Enter message: ");
                        message = new StringBuilder(Console.ReadLine());
                        Console.Write("Enter channel: ");
                        channel = new StringBuilder(Console.ReadLine());
                        client.SendMessageToChannel();
                        break;
                    case "add chat":
                        Console.Write("Enter Name of Chat: ");
                        StringBuilder nameNewChat = new StringBuilder(Console.ReadLine());
                        Console.Write("Enter Channel: ");
                        StringBuilder newChannel = new StringBuilder(Console.ReadLine());
                        channel = newChannel;
                        Chat chat = new Chat(){name=nameNewChat.ToString(), channel=newChannel.ToString()};
                        chatStorage.AddRecord(nameNewChat.ToString(), chat);
                        goto case "chat message"; 
                    case "chat message":
                        Console.Write("Enter message: ");
                        message = new StringBuilder(Console.ReadLine());
                        if (message.Equals("exit")){
                            break;
                        }
                        else{
                            client.SendMessageToChannel();
                            goto case "chat message";
                        }
                    case "change chat":
                        Console.Write("Enter Name of Chat: ");
                        string nameChannel = Console.ReadLine();
                        if (chatStorage.isExist(nameChannel)){
                            channel = new StringBuilder(nameChannel);
                            goto case "chat message";
                        }
                        else{
                            break;
                        }
                        
                    

            }
        }
        
    } 
}
}
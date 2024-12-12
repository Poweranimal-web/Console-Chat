using System.Text;
using System.Net.Sockets;
using Storage;
using Model;
using client;
namespace ConsoleControl{
class Command{
    public void RunConsole(ref Socket socket,ref StringBuilder message, ref StringBuilder channel, ref byte positionCursor, ref StringBuilder textConsole, ChatsStorage chatStorage, ChatClient client){
        bool isWorking = true;
        RenderListChats renderListChats = new RenderListChats();
        positionCursor = (byte)Console.CursorTop;
        while (isWorking){
            Console.Write("Enter command: ");
            textConsole = new StringBuilder("Enter command: "); 
            StringBuilder command = new StringBuilder(Console.ReadLine());
            switch(command.ToString()){
                    case "exit":
                        socket.Disconnect(false);
                        isWorking = false;
                        break;
                    case "chats":
                        renderListChats.RenderChats((Dictionary<string, Chat>)chatStorage.ReadAllRecords());
                        break;
                    case "message":
                        Console.Write("Enter message: ");
                        textConsole = new StringBuilder("Enter message: ");
                        message = new StringBuilder(Console.ReadLine());
                        Console.Write("Enter channel: ");
                        textConsole = new StringBuilder("Enter channel: ");
                        channel = new StringBuilder(Console.ReadLine());
                        client.SendMessageToChannel();
                        break;
                    case "add chat":
                        Console.Write("Enter Name of Chat: ");
                        textConsole = new StringBuilder("Enter Name of Chat: ");
                        StringBuilder nameNewChat = new StringBuilder(Console.ReadLine());
                        channel = nameNewChat;
                        Chat chat = new Chat(){name=nameNewChat.ToString()};
                        chatStorage.AddRecord(nameNewChat.ToString(), chat);
                        goto case "chat message";
                    case "create chat":
                        Console.Write("Enter Name of Chat: ");
                        textConsole = new StringBuilder("Enter Name of Chat: ");
                        nameNewChat = new StringBuilder(Console.ReadLine());
                        channel = nameNewChat;
                        client.AddToChannel();
                        chat = new Chat(){name=nameNewChat.ToString()};
                        chatStorage.AddRecord(nameNewChat.ToString(), chat);
                        goto case "chat message";
                    case "chat message":
                        Console.Write("Enter message: ");
                        textConsole = new StringBuilder("Enter message: ");
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
                        textConsole = new StringBuilder("Enter Name of Chat: ");
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
using System.Text;
using System.Net.Sockets;
using Storage;
using Model;
using client;
namespace ConsoleControl{
class Command{
    public void RunConsole(ref Socket socket,ref StringBuilder message, ref StringBuilder channel, 
    ref byte positionCursor, ref StringBuilder textConsole, ref ManualResetEvent thread, 
    ref ConnectMessage recievedMessage, ChatsStorage chatStorage, ChatClient client){
        bool isWorking = true;
        RenderListChats renderListChats = new RenderListChats();
        positionCursor = (byte)Console.CursorTop;
        Chat chat;
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
                        client.CheckChannelExist(nameNewChat.ToString());
                        thread.WaitOne();
                        if (recievedMessage.status.Equals("CREATED")){
                            channel = nameNewChat;
                            chat = new Chat(){name=nameNewChat.ToString()};
                            chatStorage.AddRecord(nameNewChat.ToString(), chat);
                            goto case "chat message";
                        }
                        else if (recievedMessage.status.Equals("PASSWORD")){
                            Console.Write("Chat required password: ");
                            textConsole = new StringBuilder("Chat required password: ");
                            StringBuilder password = new StringBuilder(Console.ReadLine());
                            client.SendPasswordRequest(password);
                            thread.WaitOne();
                            if (recievedMessage.status.Equals("CREATED")){
                                channel = nameNewChat;
                                chat = new Chat(){name=nameNewChat.ToString()};
                                chatStorage.AddRecord(nameNewChat.ToString(), chat);
                                goto case "chat message";
                            }
                            else{
                                Console.WriteLine("Adding chat failed! Invalid password of chat.");
                                break;
                            }
                        }
                        else{
                            Console.WriteLine("Adding chat failed! Chat doesn't exist now, you can create your own.");
                            break;
                        }
                    case "create chat":
                        Console.Write("Enter Name of Chat: ");
                        textConsole = new StringBuilder("Enter Name of Chat: ");
                        nameNewChat = new StringBuilder(Console.ReadLine());
                        Console.Write("Is a group chat: ");
                        textConsole = new StringBuilder("Is a group chat: ");
                        StringBuilder isGroupChat = new StringBuilder(Console.ReadLine());
                        bool isGroupChatBool = isGroupChat.ToString().ToUpper().Equals("YES")?true:false; 
                        channel = nameNewChat;
                        client.AddToChannel(isGroupChatBool);
                        thread.WaitOne();
                        if (recievedMessage.status.Equals("CREATED")){
                            chat = new Chat(){name=nameNewChat.ToString()};
                            chatStorage.AddRecord(nameNewChat.ToString(), chat);
                            goto case "chat message";
                        }
                        else{
                            Console.WriteLine("Adding chat failed! Chat with the same name already exist now.");
                            break;
                        }
                    case "chat message":
                        Console.Write("Enter message: ");
                        textConsole = new StringBuilder("Enter message: ");
                        message = new StringBuilder(Console.ReadLine());
                        if (message.Equals("exit")){
                            break;
                        }
                        else if (message.Equals("show users")){
                            client.ShowAllUsersRequest();
                            break;
                        }
                        else if (message.Equals("set private")){
                            Console.Write("Do you want private chat(yes/no): ");
                            textConsole = new StringBuilder("Do you want private chat(yes/no): ");
                            StringBuilder privacy = new StringBuilder(Console.ReadLine());
                            if (privacy.Equals("yes")){
                                Console.Write("Enter password: ");
                                textConsole = new StringBuilder("Enter password: ");
                                StringBuilder password = new StringBuilder(Console.ReadLine());
                                client.ChangePrivacySettingsRequest(true, password.ToString());
                                break;
                            }
                            else if(privacy.Equals("no")){
                                client.ChangePrivacySettingsRequest(false);
                                break;
                            }
                            break;

                        }
                        else if(message.Equals("ban")){
                            Console.Write("Enter index of user: ");
                            textConsole = new StringBuilder("Enter index of user: ");
                            StringBuilder index = new StringBuilder(Console.ReadLine());
                            client.BanUserRequest(index.ToString());
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
                    case "test":
                        client.TestRequest();
                        break;
            }
        }
        
    }
}
}
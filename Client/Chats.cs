using System.Collections;
using Model;
using System.Text;
namespace Storage{
    interface IStorage{
        void AddRecord(string name,object record);
        void DeleteRecord(string name);
        object ReadAllRecords();
    }
    class ChatsStorage : IStorage{
        Dictionary<string, Chat> Chats = new Dictionary<string, Chat>();
        public void AddRecord(string name,object record){
            if(record.GetType() == typeof(Chat)){
                Chats[name] = (Chat)record;
            }
        }
        public void DeleteRecord(string name){
            Chats.Remove(name);
        }
        public object ReadAllRecords(){
            return Chats;
            
        }
        public bool isExist(string name){
            bool exist = Chats.Any((chat)=> chat.Key == name);
            if (exist){
                return true;
            }
            else {
                return true;
            }
        }
    }
    class RenderListChats{
        public void RenderChats(Dictionary<string, Chat> chats){
            byte startedPosition = 0;
            foreach(KeyValuePair<string, Chat> chat in chats){
                Console.WriteLine($"{startedPosition+1}.{chat.Key}");
                startedPosition++;
            }
        }
    }
    class RenderMessages{
        bool isFirstMessage = true;
        public void RenderMessage(ConnectMessage Message){
            // if (isFirstMessage && Message.GetType()==typeof(ConnectMessage)){
            //     Console.WriteLine("\n");
            //     Console.CursorTop = Console.CursorTop-1;
            //     Console.WriteLine($"{Message.sender}:{Message.message}\n");
            //     isFirstMessage = false;
            // }
            // else if (!isFirstMessage && Message.GetType()==typeof(ConnectMessage)){
            Console.CursorTop = Console.CursorTop-1;
            Console.CursorLeft = 0;
            byte cursorTopPosition = (byte)Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.CursorTop = cursorTopPosition;
            Console.CursorLeft = 0;
            Console.Write($"You:{Message.message}\n"); 
            // }
        
        }
        public void RenderRecievedMessage(ConnectMessage Message, StringBuilder buffer){
            // Console.WriteLine("\n");
            // Console.Write(new string(' ', Console.WindowWidth));
            // Console.CursorTop = Console.CursorTop-1;
            // Console.CursorLeft = 0;
            int cursorTopPosition = Console.CursorTop;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorTop = cursorTopPosition;
            Console.CursorLeft = 0;
            Console.Write($"{Message.sender}:{Message.message}\n");
            Console.Write(buffer.ToString());
            // if (isFirstMessage && Message.GetType()==typeof(ConnectMessage)){
            //     Console.WriteLine("\n");
            //     Console.CursorTop = Console.CursorTop-1;
            //     Console.WriteLine($"You:{Message.message}\n");
            //     isFirstMessage = false;
            // }
            // else if (!isFirstMessage && Message.GetType()==typeof(ConnectMessage)){
            //    Console.CursorTop = Console.CursorTop-1;
            //    Console.WriteLine($"You:{Message.message}\n"); 
            // }
        
        }
    }
}
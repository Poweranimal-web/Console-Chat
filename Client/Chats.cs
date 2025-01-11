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
            Console.CursorTop = Console.CursorTop-1;
            Console.CursorLeft = 0;
            byte cursorTopPosition = (byte)Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.CursorTop = cursorTopPosition;
            Console.CursorLeft = 0;
            Console.Write($"You:{Message.message}\n");
        }
        public void RenderLinuxRecievedMessage(ConnectMessage Message, StringBuilder buffer){
            int cursorTopPosition = Console.CursorTop;
            Console.CursorLeft = 0;
            // Console.Write(new string(' ', Console.WindowWidth));
            // Console.CursorLeft = 0;
            Console.Write($"{Message.IPsender}:{Message.message}\n");
            Console.Write(buffer.ToString());           
        }
        public void RenderRecievedMessage(ConnectMessage Message, StringBuilder buffer){
            int cursorTopPosition = Console.CursorTop;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorTop = cursorTopPosition;
            Console.CursorLeft = 0;
            Console.Write($"{Message.IPsender}:{Message.message}\n");
            Console.Write(buffer.ToString());        
        }
        public void RenderListMessage(ConnectMessage Message, StringBuilder buffer){
            int cursorTopPosition = Console.CursorTop;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorTop = cursorTopPosition;
            Console.CursorLeft = 0;
            Console.Write("List:\n");
            for(int i =0;i<Message.listEntity.Count;i++){
                Console.Write($"{i+1}.{Message.listEntity[i]}\n");                
            }
            Console.Write(buffer.ToString());        
        }
    }
}
using System.Collections;
using Model;
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
        int startedPosition = 0;
        public void RenderChats(Dictionary<string, Chat> chats){
            Console.Clear();
            startedPosition = Console.CursorTop;
            foreach(KeyValuePair<string, Chat> chat in chats){
                Console.WriteLine($"{chat.Key}");
            }
            ControlChats();    
        }
        void ControlChats(){
            ConsoleKeyInfo key = Console.ReadKey();
            if (key.Key == ConsoleKey.UpArrow){
                if (startedPosition > Console.CursorTop + 1){
                    Console.CursorTop = Console.CursorTop + 1;
                }
            }
            else if (key.Key == ConsoleKey.DownArrow){
                Console.CursorTop = Console.CursorTop - 1;
            }
        }
    }
}
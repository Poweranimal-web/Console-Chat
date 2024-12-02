using System.Collections;
using Model;
namespace Storage{
    interface IStorage{
        void AddRecord(string name,object record);
        void DeleteRecord(string name);
        void ReadAllRecords();
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
        public void ReadAllRecords(){
            foreach(KeyValuePair<string, Chat> chat in Chats){
                Console.WriteLine($"\u25A1{chat.Key}");
            }
        }
    }
}
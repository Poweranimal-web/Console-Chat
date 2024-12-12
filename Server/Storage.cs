namespace StorageServer{
interface IStorage<T>{
        void AddRecord(string name,T record);
        void DeleteRecord(string name);
        object ReadAllRecords();
        object ReadCertainRecords(string name);
}
    class ChannelStorage<T> : IStorage<T>{
        
        Dictionary<string, List<T>> Channels = new Dictionary<string, List<T>>();
        public void AddRecord(string name,T record){
            if(Channels.ContainsKey(name)){
                Channels[name].Add(record);
            }
            else{
                List<T> values = new List<T>(10);
                values.Add(record);
                Channels[name] = values;
            }
        }
        public void DeleteRecord(string name, byte index){
            Channels[name].RemoveAt(index);
        }
        public void DeleteRecord(string name){
            Channels.Remove(name);
        }
        
        public object ReadCertainRecords(string name){
            return Channels[name];
        }
        public object ReadAllRecords(){
            return Channels;
            
        }
    }
}

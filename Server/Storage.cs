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
                Channels[name] = values;
                Channels[name].Add(record);
            }
        }
        public bool IsExist(string name){
            if (Channels.ContainsKey(name)){
                return true;
            }
            return false;
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
    class KeyChannelStorage<T>: IStorage<T> 
    {
        Dictionary<string, T> Keys = new Dictionary<string, T>();

        public void AddRecord(string name,T record){
            Keys[name] = record;
        }
        public void DeleteRecord(string name){
            Keys.Remove(name);
        }
        public object ReadAllRecords(){
            return Keys;
        }
        public object ReadCertainRecords(string name){
            return Keys[name];
        }

    }
}

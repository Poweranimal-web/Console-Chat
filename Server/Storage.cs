using System.Net.Security;
namespace StorageServer{
interface IStorage<T> {
        void AddRecord(string name,T record);
        void DeleteRecord(string name);
        object ReadAllRecords();
        object ReadCertainRecords(string name);
}
class ConnectMessage(){
        public string status{get;set;}
        public string? message{get;set;}
        public string? channel{get;set;}
        public string sender{get;set;}
        public string? IPsender{get;set;} 
}
class EndpointEntity : IEquatable<EndpointEntity>
{
    public string name{get;set;}
    public SslStream endpoint{get;set;}
    public bool Equals(EndpointEntity endpoint){
        return endpoint.endpoint == this.endpoint && endpoint.name == this.name;
    }
}
class SettingsEntity{
    public bool privateChat{get;set;}
    public string? password{get;set;}
    public List<int> admins{get;set;}
}
    class ChannelStorage<T,U> : IStorage<T> where U: EndpointEntity{
        Dictionary<string, Dictionary<string, List<U>>>? Channels = new Dictionary<string, Dictionary<string, List<U>>>();
        public void AddRecord(string name,T record){
        }
        public bool AddRecord(string name,U record) {
            if(Channels.ContainsKey(name)){
                Channels[name]["endpoints"].Add(record);
                return true;
            }
            else{
                List<U> values = new List<U>(10);
                Dictionary<string, List<U>> endpoints = new Dictionary<string, List<U>>();
                Channels[name] = endpoints;
                Channels[name]["endpoints"] = values;
                Channels[name]["endpoints"].Add(record);
                return false;
            }
        }
        public bool IsExist(string name){
            if (Channels.ContainsKey(name)){
                return true;
            }
            return false;
        }
        public void DeleteRecord(string name, byte index){
            Channels[name]["endpoints"].RemoveAt(index);
        }
        public void DeleteRecord(string name){
            Channels.Remove(name);
        }
        public object? ReadCertainRecords(string name){
            List<U> users;
            if (Channels.ContainsKey(name)){
                if (Channels[name].TryGetValue("endpoints", out users)){
                    return users;
                }
                else{
                    List<U> u = new List<U>();
                    return u;
                }
            }
            else {
                List<U> u = new List<U>();
                return u;
            }
        }
        public object ReadAllRecords(){
            return Channels;
            
        }
    }
    class SettingsChannelStorage<T> : IStorage<T>  
    {
        Dictionary<string, T>? Keys = new Dictionary<string, T>();
        public void AddRecord(string name,T record){
            Keys[name] = record;
        }
        public void DeleteRecord(string name){
            Keys.Remove(name);
        }
        public object ReadAllRecords(){
            return Keys;
        }
        public object? ReadCertainRecords(string name){
            
            T? Settings;
            if (Keys.TryGetValue(name, out Settings)){
                return Settings;
            }
            else{
                SettingsEntity settingsEntity = new SettingsEntity();
                return settingsEntity;
            }
        }

    }
    class PrivilligeSystem{
        string[] highPrivilage = new string[]{"UPDATE PRIVACY"};
        string Username{get;set;}
        string NameChat{get;set;}
        string currentCommand{get;set;}
        List<EndpointEntity>? endpointEntities;
        List<int>? adminList;
        static EndpointEntity clientEndpoint;
        Predicate<EndpointEntity> condition = MatchedUser;
        ConnectMessage Message;
        public PrivilligeSystem(SslStream client, ConnectMessage message, 
        List<EndpointEntity>? endpoints,List<int>? admins){
            Message = message;
            Username = message.sender;
            NameChat = message.channel;
            currentCommand = message.status;
            endpointEntities = endpoints;
            adminList = admins;
            clientEndpoint = new EndpointEntity(){name=message.sender, endpoint=client};
        }
        static bool MatchedUser(EndpointEntity endpoint){
            return endpoint.Equals(clientEndpoint);

        }
        public void PrivillageAction(Action<SslStream,ConnectMessage> actionNotHavePrivilage,Action<SslStream,ConnectMessage> actionHavePrivilage){
            if (Verified()){
                actionHavePrivilage(clientEndpoint.endpoint, Message);
            }
            else{
                actionNotHavePrivilage(clientEndpoint.endpoint, Message);
            }
        }
        int UserIsExistDB(){
            if (this.endpointEntities.Contains(clientEndpoint)){
                return endpointEntities.FindIndex(0,condition);
            }
            else{
                return -1;
            }

        }
        bool Verified(){
            if (highPrivilage.Contains(currentCommand) && adminList.Contains(UserIsExistDB())){
                return true;
            }
            else if (highPrivilage.Contains(currentCommand) && !adminList.Contains(UserIsExistDB())){
                return false;
            }
            else if (!highPrivilage.Contains(currentCommand)){
                return true;
            }
            else{
                return false;
            }
        }

    }
}

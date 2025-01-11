using System.Text;
namespace Model {
    class Chat{
        public string name{get;set;}
    }
    class ConnectMessage(){
        public string status{get;set;}
        public string? message{get;set;}
        public string? channel{get;set;}
        public string sender{get;set;} 
        public string? IPsender{get;set;}
        public int NumberInformation{get;set;}
        public List<string>? listEntity{get;set;}
    }
}
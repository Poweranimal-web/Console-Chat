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
    }
}
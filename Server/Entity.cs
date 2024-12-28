using System.Text;
using System.Net.Security;
namespace Entity{
    class SettingsEntity{
        public bool privateChat{get;set;}
        public string? password{get;set;}
        public List<int> admins{get;set;}
    }
    class ConnectMessage(){
            public string status{get;set;}
            public string? message{get;set;}
            public string? channel{get;set;}
            public string sender{get;set;}
            public string? IPsender{get;set;} 
            public List<string>? listEntity{get;set;}
    }
    class EndpointEntity : IEquatable<EndpointEntity>
    {
        public string name{get;set;}
        public SslStream endpoint{get;set;}
        public bool Equals(EndpointEntity endpoint){
            return endpoint.endpoint == this.endpoint && endpoint.name == this.name;
        }
    }
}
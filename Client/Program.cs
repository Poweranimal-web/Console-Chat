using client;
using System.Text;
class Program
{
    static int Main(string[] args){
        Console.OutputEncoding = Encoding.Unicode;
        Console.InputEncoding = Encoding.Unicode;
        ChatClient client = new ChatClient();
        return 0;
    }
}

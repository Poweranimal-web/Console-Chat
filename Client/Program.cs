using client;
using System.Text;
using System.Runtime.InteropServices;
class Program
{
    static int Main(string[] args){
        #if WINDOWS
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
        #endif
        ChatClient client = new ChatClient();
        return 0;
    }
}

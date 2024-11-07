

namespace ClientTest;
public class Program
{
    static async Task Main()
    {
        Thread.Sleep(6000);
        Client client = new Client();
        try
        {
            client.StartSSLClientExample();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
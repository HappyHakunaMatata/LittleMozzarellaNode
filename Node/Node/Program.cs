using Common.Certificate.Models;
using Node.YggDrasil.cmd;
using System.CommandLine;

namespace Node
{

    class Program
    {

        static async Task Main(string[] args)
        {

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "LittleMozzarella/Server");
            ServiceManager manager = new ServiceManager();
            manager.Identity = ServiceLocator.Instance.GetIdentityCreation();
            manager.Yggdrasil = ServiceLocator.Instance.GetYggdrasil();

            YggdrasilService ygd = new();
            var t = ygd.GetIPAddress("/Users/user/Desktop/yggdrasil.conf");
            Console.WriteLine($"{t.Item1},{t.Item2}");
            t = ygd.GetSnet("/Users/user/Desktop/yggdrasil.conf");
            Console.WriteLine($"{t.Item1},{t.Item2}");
            t = ygd.GetPemKey("/Users/user/Desktop/yggdrasil.conf");
            Console.WriteLine($"{t.Item1},{t.Item2}");
            t = ygd.GetPrivateKey("/Users/user/Desktop/yggdrasil.conf");
            Console.WriteLine($"{t.Item1},{t.Item2}");
            var s = ygd.Version();
            Console.WriteLine(s);
            s = ygd.BuildName();
            Console.WriteLine(s);
            t = ygd.Genconf(false);
            Console.WriteLine($"{t.Item1},{t.Item2}");
            t = ygd.NormaliseConfing("/Users/user/Documents/GitHub/LittleMozzarellaNode/Node/Node/bin/Debug/net8.0/yggdrasil.conf");
            Console.WriteLine($"{t.Item1},{t.Item2}");
            t = ygd.SetLogLevel(YggDrasil.models.LogLevels.error);
            Console.WriteLine($"{t.Item1},{t.Item2}");
            ygd.Logto(path: "/Users/user/Documents/GitHub/LittleMozzarellaNode/Node/Node/bin/Debug/net8.0/yggdrasil.log");
            t = ygd.SetLogLevel(YggDrasil.models.LogLevels.error);
            Console.WriteLine($"{t.Item1},{t.Item2}");
            var task = Task.Run(() => ygd.RunYggdrasilAsync("/Users/user/Desktop/yggdrasil.conf"));
            bool r = ygd.ExitYggdrasil();
            Console.WriteLine(r);
            task.Wait();
            Console.WriteLine($"{task.Result.Item1},{task.Result.Item2}");

            CertificateSettings settings = new CertificateSettings()
            {
                OrganizationName = "LittleMozzarella",
                IssuerName = "SelfIssued",
                Difficulty = 36,
            };
            settings.SetIPAdress("127.0.0.1");


            RootCommand root = new RootCommand();

            var CreateCommand = new Command(
            name: "Create",
            description: "Create a new full identity for a service");

            var CancelCommand = new Command(
            name: "Cancel",
            description: "Cancel creation identity for a service");

            root.AddCommand(CreateCommand);
            root.AddCommand(CancelCommand);


            CreateCommand.SetHandler(async () =>
            {
                await manager.CreateFullCertificateAuthority(path, settings);
            });
            CancelCommand.SetHandler(manager.CancelCreation);
            


            if (args.Length > 0)
            {
                root.Invoke(args);
                return;
            }
            StartReadConsole(root);
        }

        public static void StartReadConsole(RootCommand root)
        {
            try
            {
                while (true)
                {
                    var arguments = Console.ReadLine();
                    if (string.IsNullOrEmpty(arguments))
                    {
                        break;
                    }
                    var args = arguments.Split(" ");
                    if (args.First() == "Create")
                    {
                        root.InvokeAsync(args);
                    }
                    else
                    {
                        root.Invoke(args);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
        }
    }
}

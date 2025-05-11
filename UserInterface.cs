using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Stalkiana_Console
{
    public static class UserInterface
    {
        public static void displayStartingScreen()
        {
            Console.Clear();
            Console.WriteLine("Welcome to Stalkiana the instagram stalking tool\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"  _________  __           .__    __    .__                         
 /   _____/_/  |_ _____   |  |  |  | __|__|_____     ____  _____   
 \_____  \ \   __\\__  \  |  |  |  |/ /|  |\__  \   /    \ \__  \  
 /        \ |  |   / __ \_|  |__|    < |  | / __ \_|   |  \ / __ \_
/_______  / |__|  (____  /|____/|__|_ \|__|(____  /|___|  /(____  /
        \/             \/            \/         \/      \/      \/ ");
            Console.ResetColor();
            Console.Write("\nThis is a tool used for stalking an Instagram user\n");
        }
        public static string getUsername()
        {
            string username;
            do
            {
                Console.Write("\nPlease input the username to stalk: ");
                username = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("Username cannot be empty. Please enter a valid username.");
                }
            } while (string.IsNullOrWhiteSpace(username));
            return username;
        }
        public static string getOption()
        {
            string option;
            do
            {
                Console.WriteLine("\n1- Download Profile Picture ");
                Console.WriteLine("2- Get Followers/Following");
                Console.WriteLine("3- Show Local History");
                Console.WriteLine("4- Download Posts");
                Console.WriteLine("5- Download Stories");
                Console.WriteLine("6- Open Folder\n");
                Console.Write("Choose what you want to do: ");
                option = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(option))
                {
                    Console.WriteLine("Option cannot be empty. Please enter a valid option (1, 2, 3, 4, 5 or 6)");
                }
            } while (option != "1" && option != "2" && option != "3" && option != "4" && option != "5" && option != "6");
            return option;
        }

        public static string getCookie(string configFile)
        {
            if (File.Exists(configFile + ".txt"))
            {
                try
                {
                    return File.ReadAllText(configFile + ".txt").Replace("\n", "");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error reading TXT config: " + ex.Message + "\nFallback to manual credentials.");
                }
            }
            else if (File.Exists(configFile + ".yaml"))
            {
                try
                {
                    string yamlFileFullPath = configFile + ".yaml";
                    string yamlContent = File.ReadAllText(yamlFileFullPath);

                    var deserializer = new DeserializerBuilder().Build();

                    AppConfig configObject = deserializer.Deserialize<AppConfig>(yamlContent);
                    if (configObject != null && configObject.cookie != null)
                    {
                        return configObject.cookie.ToString();
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: Cookie not found or is null in YAML config.\nFallback to manual credentials.");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error reading YAML config: " + ex.Message + "\nFallback to manual credentials.");
                }
            }
            else if (File.Exists(configFile + ".json"))
            {
                try
                {
                    AppConfig configObject = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(configFile + ".json"))!;
                    if (configObject != null && configObject.cookie != null)
                    {
                        return configObject.cookie.ToString();
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: Cookie not found or is null in JSON config.\nFallback to manual credentials.");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error reading JSON config: " + ex.Message + "\nFallback to manual credentials.");
                }
            }

            string cookie;
            do
            {
                Console.Write("\nPlease input the full instagram cookie: ");
                cookie = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    Console.WriteLine("Cookie cannot be empty. Please enter a valid cookie.");
                }
            } while (string.IsNullOrWhiteSpace(cookie));
            return cookie;
        }
    }
}
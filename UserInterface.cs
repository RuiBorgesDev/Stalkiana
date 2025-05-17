using YamlDotNet.Serialization;

namespace Stalkiana_Console
{
    public static class UserInterface
    {
        public static void displayStartingScreen()
        {
            Console.Clear();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"  _________  __           .__    __    .__                         
 /   _____/_/  |_ _____   |  |  |  | __|__|_____     ____  _____   
 \_____  \ \   __\\__  \  |  |  |  |/ /|  |\__  \   /    \ \__  \  
 /        \ |  |   / __ \_|  |__|    < |  | / __ \_|   |  \ / __ \_
/_______  / |__|  (____  /|____/|__|_ \|__|(____  /|___|  /(____  /
        \/             \/            \/         \/      \/      \/ ");
            Console.ResetColor();
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
        public static int getOption()
        {
            int option;
            string? input;

            do
            {
                Console.WriteLine("\n1- Download Profile Picture ");
                Console.WriteLine("2- Get Followers/Following");
                Console.WriteLine("3- Show Local History");
                Console.WriteLine("4- Download Posts");
                Console.WriteLine("5- Download Stories");
                Console.WriteLine("6- Get User ID");
                Console.WriteLine("7- Save Cookie");
                Console.WriteLine("8- List All Users");
                Console.WriteLine("9- Open Folder\n");
                Console.Write("Choose what you want to do: ");
                input = Console.ReadLine();
                if (!int.TryParse(input, out option) || option > 9 || option <= 0)
                {
                    Console.Clear();
                    Console.WriteLine("Please enter a valid option");
                }
            } while (option > 9 || option <= 0);
            return option;
        }

        public static string getCookieInput()
        {
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

        public static string getCookie(string configFilePath)
        {
            if (File.Exists(configFilePath + ".txt"))
            {
                try
                {
                    return File.ReadAllText(configFilePath + ".txt").Replace("\n", "");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error reading TXT config: " + ex.Message + "\nFallback to manual credentials.");
                }
            }
            else if (File.Exists(configFilePath + ".yaml") || File.Exists(configFilePath + ".yml"))
            {
                try
                {
                    string yamlFileFullPath = File.Exists(configFilePath + ".yaml") ? configFilePath + ".yaml" : configFilePath + ".yml";
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

            return getCookieInput();
        }
    }
}
using Newtonsoft.Json;

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
                Console.WriteLine("4- Download Media\n");
                Console.Write("Choose what you want to do: ");
                option = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(option))
                {
                    Console.WriteLine("Option cannot be empty. Please enter a valid option (1, 2, 3 or 4).");
                }
            } while (option != "1" && option != "2" && option != "3" && option != "4");
            return option;
        }

        public static string getCookie(string configFile)
        {
            string cookie;
            if (File.Exists(configFile))
            {
                try
                {
                    AppConfig configObject = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(configFile))!;
                    return configObject.cookie!.ToString();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error: " + ex.Message + "\nFallback to manual credentials.");
                }
            }

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
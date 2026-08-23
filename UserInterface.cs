using Newtonsoft.Json;

namespace Stalkiana_Console
{
    public class CookieConfig
    {
        public string ActiveCookie { get; set; } = string.Empty;
        public Dictionary<string, string> Cookies { get; set; } = new Dictionary<string, string>();
    }

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
                Console.Write("\nPlease input the username to target: ");
                username = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("Username cannot be empty. Please enter a valid username.");
                }
            } while (string.IsNullOrWhiteSpace(username));
            return username;
        }

        public static int getOption(ref string username)
        {
            int option;
            string? input;

            while (true)
            {
                Console.WriteLine();
                
                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("1- Set Target Username");
                    Console.WriteLine("2- Manage Cookies");
                    Console.WriteLine("3- List All Users");
                    Console.WriteLine("4- Exit\n");
                    Console.Write("Choose what you want to do: ");

                    input = Console.ReadLine();
                    
                    if (input == "1")
                    {
                        username = getUsername();
                        Console.Clear();
                        displayStartingScreen();
                        continue;
                    }
                    else if (input == "2") return 7;
                    else if (input == "3") return 8;
                    else if (input == "4") return 10;
                    else
                    {
                        Console.Clear();
                        displayStartingScreen();
                        Console.WriteLine("\nPlease enter a valid option.");
                    }
                }

                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"--- Current Target: {username} ---");
                    Console.ResetColor();
                    Console.WriteLine("1- Download Profile Picture ");
                    Console.WriteLine("2- Get Followers/Following");
                    Console.WriteLine("3- Show Local History");
                    Console.WriteLine("4- Download Posts");
                    Console.WriteLine("5- Download Stories");
                    Console.WriteLine("6- Get User ID");
                    Console.WriteLine("7- Manage Cookies");
                    Console.WriteLine("8- List All Users");
                    Console.WriteLine("9- Open Folder");
                    Console.WriteLine("10- Change/Clear Target Username");
                    Console.WriteLine("11- Exit\n");
                    Console.Write("Choose what you want to do: ");
                    
                    input = Console.ReadLine();
                    
                    if (int.TryParse(input, out option) && option >= 1 && option <= 11)
                    {
                        if (option == 10) 
                        {
                            username = string.Empty;
                            Console.Clear();
                            displayStartingScreen();
                            continue;
                        }
                        if (option == 11) return 10;
                        
                        return option;
                    }

                    Console.Clear();
                    displayStartingScreen();
                    Console.WriteLine("\nPlease enter a valid option.");
                }
            }
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

        private static CookieConfig LoadConfig(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    return JsonConvert.DeserializeObject<CookieConfig>(File.ReadAllText(path)) ?? new CookieConfig();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to load JSON config: {ex.Message}");
                }
            }
            return new CookieConfig();
        }

        private static void SaveConfig(string path, CookieConfig config)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public static string getCookie(string configFilePath)
        {
            var config = LoadConfig(configFilePath);
            
            if (!string.IsNullOrEmpty(config.ActiveCookie) && config.Cookies.ContainsKey(config.ActiveCookie))
            {
                return config.Cookies[config.ActiveCookie];
            }

            Console.WriteLine("\nNo active cookie found. Please configure a cookie first.");
            manageCookies(configFilePath);

            config = LoadConfig(configFilePath);
            if (!string.IsNullOrEmpty(config.ActiveCookie) && config.Cookies.ContainsKey(config.ActiveCookie))
            {
                return config.Cookies[config.ActiveCookie];
            }

            return getCookieInput();
        }

        public static void manageCookies(string configFilePath)
        {
            CookieConfig config = LoadConfig(configFilePath);

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=== Manage Cookies ===");
                Console.ResetColor();
                Console.WriteLine($"Active Cookie: {(string.IsNullOrEmpty(config.ActiveCookie) ? "None" : config.ActiveCookie)}\n");

                Console.WriteLine("1. List all cookies");
                Console.WriteLine("2. Add a new cookie");
                Console.WriteLine("3. Update an existing cookie");
                Console.WriteLine("4. Remove a cookie");
                Console.WriteLine("5. Rename a cookie");
                Console.WriteLine("6. Set active cookie");
                Console.WriteLine("7. Back to main menu");
                Console.Write("\nChoose an option: ");

                string? choice = Console.ReadLine();

                if (choice == "7") break; 
                
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        if (config.Cookies.Count == 0)
                        {
                            Console.WriteLine("No cookies saved.");
                        }
                        else
                        {
                            foreach (var kvp in config.Cookies)
                            {
                                string activeMarker = (kvp.Key == config.ActiveCookie) ? " [ACTIVE]" : "";
                                Console.WriteLine($"- {kvp.Key}{activeMarker}");
                            }
                        }
                        break;

                    case "2":
                        Console.Write("Enter a name/alias for the new cookie: ");
                        string? addName = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(addName)) break;

                        if (config.Cookies.ContainsKey(addName))
                        {
                            Console.WriteLine("A cookie with this name already exists.");
                        }
                        else
                        {
                            string newCookieStr = getCookieInput();
                            config.Cookies[addName] = newCookieStr;

                            if (string.IsNullOrEmpty(config.ActiveCookie) || config.Cookies.Count == 1)
                            {
                                config.ActiveCookie = addName;
                            }
                            SaveConfig(configFilePath, config);
                            Console.WriteLine($"\nCookie '{addName}' added successfully.");
                        }
                        break;

                    case "3":
                        Console.Write("Enter the name of the cookie to update: ");
                        string? updateName = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(updateName)) break;

                        if (config.Cookies.ContainsKey(updateName))
                        {
                            string updatedCookieStr = getCookieInput();
                            config.Cookies[updateName] = updatedCookieStr;
                            SaveConfig(configFilePath, config);
                            Console.WriteLine($"\nCookie '{updateName}' updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Cookie not found.");
                        }
                        break;

                    case "4":
                        Console.Write("Enter the name of the cookie to remove: ");
                        string? removeName = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(removeName)) break;

                        if (config.Cookies.ContainsKey(removeName))
                        {
                            config.Cookies.Remove(removeName);
                            if (config.ActiveCookie == removeName)
                            {
                                config.ActiveCookie = config.Cookies.Keys.FirstOrDefault() ?? string.Empty;
                            }
                            SaveConfig(configFilePath, config);
                            Console.WriteLine($"\nCookie '{removeName}' removed successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Cookie not found.");
                        }
                        break;

                    case "5":
                        Console.Write("Enter the current name of the cookie: ");
                        string? oldName = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(oldName)) break;

                        if (config.Cookies.ContainsKey(oldName))
                        {
                            Console.Write("Enter the new name: ");
                            string? newName = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(newName)) break;

                            if (config.Cookies.ContainsKey(newName))
                            {
                                Console.WriteLine("The new name already exists.");
                            }
                            else
                            {
                                string val = config.Cookies[oldName];
                                config.Cookies.Remove(oldName);
                                config.Cookies[newName] = val;
                                
                                if (config.ActiveCookie == oldName)
                                {
                                    config.ActiveCookie = newName;
                                }
                                SaveConfig(configFilePath, config);
                                Console.WriteLine($"\nCookie renamed from '{oldName}' to '{newName}'.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Cookie not found.");
                        }
                        break;

                    case "6":
                        Console.Write("Enter the name of the cookie to set as active: ");
                        string? activeName = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(activeName)) break;

                        if (config.Cookies.ContainsKey(activeName))
                        {
                            config.ActiveCookie = activeName;
                            SaveConfig(configFilePath, config);
                            Console.WriteLine($"\n'{activeName}' is now the active cookie.");
                        }
                        else
                        {
                            Console.WriteLine("Cookie not found.");
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }
    }
}
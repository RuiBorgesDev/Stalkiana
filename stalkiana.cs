/*

Do not use the tool multiple times per day or you might get flagged by Instagram

*/

using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Stalkiana_Console
{
    public class AppConfig
    {
        public string? cookie { get; set; }
    }
    internal class Program
    {
        static Dictionary<string, string>? getDataFromFile(string filename)
        {
            string jsonString = File.ReadAllText(filename);

            try
            {
                var userList = JsonConvert.DeserializeObject<List<JObject>>(jsonString);
                var dictionary = new Dictionary<string, string>();

                foreach (JObject user in userList!)
                {
                    var userPK = user["userPK"]!.ToString();
                    var username = user["username"]!.ToString();
                    dictionary[userPK] = username;
                }

                return dictionary;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        static string dictionaryToJsonString(Dictionary<string, string> list)
        {
            var jsonArray = list.Select(kv => new { userPK = kv.Key, username = kv.Value }).ToArray();
            return JsonConvert.SerializeObject(jsonArray);
        }

        static string getCsrftoken(string cookie)
        {
            string regex = @"(?<=csrftoken=)(\S+)(?=;)";
            Match match = Regex.Match(cookie, regex);
            string csrftoken = match.Value;
            return csrftoken;
        }

        static void Main(string[] args)
        {
            Dictionary<string, string>? usersFollowing;
            Dictionary<string, string>? usersFollowers;

            Dictionary<string, string>? mediaList;

            var usersFollowingFile = new Dictionary<string, string>();
            var usersFollowersFile = new Dictionary<string, string>();
            var resultLines = new List<string>();

            const int minTime = 1000;
            const int maxTime = 2500;
            string username;
            string option;
            string cookie;
            string csrftoken;
            int countUsers = 50;
            int countMedia = 24;
            string? userPK;

            int userFollowerCount;
            int userFollowingCount;

            string configFile = "cookie.json";

            UserInterface.displayStartingScreen();

            if (args.Length == 1)
            {
                username = args[0];
            }
            else
            {
                username = UserInterface.getUsername();
            }

            option = UserInterface.getOption();

            string followingFileName = $"{username}/{username}_followings.json";
            string followersFileName = $"{username}/{username}_followers.json";
            string resultFileName = $"{username}/result.txt";

            if (option == "1")
            {
                cookie = UserInterface.getCookie(configFile);
                csrftoken = getCsrftoken(cookie);
                userPK = InstagramAPI.getUserPK(cookie, username);

                if (userPK == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user PK");
                    return;
                }

                Directory.CreateDirectory(username);
                string? profileImagePath = CreateNewFile($"{username}/{username}_profileImage.jpg", getMediaBytesFromUrl(InstagramAPI.getProfileImageUrl(userPK, csrftoken)!)!);
                Console.WriteLine($"{(profileImagePath == null ? "\nThe profile picture is unchanged. No new file created" : $"\nThe profile picture was successfully saved in ./{profileImagePath}")}");
            }

            else if (option == "2")
            {
                cookie = UserInterface.getCookie(configFile);
                csrftoken = getCsrftoken(cookie);
                userPK = InstagramAPI.getUserPK(cookie, username);

                if (userPK == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user PK");
                    return;
                }

                Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following");

                (userFollowingCount, userFollowerCount) = InstagramAPI.getFollowingAndFollowerCount(userPK, cookie, csrftoken);

                if (userFollowerCount < 1 || userFollowingCount < 1)
                {
                    Console.Error.WriteLine("Something went wrong while getting following and follower counts");
                    return;
                }

                if (File.Exists(followingFileName) && File.Exists(followersFileName))
                {
                    usersFollowingFile = getDataFromFile(followingFileName);
                    usersFollowersFile = getDataFromFile(followersFileName);
                }

                if (usersFollowingFile == null || usersFollowersFile == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting local following and followers");
                    return;
                }

                Console.WriteLine($"\nPrevious follower count: {usersFollowersFile!.Count}, previous following count: {usersFollowingFile!.Count}");
                Console.WriteLine($"Current follower count:  {userFollowerCount}, current following count:  {userFollowingCount}\n");

                Console.WriteLine("Getting Following...");
                usersFollowing = InstagramAPI.getFollowingOrFollowerList(userPK, cookie, minTime, maxTime, countUsers, "following");

                if (usersFollowing == null || Math.Abs(userFollowingCount - usersFollowing.Count) >= 2)//Sometimes a few followings are not fetched
                {
                    Console.Error.WriteLine("Something went wrong while fetching Following");
                    return;
                }

                Console.WriteLine("Getting Followers...");
                usersFollowers = InstagramAPI.getFollowingOrFollowerList(userPK, cookie, minTime, maxTime, countUsers, "followers");

                if (usersFollowers == null || Math.Abs(userFollowerCount - usersFollowers.Count) >= 2)//Sometimes a few followers are not fetched
                {
                    Console.Error.WriteLine("Something went wrong while fetching Followers");
                    return;
                }

                Directory.CreateDirectory(username);

                File.WriteAllText(followersFileName, dictionaryToJsonString(usersFollowers));
                File.WriteAllText(followingFileName, dictionaryToJsonString(usersFollowing));

                Console.WriteLine("\n\nVerifying...\n");

                resultLines.Add($"\n{DateTime.Now}: Current Follower count: {userFollowerCount}, Current Following count: {userFollowingCount}");
                resultLines.Add($"{DateTime.Now}: {username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(usersFollowing.Count < usersFollowingFile.Count ? usersFollowingFile.Count - usersFollowing.Count : usersFollowing.Count - usersFollowingFile.Count)} users");
                Console.WriteLine($"\n{username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(usersFollowing.Count < usersFollowingFile.Count ? usersFollowingFile.Count - usersFollowing.Count : usersFollowing.Count - usersFollowingFile.Count)} users");

                foreach (var user in usersFollowingFile)
                {
                    if (!usersFollowing.ContainsKey(user.Key))
                    {
                        resultLines.Add($"{username} stopped following {user.Value}");
                        Console.WriteLine($"{username} stopped following {user.Value}");
                    }
                }

                foreach (var user in usersFollowing)
                {
                    if (!usersFollowingFile.ContainsKey(user.Key))
                    {
                        resultLines.Add($"{username} started following {user.Value}");
                        Console.WriteLine($"{username} started following {user.Value}");
                    }
                }

                resultLines.Add($"{DateTime.Now}: {(usersFollowers.Count < usersFollowersFile.Count ? usersFollowersFile.Count - usersFollowers.Count : usersFollowers.Count - usersFollowersFile.Count)} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");
                Console.WriteLine($"\n{(usersFollowers.Count < usersFollowersFile.Count ? usersFollowersFile.Count - usersFollowers.Count : usersFollowers.Count - usersFollowersFile.Count)} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");

                foreach (var user in usersFollowersFile)
                {
                    if (!usersFollowers.ContainsKey(user.Key))
                    {
                        resultLines.Add($"{user.Value} stopped following {username}");
                        Console.WriteLine($"{user.Value} stopped following {username}");
                    }
                }

                foreach (var user in usersFollowers)
                {
                    if (!usersFollowersFile.ContainsKey(user.Key))
                    {
                        resultLines.Add($"{user.Value} started following {username}");
                        Console.WriteLine($"{user.Value} started following {username}");
                    }
                }

                resultLines.Add($"{DateTime.Now}: Name changes");
                Console.WriteLine("\nName changes");

                foreach (var user in usersFollowersFile)
                {
                    if (usersFollowers.ContainsKey(user.Key) && usersFollowers[user.Key] != usersFollowersFile[user.Key])
                    {
                        resultLines.Add($"{user.Value} changed their username to {usersFollowers[user.Key]}");
                        Console.WriteLine($"{user.Value} changed their username to {usersFollowers[user.Key]}");
                    }
                }

                foreach (var user in usersFollowingFile)
                {
                    if (usersFollowing.ContainsKey(user.Key) && usersFollowing[user.Key] != usersFollowingFile[user.Key])
                    {
                        resultLines.Add($"{user.Value} changed their username to {usersFollowing[user.Key]}");
                        Console.WriteLine($"{user.Value} changed their username to {usersFollowing[user.Key]}");
                    }
                }

                File.AppendAllLines(resultFileName, resultLines);
                Console.WriteLine($"\nFinished successfully, results saved in ./{username}/results.txt");
            }
            else if (option == "3")
            {
                if (File.Exists($"{username}/result.txt"))
                {
                    Console.WriteLine(File.ReadAllText($"{username}/result.txt"));
                }
                else
                {
                    Console.WriteLine($"\nThe local history for {username} does not exist, please use the Get Followers/Following on {username} first to create the history.");
                }
            }
            else if (option == "4")
            {
                cookie = UserInterface.getCookie(configFile);
                csrftoken = getCsrftoken(cookie);

                mediaList = InstagramAPI.getMediaList(cookie, csrftoken, 250, 500, countMedia, username);

                if (mediaList == null || mediaList!.Count == 0)
                {
                    Console.Error.WriteLine($"Something went wrong while fetching media: {mediaList}");
                    return;
                }

                string mediaInfo = "";

                foreach (var media in mediaList)
                {
                    mediaInfo += media.Key + ": " + media.Value + "\n";
                }

                Directory.CreateDirectory($"{username}/media");
                File.WriteAllText($"{username}/media.txt", mediaInfo);
                Console.WriteLine("\nDownloading media...\n");

                string fileExtPattern = @"\.[a-zA-Z0-9]{2,5}(?=\?)";
                foreach (var media in mediaList)
                {
                    byte[] mediaBytes = getMediaBytesFromUrl(media.Value)!;
                    Match match = Regex.Match(media.Value, fileExtPattern);
                    CreateNewFile($"{username}/media/{media.Key}{match.Value}", mediaBytes);
                }

                Console.WriteLine($"Media of {username} saved in ./{username}/media/ and the URLs were saved in ./{username}/media.txt");
            }
            return;
        }

        static byte[]? getMediaBytesFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            using (var client = new RestClient())
            {
                return client.DownloadData(new RestRequest(url, Method.Get));
            }
        }

        public static string? CreateNewFile(string desiredFullFilePath, byte[] newFileContent)
        {
            if (string.IsNullOrEmpty(desiredFullFilePath) || newFileContent == null)
            {
                return null;
            }

            string? directoryPath = Path.GetDirectoryName(desiredFullFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(desiredFullFilePath);
            string fileExtension = Path.GetExtension(desiredFullFilePath);

            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Environment.CurrentDirectory;
            }

            if (!File.Exists(desiredFullFilePath))
            {
                File.WriteAllBytes(desiredFullFilePath, newFileContent);
                return desiredFullFilePath;
            }
            else
            {
                byte[] existingFileContent = File.ReadAllBytes(desiredFullFilePath);
                if (existingFileContent.SequenceEqual(newFileContent))
                {
                    return null;
                }
            }

            int counter = 1;
            string currentFilePath;
            while (true)
            {
                string newFileName = $"{fileNameWithoutExtension}({counter}){fileExtension}";
                currentFilePath = Path.Combine(directoryPath, newFileName);

                if (!File.Exists(currentFilePath))
                {
                    File.WriteAllBytes(currentFilePath, newFileContent);
                    return currentFilePath;
                }
                else
                {
                    byte[] existingFileContent = File.ReadAllBytes(currentFilePath);
                    if (existingFileContent.SequenceEqual(newFileContent))
                    {
                        return null;
                    }
                }
                counter++;
            }
        }
    }
}
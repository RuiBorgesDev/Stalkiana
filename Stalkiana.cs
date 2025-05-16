/*

Do not use the tool multiple times per day or you might get flagged by Instagram

*/

using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace Stalkiana_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string stalkianaBasePath = Path.Combine(userProfileDirectory, ".stalkiana");
            const int minTime = 100;
            const int maxTime = 250;
            string username;
            string option;
            string cookie;
            string csrftoken;
            const int countUsers = 64;
            const int countPost = 28;
            string? userID;
            string configFileName = Path.Combine(stalkianaBasePath, "cookie");

            Directory.CreateDirectory(stalkianaBasePath);

            UserInterface.displayStartingScreen();
            option = UserInterface.getOption();

            if (args.Length == 1)
            {
                username = args[0];
            }
            else if (option == "1" || option == "2" || option == "3" || option == "4" || option == "5" || option == "6")
            {
                username = UserInterface.getUsername();
            }
            else
            {
                username = "";
            }

            string userSpecificBasePath = Path.Combine(stalkianaBasePath, username);
            string resultFilePath = Path.Combine(userSpecificBasePath, "result.txt");
            Directory.CreateDirectory(userSpecificBasePath);

            if (option == "1")
            {
                cookie = UserInterface.getCookie(configFileName);
                csrftoken = Helper.getCsrftoken(cookie);
                userID = InstagramAPI.getUserID(username);

                if (userID == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user ID");
                    return;
                }

                string profileImageFileName = $"{username}_profileImage.jpg";
                string profileImageFullPath = Path.Combine(userSpecificBasePath, profileImageFileName);
                string? profileImagePath = Helper.CreateNewFile(profileImageFullPath, Helper.getPostBytesFromUrl(InstagramAPI.getProfileImageUrl(userID, csrftoken)!)!);
                Console.WriteLine($"{(profileImagePath == null ? "\nThe profile picture is unchanged. No new file created" : $"\nThe profile picture was successfully saved in {profileImageFullPath}")}");
            }

            else if (option == "2")
            {
                cookie = UserInterface.getCookie(configFileName);
                csrftoken = Helper.getCsrftoken(cookie);
                userID = InstagramAPI.getUserID(username);

                string followingFilePath = Path.Combine(userSpecificBasePath, $"{username}_followings.json");
                string followersFilePath = Path.Combine(userSpecificBasePath, $"{username}_followers.json");

                if (userID == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user ID");
                    return;
                }

                Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                var usersFollowingFile = new Dictionary<string, string>();
                var usersFollowersFile = new Dictionary<string, string>();
                int userFollowerCount;
                int userFollowingCount;
                var resultLines = new List<string>();

                (userFollowingCount, userFollowerCount) = InstagramAPI.getFollowingAndFollowerCount(userID, cookie, csrftoken);

                if (userFollowerCount < 1 || userFollowingCount < 1)
                {
                    Console.Error.WriteLine("Something went wrong while getting following and follower counts");
                    return;
                }

                if (File.Exists(followingFilePath) && File.Exists(followersFilePath))
                {
                    usersFollowingFile = Helper.getDataFromFile(followingFilePath);
                    usersFollowersFile = Helper.getDataFromFile(followersFilePath);
                }

                if (usersFollowingFile == null || usersFollowersFile == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting local following and followers");
                    return;
                }

                Console.WriteLine($"Previous follower count: {usersFollowersFile!.Count}, previous following count: {usersFollowingFile!.Count}");
                Console.WriteLine($"Current follower count:  {userFollowerCount}, current following count:  {userFollowingCount}\n");

                Dictionary<string, string>? usersFollowing;
                Dictionary<string, string>? usersFollowers;

                Console.WriteLine("Getting Following...");
                usersFollowing = InstagramAPI.getFollowingOrFollowerList(userID, cookie, minTime, maxTime, countUsers, "following");

                if (usersFollowing == null || Math.Abs(userFollowingCount - usersFollowing.Count) >= 3)//Sometimes a few followings are not fetched
                {
                    Console.Error.WriteLine($"Something went wrong while fetching following: {usersFollowing}");
                    return;
                }

                Console.WriteLine("Getting Followers...");
                usersFollowers = InstagramAPI.getFollowingOrFollowerList(userID, cookie, minTime, maxTime, countUsers, "followers");

                if (usersFollowers == null || Math.Abs(userFollowerCount - usersFollowers.Count) >= 3)//Sometimes a few followers are not fetched
                {
                    Console.Error.WriteLine($"Something went wrong while fetching followers: {usersFollowers}");
                    return;
                }

                File.WriteAllText(followersFilePath, Helper.dictionaryToJsonString(usersFollowers));
                File.WriteAllText(followingFilePath, Helper.dictionaryToJsonString(usersFollowing));

                resultLines.Add($"\n{DateTime.Now:yyyy/MM/dd HH:mm}: Current Follower count: {userFollowerCount}, Current Following count: {userFollowingCount}");
                resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: {username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(usersFollowing.Count < usersFollowingFile.Count ? usersFollowingFile.Count - usersFollowing.Count : usersFollowing.Count - usersFollowingFile.Count)} users");
                Console.WriteLine($"\n{username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(usersFollowing.Count < usersFollowingFile.Count ? usersFollowingFile.Count - usersFollowing.Count : usersFollowing.Count - usersFollowingFile.Count)} users");

                resultLines.AddRange(Helper.compareLists(usersFollowersFile, usersFollowers, username, "followers"));

                resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: {(usersFollowers.Count < usersFollowersFile.Count ? usersFollowersFile.Count - usersFollowers.Count : usersFollowers.Count - usersFollowersFile.Count)} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");
                Console.WriteLine($"\n{(usersFollowers.Count < usersFollowersFile.Count ? usersFollowersFile.Count - usersFollowers.Count : usersFollowers.Count - usersFollowersFile.Count)} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");

                resultLines.AddRange(Helper.compareLists(usersFollowingFile, usersFollowing, username, "following"));

                bool hasNameChanges = false;

                foreach (var user in usersFollowersFile)
                {
                    if (usersFollowers.ContainsKey(user.Key) && usersFollowers[user.Key] != usersFollowersFile[user.Key])
                    {
                        hasNameChanges = true;
                        resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: {user.Value} changed their username to {usersFollowers[user.Key]}");
                        Console.WriteLine($"{user.Value} changed their username to {usersFollowers[user.Key]}");
                    }
                }

                foreach (var user in usersFollowingFile)
                {
                    if (usersFollowing.ContainsKey(user.Key) && usersFollowing[user.Key] != usersFollowingFile[user.Key])
                    {
                        hasNameChanges = true;
                        resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: {user.Value} changed their username to {usersFollowing[user.Key]}");
                        Console.WriteLine($"{user.Value} changed their username to {usersFollowing[user.Key]}");
                    }
                }

                if (!hasNameChanges)
                {
                    resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: There are no name changes");
                    Console.WriteLine("\nThere are no name changes");
                }

                File.AppendAllLines(resultFilePath, resultLines);
                Console.WriteLine($"\nFinished successfully, results saved in {resultFilePath}");
            }

            else if (option == "3")
            {
                if (File.Exists(resultFilePath))
                {
                    Console.WriteLine(File.ReadAllText(resultFilePath));
                }
                else
                {
                    Console.WriteLine($"\nThe local history for {username} does not exist, please use the Get Followers/Following on {username} first to create the history");
                }
            }

            else if (option == "4")
            {
                cookie = UserInterface.getCookie(configFileName);
                csrftoken = Helper.getCsrftoken(cookie);

                Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                Dictionary<string, string>? postList;

                postList = InstagramAPI.getPostList(cookie, csrftoken, minTime, maxTime, countPost, username);

                if (postList == null || postList!.Count == 0)
                {
                    Console.Error.WriteLine($"Something went wrong while fetching posts: {postList}");
                    return;
                }

                string postInfo = "";

                foreach (var post in postList)
                {
                    postInfo += post.Key + ": " + post.Value + "\n";
                }

                string postsDirectory = Path.Combine(userSpecificBasePath, "posts");
                Directory.CreateDirectory(postsDirectory);
                string postsTxtFile = Path.Combine(userSpecificBasePath, "posts.txt");
                File.WriteAllText(postsTxtFile, postInfo);
                Console.WriteLine("\nDownloading posts...\n");

                string fileExtPattern = @"\.[a-zA-Z0-9]{2,5}(?=\?)";
                foreach (var post in postList)
                {
                    byte[] postBytes = Helper.getPostBytesFromUrl(post.Value)!;
                    Match match = Regex.Match(post.Value, fileExtPattern);
                    string postFileName = post.Key + match.Value;
                    string postFilePath = Path.Combine(postsDirectory, postFileName);
                    Helper.CreateNewFile(postFilePath, postBytes);
                }

                Console.WriteLine($"Post of {username} saved in {postsDirectory} and the URLs were saved in {postsTxtFile}");
            }

            else if (option == "5")
            {
                cookie = UserInterface.getCookie(configFileName);
                csrftoken = Helper.getCsrftoken(cookie);
                userID = InstagramAPI.getUserID(username);

                Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                Dictionary<string, string>? storiesList;

                if (userID == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user ID");
                    return;
                }

                storiesList = InstagramAPI.getStoriesList(cookie, csrftoken, userID);

                if (storiesList == null || storiesList!.Count == 0)
                {
                    Console.Error.WriteLine($"Something went wrong while fetching stories: {storiesList}");
                    return;
                }

                string storiesInfo = "";

                foreach (var story in storiesList)
                {
                    storiesInfo += story.Key + ": " + story.Value + "\n";
                }

                string storiesDirectory = Path.Combine(userSpecificBasePath, "stories");
                Directory.CreateDirectory(storiesDirectory);
                string storiesTxtFile = Path.Combine(userSpecificBasePath, "stories.txt");
                File.AppendAllText(storiesTxtFile, storiesInfo);

                Console.WriteLine("\nDownloading stories...\n");

                string fileExtPattern = @"\.[a-zA-Z0-9]{2,5}(?=\?)";
                foreach (var story in storiesList)
                {
                    byte[] storyBytes = Helper.getPostBytesFromUrl(story.Value)!;
                    Match match = Regex.Match(story.Value, fileExtPattern);
                    string storyFileName = story.Key + match.Value;
                    string storyFilePath = Path.Combine(storiesDirectory, storyFileName);
                    Helper.CreateNewFile(storyFilePath, storyBytes);
                }

                Console.WriteLine($"Stories of {username} saved in {storiesDirectory} and the URLs were saved in {storiesTxtFile}");
            }

            else if (option == "6")
            {
                Console.WriteLine("Getting user ID...\n");
                userID = InstagramAPI.getUserID(username);

                if (userID == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user ID");
                    return;
                }

                Console.WriteLine($"\n{username} has the ID: {userID}\n");
            }

            else if (option == "7")
            {
                cookie = UserInterface.getCookieInput();
                try
                {
                    string configFilePath = Path.Combine(stalkianaBasePath, configFileName + ".yaml");
                    var yaml = new YamlStream();
                    if (File.Exists(configFilePath))
                    {
                        using (var reader = new StreamReader(configFilePath))
                        {
                            yaml.Load(reader);
                        }
                    }
                    else
                    {
                        yaml.Documents.Add(new YamlDocument(new YamlMappingNode()));
                    }

                    var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
                    rootNode.Children[new YamlScalarNode("cookie")] = new YamlScalarNode(cookie.Trim());
                    using (var writer = new StreamWriter(configFilePath))
                    {
                        yaml.Save(writer, assignAnchors: false);
                    }

                    Console.WriteLine($"\nCookie successfully saved to: {configFilePath}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"\nAn error occurred while saving the cookie: {ex.Message}");
                }
            }

            else if (option == "8")
            {
                Console.WriteLine("\nListing all users with stored data:\n");
                try
                {
                    var userDirectories = Directory.GetDirectories(stalkianaBasePath);
                    if (userDirectories.Length == 0)
                    {
                        Console.WriteLine("No user data found in .stalkiana");
                    }
                    else
                    {
                        foreach (var dirPath in userDirectories)
                        {
                            Console.WriteLine($"- {Path.GetFileName(dirPath)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error listing users: {ex.Message}");
                }
                Console.WriteLine();
            }

            else if (option == "9")
            {
                Helper.OpenFolder(userSpecificBasePath);
            }

            return;
        }
    }
}
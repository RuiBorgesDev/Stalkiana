using System.Text.RegularExpressions;

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
            const int countUsers = 64;
            const int countPosts = 28;
            
            string configFileName = Path.Combine(stalkianaBasePath, "cookies.json");
            Directory.CreateDirectory(stalkianaBasePath);

            string username = args.Length > 0 ? args[0] : string.Empty;

            while (true)
            {
                UserInterface.displayStartingScreen();
                
                int option = UserInterface.getOption(ref username);

                if (option == 10)
                {
                    break;
                }

                string userSpecificBasePath = string.Empty;
                string resultFilePath = string.Empty;
                string cookie;
                string csrftoken;
                string? userID;

                if (!string.IsNullOrEmpty(username))
                {
                    userSpecificBasePath = Path.Combine(stalkianaBasePath, username);
                    resultFilePath = Path.Combine(userSpecificBasePath, "result.txt");
                    Directory.CreateDirectory(userSpecificBasePath);
                }

                if (option == 1)
                {
                    cookie = UserInterface.getCookie(configFileName);
                    csrftoken = Helper.getCsrftoken(cookie);
                    userID = InstagramAPI.getUserID(username);

                    if (userID == null)
                    {
                        Console.Error.WriteLine("Something went wrong while getting user ID");
                    }
                    else
                    {
                        string profileImageFileName = $"{username}_profileImage.jpg";
                        string profileImageFullPath = Path.Combine(userSpecificBasePath, profileImageFileName);
                        string? profileImagePath = Helper.CreateNewFile(profileImageFullPath, Helper.getPostBytesFromUrl(InstagramAPI.getProfileImageUrl(userID, csrftoken)!)!);
                        Console.WriteLine($"{(profileImagePath == null ? "\nThe profile picture is unchanged. No new file created" : $"\nThe profile picture was successfully saved in {profileImageFullPath}")}");
                    }
                }

                else if (option == 2)
                {
                    cookie = UserInterface.getCookie(configFileName);
                    csrftoken = Helper.getCsrftoken(cookie);
                    userID = InstagramAPI.getUserID(username);

                    string followingFilePath = Path.Combine(userSpecificBasePath, $"{username}_followings.json");
                    string followersFilePath = Path.Combine(userSpecificBasePath, $"{username}_followers.json");

                    if (userID == null)
                    {
                        Console.Error.WriteLine("Something went wrong while getting user ID");
                    }
                    else
                    {
                        Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                        var usersFollowingFile = new Dictionary<string, string>();
                        var usersFollowersFile = new Dictionary<string, string>();
                        var resultLines = new List<string>();

                        (int userFollowingCount, int userFollowerCount) = InstagramAPI.getFollowingAndFollowerCount(userID, cookie, csrftoken);

                        if (userFollowerCount < 1 || userFollowingCount < 1)
                        {
                            Console.Error.WriteLine("Something went wrong while getting following and follower counts");
                        }
                        else
                        {
                            if (File.Exists(followingFilePath) && File.Exists(followersFilePath))
                            {
                                usersFollowingFile = Helper.getDataFromFile(followingFilePath) ?? new Dictionary<string, string>();
                                usersFollowersFile = Helper.getDataFromFile(followersFilePath) ?? new Dictionary<string, string>();
                            }

                            Console.WriteLine($"Previous follower count: {usersFollowersFile.Count}, previous following count: {usersFollowingFile.Count}");
                            Console.WriteLine($"Current follower count:  {userFollowerCount}, current following count:  {userFollowingCount}\n");

                            Console.WriteLine("Getting Following...");
                            var usersFollowing = InstagramAPI.getFollowingOrFollowerList(userID, cookie, minTime, maxTime, countUsers, "following");

                            int followingThreshold = (int)Math.Max(2, userFollowingCount * 0.01);

                            if (usersFollowing == null || Math.Abs(userFollowingCount - usersFollowing.Count) > followingThreshold)
                            {
                                Console.Error.WriteLine($"Error: Fetching following failed or count mismatch too high.");
                                Console.Error.WriteLine($"Expected: {userFollowingCount}, Fetched: {usersFollowing?.Count ?? 0}, Allowed Diff: {followingThreshold}");
                            }
                            else
                            {
                                Console.WriteLine("Getting Followers...");
                                var usersFollowers = InstagramAPI.getFollowingOrFollowerList(userID, cookie, minTime, maxTime, countUsers, "followers");

                                int followerThreshold = (int)Math.Max(2, userFollowerCount * 0.01);

                                if (usersFollowers == null || Math.Abs(userFollowerCount - usersFollowers.Count) > followerThreshold)
                                {
                                    Console.Error.WriteLine($"Error: Fetching followers failed or count mismatch too high.");
                                    Console.Error.WriteLine($"Expected: {userFollowerCount}, Fetched: {usersFollowers?.Count ?? 0}, Allowed Diff: {followerThreshold}");
                                }
                                else
                                {
                                    File.WriteAllText(followersFilePath, Helper.dictionaryToJsonString(usersFollowers));
                                    File.WriteAllText(followingFilePath, Helper.dictionaryToJsonString(usersFollowing));

                                    resultLines.Add($"\n{DateTime.Now:yyyy/MM/dd HH:mm}: Current Follower count: {userFollowerCount}, Current Following count: {userFollowingCount}");
                                    resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: {username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(Math.Abs(usersFollowingFile.Count - usersFollowing.Count))} users");
                                    Console.WriteLine($"\n{username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(Math.Abs(usersFollowingFile.Count - usersFollowing.Count))} users");

                                    resultLines.AddRange(Helper.compareLists(usersFollowingFile, usersFollowing, username, "following"));

                                    resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: {(Math.Abs(usersFollowersFile.Count - usersFollowers.Count))} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");
                                    Console.WriteLine($"\n{(Math.Abs(usersFollowersFile.Count - usersFollowers.Count))} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");

                                    resultLines.AddRange(Helper.compareLists(usersFollowersFile, usersFollowers, username, "followers"));

                                    bool hasNameChanges = false;
                                    Console.WriteLine();

                                    var allUsersFile = usersFollowersFile.Concat(usersFollowingFile).GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.Last().Value);
                                    var allUsers = usersFollowers.Concat(usersFollowing).GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.Last().Value);

                                    foreach (var user in allUsersFile)
                                    {
                                        if (allUsers.ContainsKey(user.Key) && allUsers[user.Key] != allUsersFile[user.Key])
                                        {
                                            hasNameChanges = true;
                                            resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: {user.Value} changed their username to {allUsers[user.Key]}");
                                            Console.WriteLine($"{user.Value} changed their username to {allUsers[user.Key]}");
                                        }
                                    }

                                    if (!hasNameChanges)
                                    {
                                        resultLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm}: There are no name changes");
                                        Console.WriteLine("There are no name changes");
                                    }

                                    File.AppendAllLines(resultFilePath, resultLines);
                                    Console.WriteLine($"\nFinished successfully, results saved in {resultFilePath}");
                                }
                            }
                        }
                    }
                }

                else if (option == 3)
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

                else if (option == 4)
                {
                    cookie = UserInterface.getCookie(configFileName);
                    csrftoken = Helper.getCsrftoken(cookie);

                    Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                    Dictionary<string, string>? postList = InstagramAPI.getPostList(cookie, csrftoken, minTime, maxTime, countPosts, username);

                    if (postList == null || postList!.Count == 0)
                    {
                        Console.Error.WriteLine($"Something went wrong while fetching posts.");
                    }
                    else
                    {
                        string postInfo = string.Empty;
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
                }

                else if (option == 5)
                {
                    cookie = UserInterface.getCookie(configFileName);
                    csrftoken = Helper.getCsrftoken(cookie);
                    userID = InstagramAPI.getUserID(username);

                    Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                    if (userID == null)
                    {
                        Console.Error.WriteLine("Something went wrong while getting user ID");
                    }
                    else
                    {
                        Dictionary<string, string>? storiesList = InstagramAPI.getStoriesList(cookie, csrftoken, userID);

                        if (storiesList == null || storiesList!.Count == 0)
                        {
                            Console.Error.WriteLine($"Something went wrong while fetching stories (Or user has no stories).");
                        }
                        else
                        {
                            string storiesInfo = string.Empty;
                            foreach (var story in storiesList)
                            {
                                storiesInfo += story.Key + ": " + story.Value + "\n";
                            }

                            string storiesDirectory = Path.Combine(userSpecificBasePath, "stories");
                            Directory.CreateDirectory(storiesDirectory);
                            string storiesTxtFile = Path.Combine(userSpecificBasePath, "stories.txt");
                            File.AppendAllText(storiesTxtFile, storiesInfo);

                            Console.WriteLine("Downloading stories...\n");

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
                    }
                }

                else if (option == 6)
                {
                    Console.WriteLine("Getting user ID...\n");
                    userID = InstagramAPI.getUserID(username);

                    if (userID == null)
                    {
                        Console.Error.WriteLine("Something went wrong while getting user ID");
                    }
                    else
                    {
                        Console.WriteLine($"\n{username} has the ID: {userID}\n");
                    }
                }

                else if (option == 7)
                {
                    UserInterface.manageCookies(configFileName);
                }

                else if (option == 8)
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
                }

                else if (option == 9)
                {
                    Helper.OpenFolder(userSpecificBasePath);
                }

                Console.WriteLine("\nPress Enter to return to the menu...");
                Console.ReadLine();
                Console.Clear();
            }
        }
    }
}
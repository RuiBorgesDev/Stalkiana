/*

Do not use the tool multiple times per day or you might get flagged by Instagram

*/

using System.Text.RegularExpressions;

namespace Stalkiana_Console
{
    public class AppConfig
    {
        public string? cookie { get; set; }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string>? usersFollowing;
            Dictionary<string, string>? usersFollowers;

            Dictionary<string, string>? postList;
            Dictionary<string, string>? storiesList;

            var usersFollowingFile = new Dictionary<string, string>();
            var usersFollowersFile = new Dictionary<string, string>();
            var resultLines = new List<string>();

            const int minTime = 100;
            const int maxTime = 250;
            string username;
            string option;
            string cookie;
            string csrftoken;
            int countUsers = 64;
            int countPost = 28;
            string? userPK;

            int userFollowerCount;
            int userFollowingCount;

            string configFile = "cookie";

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
                csrftoken = Helper.getCsrftoken(cookie);
                userPK = InstagramAPI.getUserID(username);

                if (userPK == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user PK");
                    return;
                }

                Directory.CreateDirectory(username);
                string? profileImagePath = Helper.CreateNewFile($"{username}/{username}_profileImage.jpg", Helper.getPostBytesFromUrl(InstagramAPI.getProfileImageUrl(userPK, csrftoken)!)!);
                Console.WriteLine($"{(profileImagePath == null ? "\nThe profile picture is unchanged. No new file created" : $"\nThe profile picture was successfully saved in ./{profileImagePath}")}");
            }

            else if (option == "2")
            {
                cookie = UserInterface.getCookie(configFile);
                csrftoken = Helper.getCsrftoken(cookie);
                userPK = InstagramAPI.getUserID(username);

                if (userPK == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user PK");
                    return;
                }

                Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                (userFollowingCount, userFollowerCount) = InstagramAPI.getFollowingAndFollowerCount(userPK, cookie, csrftoken);

                if (userFollowerCount < 1 || userFollowingCount < 1)
                {
                    Console.Error.WriteLine("Something went wrong while getting following and follower counts");
                    return;
                }

                if (File.Exists(followingFileName) && File.Exists(followersFileName))
                {
                    usersFollowingFile = Helper.getDataFromFile(followingFileName);
                    usersFollowersFile = Helper.getDataFromFile(followersFileName);
                }

                if (usersFollowingFile == null || usersFollowersFile == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting local following and followers");
                    return;
                }

                Console.WriteLine($"Previous follower count: {usersFollowersFile!.Count}, previous following count: {usersFollowingFile!.Count}");
                Console.WriteLine($"Current follower count:  {userFollowerCount}, current following count:  {userFollowingCount}\n");

                Console.WriteLine("Getting Following...");
                usersFollowing = InstagramAPI.getFollowingOrFollowerList(userPK, cookie, minTime, maxTime, countUsers, "following");

                if (usersFollowing == null || Math.Abs(userFollowingCount - usersFollowing.Count) >= 3)//Sometimes a few followings are not fetched
                {
                    Console.Error.WriteLine($"Something went wrong while fetching following: {usersFollowing}");
                    return;
                }

                Console.WriteLine("Getting Followers...");
                usersFollowers = InstagramAPI.getFollowingOrFollowerList(userPK, cookie, minTime, maxTime, countUsers, "followers");

                if (usersFollowers == null || Math.Abs(userFollowerCount - usersFollowers.Count) >= 3)//Sometimes a few followers are not fetched
                {
                    Console.Error.WriteLine($"Something went wrong while fetching followers: {usersFollowers}");
                    return;
                }

                Directory.CreateDirectory(username);

                File.WriteAllText(followersFileName, Helper.dictionaryToJsonString(usersFollowers));
                File.WriteAllText(followingFileName, Helper.dictionaryToJsonString(usersFollowing));

                resultLines.Add($"\n{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}: Current Follower count: {userFollowerCount}, Current Following count: {userFollowingCount}");
                resultLines.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}: {username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(usersFollowing.Count < usersFollowingFile.Count ? usersFollowingFile.Count - usersFollowing.Count : usersFollowing.Count - usersFollowingFile.Count)} users");
                Console.WriteLine($"\n{username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(usersFollowing.Count < usersFollowingFile.Count ? usersFollowingFile.Count - usersFollowing.Count : usersFollowing.Count - usersFollowingFile.Count)} users");

                resultLines.AddRange(Helper.compareLists(usersFollowersFile, usersFollowers, username, "followers"));

                resultLines.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}: {(usersFollowers.Count < usersFollowersFile.Count ? usersFollowersFile.Count - usersFollowers.Count : usersFollowers.Count - usersFollowersFile.Count)} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");
                Console.WriteLine($"\n{(usersFollowers.Count < usersFollowersFile.Count ? usersFollowersFile.Count - usersFollowers.Count : usersFollowers.Count - usersFollowersFile.Count)} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");

                resultLines.AddRange(Helper.compareLists(usersFollowingFile, usersFollowing, username, "following"));

                bool hasNameChanges = false;

                foreach (var user in usersFollowersFile)
                {
                    if (usersFollowers.ContainsKey(user.Key) && usersFollowers[user.Key] != usersFollowersFile[user.Key])
                    {
                        hasNameChanges = true;
                        resultLines.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}: {user.Value} changed their username to {usersFollowers[user.Key]}");
                        Console.WriteLine($"{user.Value} changed their username to {usersFollowers[user.Key]}");
                    }
                }

                foreach (var user in usersFollowingFile)
                {
                    if (usersFollowing.ContainsKey(user.Key) && usersFollowing[user.Key] != usersFollowingFile[user.Key])
                    {
                        hasNameChanges = true;
                        resultLines.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}: {user.Value} changed their username to {usersFollowing[user.Key]}");
                        Console.WriteLine($"{user.Value} changed their username to {usersFollowing[user.Key]}");
                    }
                }

                if(!hasNameChanges){
                    resultLines.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm")}: There are no name changes");
                    Console.WriteLine("\nThere are no name changes");
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
                csrftoken = Helper.getCsrftoken(cookie);

                Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                postList = InstagramAPI.getPostList(cookie, csrftoken, 250, 500, countPost, username);

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

                Directory.CreateDirectory($"{username}/posts");
                File.WriteAllText($"{username}/posts.txt", postInfo);
                Console.WriteLine("\nDownloading posts...\n");

                string fileExtPattern = @"\.[a-zA-Z0-9]{2,5}(?=\?)";
                foreach (var post in postList)
                {
                    byte[] postBytes = Helper.getPostBytesFromUrl(post.Value)!;
                    Match match = Regex.Match(post.Value, fileExtPattern);
                    Helper.CreateNewFile($"{username}/posts/{post.Key}{match.Value}", postBytes);
                }

                Console.WriteLine($"Post of {username} saved in ./{username}/posts/ and the URLs were saved in ./{username}/posts.txt");
            }
            else if (option == "5")
            {
                cookie = UserInterface.getCookie(configFile);
                csrftoken = Helper.getCsrftoken(cookie);
                userPK = InstagramAPI.getUserID(username);

                Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n");

                if (userPK == null)
                {
                    Console.Error.WriteLine("Something went wrong while getting user PK");
                    return;
                }

                storiesList = InstagramAPI.getStoriesList(cookie, csrftoken, userPK);

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

                Directory.CreateDirectory($"{username}/stories");
                File.AppendAllText($"{username}/stories.txt", storiesInfo);

                Console.WriteLine("\nDownloading stories...\n");

                string fileExtPattern = @"\.[a-zA-Z0-9]{2,5}(?=\?)";
                foreach (var story in storiesList)
                {
                    byte[] storyBytes = Helper.getPostBytesFromUrl(story.Value)!;
                    Match match = Regex.Match(story.Value, fileExtPattern);
                    Helper.CreateNewFile($"{username}/stories/{story.Key}{match.Value}", storyBytes);
                }

                Console.WriteLine($"Stories of {username} saved in ./{username}/stories/ and the URLs were saved in ./{username}/stories.txt");
            }

            else if (option == "6")
            {
                Console.WriteLine("Getting user ID...\n");
                userPK = InstagramAPI.getUserID(username);

                Console.WriteLine($"\n{username} has the ID: {userPK}\n");
            }

            else if (option == "7")
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), username);
                Helper.OpenFolder(path);
            }
            return;
        }
    }
}
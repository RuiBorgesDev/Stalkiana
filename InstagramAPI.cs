using RestSharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Stalkiana_Console
{
    public static class InstagramAPI
    {
        private static RestClient client = new RestClient("https://www.instagram.com");

        private static void sleepRandom(int minTime, int maxTime)
        {
            Random rand = new Random();
            Thread.Sleep(rand.Next(minTime, maxTime));
        }

        public static Dictionary<string, string>? getFollowingOrFollowerList(string userPK, string cookie, int minTime, int maxTime, int count, string type, int totalPasses = 4)
        {
            var list = new Dictionary<string, string>();

            if (type != "followers" && type != "following")
            {
                Console.WriteLine("Invalid request type.");
                return null;
            }

            for (int pass = 1; pass <= totalPasses; pass++)
            {
                Console.WriteLine($"Starting {type} pass {pass}/{totalPasses}...");

                bool hasMore = true;
                string? after = null;

                while (hasMore)
                {
                    var request = new RestRequest($"/api/v1/friendships/{userPK}/{type}/", Method.Get);
                    request.AddQueryParameter("count", count);
                    if (!string.IsNullOrEmpty(after))
                    {
                        request.AddQueryParameter("max_id", after);
                    }
                    request.AddHeader("cookie", cookie);
                    request.AddHeader("x-ig-app-id", "936619743392459");
                    if (type == "followers")
                    {
                        request.AddQueryParameter("search_surface", "follow_list_page");
                    }

                    var response = client.Execute(request);

                    if (!response.IsSuccessful)
                    {
                        Console.Error.WriteLine($"Error fetching {type} on pass {pass} (maybe cookie is invalid)\nStatus code: {response.StatusCode}");
                        return null;
                    }

                    try
                    {
                        dynamic obj = JsonConvert.DeserializeObject(response.Content!)!;

                        hasMore = obj.has_more;
                        if (hasMore)
                        {
                            after = (string)obj.next_max_id;
                        }

                        foreach (dynamic user in obj.users)
                        {
                            list[(string)user.pk] = (string)user.username;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"Error fetching {type} on pass {pass}: {e.Message}");
                        return null;
                    }

                    sleepRandom(minTime, maxTime);
                }
            }

            return list;
        }

        public static Dictionary<string, string>? getStoriesList(string cookie, string csrftoken, string userPK)
        {
            var list = new Dictionary<string, string>();
            var request = new RestRequest("/graphql/query/", Method.Post);
            request.AddHeader("cookie", cookie);
            request.AddHeader("x-csrftoken", csrftoken);
            request.AddBody($"variables=%7B%22reel_ids_arr%22%3A%5B%22{userPK}%22%5D%7D&server_timestamps=true&doc_id=8010808625710156", "application/x-www-form-urlencoded");
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                Console.Error.WriteLine($"Error in get stories request (maybe cookie is invalid)\nStatus code: {response.StatusCode}");
                return null;
            }
            try
            {
                dynamic obj = JsonConvert.DeserializeObject(response.Content!)!;
                dynamic items = obj.data.xdt_api__v1__feed__reels_media.reels_media[0].items;
                foreach (dynamic item in items)
                {
                    string unixTime = item.taken_at;
                    string date = DateTimeOffset.FromUnixTimeSeconds(int.Parse(unixTime)).LocalDateTime.ToString("dd-MM-yyyy_HH\\hmm\\mss\\s");
                    string url = item.video_versions == null ? item.image_versions2.candidates[0].url : item.video_versions[0].url;
                    list[date] = url;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error in get stories request (maybe cookie is invalid): {e.Message}");
                return null;
            }
            return list;
        }

        public static Dictionary<string, string>? getPostList(string cookie, string csrftoken, int minTime, int maxTime, int count, string username)
        {
            var list = new Dictionary<string, string>();
            string? after = null;
            do
            {
                var request = new RestRequest("/graphql/query/", Method.Post);
                request.AddHeader("cookie", cookie);
                request.AddHeader("x-csrftoken", csrftoken);
                request.AddBody($"variables=%7B%22after%22%3A%22{(after == null ? "null" : after)}%22%2C%22before%22%3Anull%2C%22data%22%3A%7B%22count%22%3A{count}%2C%22include_reel_media_seen_timestamp%22%3Atrue%2C%22include_relationship_info%22%3Atrue%2C%22latest_besties_reel_media%22%3Atrue%2C%22latest_reel_media%22%3Atrue%7D%2C%22first%22%3A{count}%2C%22last%22%3Anull%2C%22username%22%3A%22{username}%22%2C%22__relay_internal__pv__PolarisIsLoggedInrelayprovider%22%3Atrue%2C%22__relay_internal__pv__PolarisShareSheetV3relayprovider%22%3Atrue%7D&server_timestamps=true&doc_id=9333503846778781", "application/x-www-form-urlencoded");
                var response = client.Execute(request);

                if (!response.IsSuccessful)
                {
                    Console.Error.WriteLine($"Error in get media request (maybe cookie is invalid)\nStatus code: {response.StatusCode}");
                    return null;
                }

                try
                {
                    dynamic obj = JsonConvert.DeserializeObject(response.Content!)!;

                    foreach (dynamic node in obj!.data.xdt_api__v1__feed__user_timeline_graphql_connection.edges)
                    {
                        string id = (string)node.node.id;
                        string url = node.node.video_versions == null ? node.node.image_versions2.candidates[0].url : node.node.video_versions[0].url;
                        list[id] = url;

                        if (node.node.carousel_media != null)
                        {
                            foreach (dynamic carousel_media in node.node.carousel_media)
                            {
                                string carouselId = (string)carousel_media.id;
                                string carouselUrl = carousel_media.video_versions == null ? carousel_media.image_versions2.candidates[0].url : carousel_media.video_versions[0].url;
                                list[carouselId] = carouselUrl;
                            }
                        }
                    }

                    after = after == list.Last().Key ? null : list.Last().Key;
                    if (string.IsNullOrEmpty(after)) break;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error in get media request (maybe cookie is invalid): {e.Message}");
                    return null;
                }
                sleepRandom(minTime, maxTime);
                Console.WriteLine($"{list.Count} media files fetched");
            } while (count > 0);

            return list;
        }
        public static string? getUserID(string username)
        {
            string userPK;
            var request = new RestRequest($"/{username}", Method.Get);

            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                Console.Error.WriteLine($"\nError in get user PK request\nStatus code: {response.StatusCode}");
                return null;
            }
            try
            {
                Match match = Regex.Match(response.Content!, @"profile_id\D*(\d+)");
                string result = match.Groups[1].Value;
                userPK = result;
                return userPK;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"\nError in get user PK request: {e.Message}");
                return null;
            }
        }

        public static (int followingCount, int followerCount) getFollowingAndFollowerCount(string userPK, string cookie, string csrftoken)
        {
            var request = new RestRequest("/graphql/query/", Method.Post);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("cookie", cookie);
            request.AddHeader("x-csrftoken", csrftoken);
            request.AddBody($"variables=%7B%22id%22%3A%22{userPK}%22%2C%22render_surface%22%3A%22PROFILE%22%7D&doc_id=7663723823674585", "application/x-www-form-urlencoded");
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                Console.Error.WriteLine($"Error in get following and followers count request (maybe cookie is invalid)\nStatus code: {response.StatusCode}");
                return (-1, -1);
            }

            try
            {
                dynamic obj = JsonConvert.DeserializeObject(response.Content!)!;
                return (obj.data.user.following_count, obj.data.user.follower_count);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error in get following and followers count request (maybe cookie is invalid): {e.Message}");
                return (-1, -1);
            }
        }

        public static string? getProfileImageUrl(string userPK, string csrftoken)
        {
            var request = new RestRequest("/api/graphql/", Method.Post);
            request.AddHeader("x-csrftoken", csrftoken);
            request.AddBody($"variables=%7B%22id%22%3A%22{userPK}%22%2C%22render_surface%22%3A%22PROFILE%22%7D&doc_id=26672929172408668", "application/x-www-form-urlencoded");
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                Console.Error.WriteLine($"Error in get profile image request (maybe cookie is invalid)\nStatus code: {response.StatusCode}");
                return null;
            }
            try
            {
                dynamic obj = JsonConvert.DeserializeObject(response.Content!)!;
                return obj.data.user.hd_profile_pic_url_info.url.ToString();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error in get profile image request (maybe cookie is invalid): {e.Message}");
                return null;
            }
        }
    }
}
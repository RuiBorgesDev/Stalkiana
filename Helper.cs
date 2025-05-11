using RestSharp;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stalkiana_Console
{
    public class Helper
    {
        public static Dictionary<string, string>? getDataFromFile(string filename)
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

        public static string dictionaryToJsonString(Dictionary<string, string> list)
        {
            var jsonArray = list.Select(kv => new { userPK = kv.Key, username = kv.Value }).ToArray();
            return JsonConvert.SerializeObject(jsonArray);
        }

        public static string getCsrftoken(string cookie)
        {
            string regex = @"(?<=csrftoken=)(\S+)(?=;)";
            Match match = Regex.Match(cookie, regex);
            string csrftoken = match.Value;
            return csrftoken;
        }
        public static void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = GetFileExplorerProcessName()
                };

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    startInfo.FileName = "explorer.exe";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    startInfo.FileName = "open";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    startInfo.FileName = "xdg-open";
                }
                else
                {
                    startInfo.FileName = folderPath;
                    startInfo.UseShellExecute = true;
                    startInfo.Arguments = null;
                }

                try
                {
                    Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Error: Folder not found at '{folderPath}'");
            }
        }

        public static string GetFileExplorerProcessName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "explorer.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "open";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "xdg-open";
            }
            return string.Empty;
        }

        public static byte[]? getPostBytesFromUrl(string url)
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
using System.IO;
using System.Threading;

public static class ProfileFileManager
{
    private static readonly object fileLock = new object();
    private static readonly string profilesFileName = "profiles.txt";

    private static string GetProfilesFilePath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string profilesDirectoryPath = Path.Combine(appDataPath, "VkLogs", "profiles");
        string profilesFilePath = Path.Combine(profilesDirectoryPath, profilesFileName);

        try
        {
            if (!Directory.Exists(profilesDirectoryPath))
            {
                Directory.CreateDirectory(profilesDirectoryPath);
            }

            if (!File.Exists(profilesFilePath))
            {
                using (File.Create(profilesFilePath)) { }
            }
        }
        catch (IOException)
        {
            // handle exception
        }

        return profilesFilePath;
    }

    public static bool AddProfile(string profileId)
    {
        bool profileAdded = false;

        while (true)
        {
            try
            {
                lock (fileLock)
                {
                    using (FileStream stream = new FileStream(GetProfilesFilePath(), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd();
                        if (!content.Contains(profileId))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                writer.Write(content + profileId + "\n");
                                writer.Flush();
                            }
                            profileAdded = true;
                        }
                    }
                }

                break; // Exit while loop if successful
            }
            catch (IOException)
            {
                // handle exception
                // wait for some time before retrying
                Thread.Sleep(1000); // 1 second
            }
        }

        return profileAdded;
    }

    public static bool RemoveProfile(string profileId)
    {
        bool profileRemoved = false;

        while (true)
        {
            try
            {
                lock (fileLock)
                {
                    using (FileStream stream = new FileStream(GetProfilesFilePath(), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd();
                        if (content.Contains(profileId))
                        {
                            content = content.Replace(profileId + "\n", "");
                            stream.Seek(0, SeekOrigin.Begin);
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                writer.Write(content);
                                writer.Flush();
                            }
                            profileRemoved = true;
                        }
                    }
                }

                break; // Exit while loop if successful
            }
            catch (IOException)
            {
                // handle exception
                // wait for some time before retrying
                Thread.Sleep(1000); // 1 second
            }
        }

        return profileRemoved;
    }

    public static void ClearProfiles()
    {
        while (true)
        {
            try
            {
                lock (fileLock)
                {
                    using (FileStream stream = new FileStream(GetProfilesFilePath(), FileMode.Create, FileAccess.Write, FileShare.None))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write("");
                        writer.Flush();
                    }
                    break; // выходим из цикла если удалось успешно обратиться к файлу и очистить его
                }
            }
            catch (IOException)
            {
                // handle exception
                // wait for some time before retrying
                Thread.Sleep(1000); // 1 second
            }
        }
    }

    public static bool IsProfileUsed(string profileId)
    {
        bool profileUsed = false;

        while (true)
        {
            try
            {
                lock (fileLock)
                {
                    using (FileStream stream = new FileStream(GetProfilesFilePath(), FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        StreamReader reader = new StreamReader(stream);
                        string content = reader.ReadToEnd();
                        profileUsed = content.Contains(profileId);
                    }
                }

                break; // Exit while loop if successful
            }
            catch (IOException)
            {
                // handle exception
                // wait for some time before retrying
                Thread.Sleep(1000); // 1 second
            }
        }

        return profileUsed;
    }
}










namespace DirectoryScanner.Tests;

using System.Text;

class TestUtils
{
    private static Random _random = new Random();
    private const int MIN_NAME_LENGTH = 5;
    private const int MAX_NAME_LENGTH = 20;
    private const string CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string RandomString(int minLength, int maxLength)
    {
        int length = _random.Next(minLength, maxLength + 1);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(CHARS[_random.Next(0, CHARS.Length)]);
        }

        return sb.ToString();
    }

    public static FakeDirectoryInfo RandomDirectoryInfo(int maxFiles, int maxDirectories, int maxDepth, int curDepth = 0)
    {
        int filesCount = _random.Next(0, maxFiles + 1);
        List<FakeFileInfo> files = new List<FakeFileInfo>();
        for (int i = 0; i < filesCount; i++)
        {
            string name = RandomString(MIN_NAME_LENGTH, MAX_NAME_LENGTH);
            long size = _random.Next(1, 1 << 16);
            files.Add(new FakeFileInfo(name, null, size));
        }

        List<FakeDirectoryInfo> directories = new List<FakeDirectoryInfo>();
        if (curDepth < maxDepth)
        {
            int directoriesCount = _random.Next(0, maxDirectories + 1);
            for (int i = 0; i < directoriesCount; i++)
            {
                directories.Add(RandomDirectoryInfo(maxFiles, maxDirectories, maxDepth, curDepth + 1));
            }
        }

        return new FakeDirectoryInfo(RandomString(MIN_NAME_LENGTH, MAX_NAME_LENGTH), null, files, directories);
    }
}

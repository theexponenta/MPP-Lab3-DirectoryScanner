namespace DirectoryScanner.Core.Exceptions;

public class DirectoryDoesNotExistException : Exception
{
    public DirectoryDoesNotExistException(string path) 
        : base($"Directory does not exist \"{path}\"")
    {
    }
}

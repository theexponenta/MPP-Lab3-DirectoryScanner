namespace DirectoryScanner.Core.Exceptions;

internal class FileAlreadyExistsException : Exception
{
    public FileAlreadyExistsException(string name) 
        : base($"File \"{name}\" already exists")
    {
    }
}

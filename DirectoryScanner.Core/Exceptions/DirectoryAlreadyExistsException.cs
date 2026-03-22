namespace DirectoryScanner.Core.Exceptions;

internal class DirectoryAlreadyExistsException : Exception
{
    public DirectoryAlreadyExistsException(string name) 
        : base($"Directory \"{name}\" already exists")
    {
    }
}

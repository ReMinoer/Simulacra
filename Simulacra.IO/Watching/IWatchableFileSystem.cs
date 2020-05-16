namespace Simulacra.IO.Watching
{
    public interface IWatchableFileSystem
    {
        bool IsPathRooted(string path);
        bool IsExplicitFolderPath(string path);
        string GetDirectoryName(string path);

        string UniqueFile(string path);
        string UniqueFolder(string path);

        bool FileExists(string path);
        bool FolderExists(string path);

        IFileSystemWatcher GetWatcher(string folderPath);
    }

    public interface IWatchableFileSystem<out TWatcher> : IWatchableFileSystem
        where TWatcher : IFileSystemWatcher
    {
        new TWatcher GetWatcher(string folderPath);
    }
}
namespace Simulacra.IO.Watching
{
    public interface IWatchableFileSystem
    {
        bool PathsCaseSensitive { get; }

        bool IsPathRooted(string path);
        bool IsExplicitFolderPath(string path);
        string GetFolderPath(string path);

        string UniqueFile(string path);
        string UniqueFolder(string path);

        bool FileExists(string path);
        bool FolderExists(string path);

        IFileSystemWatcher GetWatcher(string path);
    }

    public interface IWatchableFileSystem<out TWatcher> : IWatchableFileSystem
        where TWatcher : IFileSystemWatcher
    {
        new TWatcher GetWatcher(string folderPath);
    }
}
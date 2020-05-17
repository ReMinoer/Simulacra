namespace Simulacra.IO.Watching
{
    public interface IFileSystemWatcherProvider
    {
        IFileSystemWatcher GetWatcher(string path);
    }

    public interface IFileSystemWatcherProvider<out TWatcher> : IFileSystemWatcherProvider
        where TWatcher : IFileSystemWatcher
    {
        new TWatcher GetWatcher(string path);
    }
}
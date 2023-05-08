using System.Collections.Generic;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public interface IWatchableFileSystem : IPathSystem
    {
        bool FileExists(string path);
        bool FolderExists(string path);

        IEnumerable<string> GetFiles(PathPattern pathPattern);
        IEnumerable<string> GetFolders(PathPattern pathPattern);
    }
}
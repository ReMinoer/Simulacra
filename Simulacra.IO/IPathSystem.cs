namespace Simulacra.IO
{
    public interface IPathSystem
    {
        bool PathsCaseSensitive { get; }
        char[] Separators { get; }
        char AbsoluteSeparator { get; }
        char RelativeSeparator { get; }
        char[] InvalidPathChars { get; }
        bool IsPathRooted(string path);
    }
}
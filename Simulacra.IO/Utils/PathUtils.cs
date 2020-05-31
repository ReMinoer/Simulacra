using System;
using System.IO;
using System.Linq;

namespace Simulacra.IO.Utils
{
    static public class PathUtils
    {
        static public char AbsoluteSeparator => Path.DirectorySeparatorChar;
        static public char RelativeSeparator => Path.AltDirectorySeparatorChar;

        static public bool IsValidPath(string path) => IsValidPathInternal(path) && (IsValidAbsolutePathInternal(path) || IsValidRelativePathInternal(path));
        static public bool IsValidAbsolutePath(string path) => IsValidPathInternal(path) && IsValidAbsolutePathInternal(path);
        static public bool IsValidRelativePath(string path) => IsValidPathInternal(path) && IsValidRelativePathInternal(path);

        static private bool IsValidPathInternal(string path) => !Path.GetInvalidPathChars().Any(path.Contains);
        static private bool IsValidAbsolutePathInternal(string path) => Path.IsPathRooted(path);
        static private bool IsValidRelativePathInternal(string path) => !Path.IsPathRooted(path) && path[0] != AbsoluteSeparator && path[0] != RelativeSeparator;

        static public string Normalize(string path) => Normalize(path, out _);
        static private string Normalize(string path, out char separator)
        {
            if (!IsValidPath(path))
                throw new ArgumentException();

            bool isAbsolute = Path.IsPathRooted(path);
            separator = isAbsolute ? AbsoluteSeparator : RelativeSeparator;
            char otherSeparator = isAbsolute ? RelativeSeparator : AbsoluteSeparator;

            // Use unique separator
            return path.Replace(otherSeparator, separator);
        }

        static public string UniqueFile(string path) => UniqueFile(path, PathCaseComparison.EnvironmentDefault);
        static public string UniqueFile(string path, PathCaseComparison caseComparison)
        {
            if (!IsValidPath(path))
                throw new ArgumentException();
            if (IsExplicitFolderPath(path))
                throw new ArgumentException();

            // Normalize
            path = Normalize(path);

            // Change case if necessary
            return PathComparer.ApplyCaseComparison(Normalize(path), caseComparison);
        }

        static public string UniqueFolder(string path) => UniqueFolder(path, PathCaseComparison.EnvironmentDefault);
        static public string UniqueFolder(string path, PathCaseComparison caseComparison)
        {
            if (!IsValidPath(path))
                throw new ArgumentException();

            // Normalize
            path = Normalize(path, out char separator);

            // Change case if necessary
            path = PathComparer.ApplyCaseComparison(path, caseComparison);

            // Add end separator
            if (!IsExplicitFolderPath(path, separator))
                path += separator;

            return path;
        }

        static public string TrimEndSeparator(string path) => path.TrimEnd(AbsoluteSeparator, RelativeSeparator);
        static public string GetFolderPath(string path)
        {
            if (!IsValidPath(path))
                throw new ArgumentException();

            string directoryName = Path.GetDirectoryName(TrimEndSeparator(path));
            if (directoryName == null)
                return null;

            return UniqueFolder(directoryName, PathCaseComparison.IgnoreCase);
        }

        static public string GetName(string path) => Path.GetFileName(TrimEndSeparator(path));

        static public bool IsExplicitFolderPath(string path) => IsExplicitAbsoluteFolderPath(path) || IsExplicitRelativeFolderPath(path);
        static public bool IsExplicitAbsoluteFolderPath(string path) => IsExplicitFolderPath(path, AbsoluteSeparator);
        static public bool IsExplicitRelativeFolderPath(string path) => IsExplicitFolderPath(path, RelativeSeparator);
        static private bool IsExplicitFolderPath(string path, char endSeparator) => path[path.Length - 1] == endSeparator;
    }
}
using System;
using System.IO;

namespace Simulacra.IO.Utils
{
    static public class PathUtils
    {
        static public char AbsoluteSeparator => Path.DirectorySeparatorChar;
        static public char RelativeSeparator => Path.AltDirectorySeparatorChar;

        static public string Normalize(string path) => Normalize(path, out _);
        static private string Normalize(string path, out char separator)
        {
            bool isAbsolute = Path.IsPathRooted(path);
            separator = isAbsolute ? AbsoluteSeparator : RelativeSeparator;
            char otherSeparator = isAbsolute ? RelativeSeparator : AbsoluteSeparator;

            // Use unique separator
            return path.Replace(otherSeparator, separator);
        }

        static public string UniqueFile(string path) => UniqueFile(path, PathCaseComparison.EnvironmentDefault);
        static public string UniqueFile(string path, PathCaseComparison caseComparison)
        {
            if (IsExplicitFolderPath(path))
                throw new ArgumentException();

            // Normalize
            path = Normalize(path);

            // Change case if necessary
            return PathComparer.TransformCase(Normalize(path), caseComparison);
        }

        static public string UniqueFolder(string path) => UniqueFolder(path, PathCaseComparison.EnvironmentDefault);
        static public string UniqueFolder(string path, PathCaseComparison caseComparison)
        {
            // Normalize
            path = Normalize(path, out char separator);

            // Change case if necessary
            path = PathComparer.TransformCase(path, caseComparison);

            // Add end separator
            if (!IsExplicitFolderPath(path, separator))
                path += separator;

            return path;
        }

        static public string TrimEndSeparator(string path) => path.TrimEnd(AbsoluteSeparator, RelativeSeparator);

        static public bool IsExplicitFolderPath(string path) => IsExplicitAbsoluteFolderPath(path) || IsExplicitRelativeFolderPath(path);
        static public bool IsExplicitAbsoluteFolderPath(string path) => IsExplicitFolderPath(path, AbsoluteSeparator);
        static public bool IsExplicitRelativeFolderPath(string path) => IsExplicitFolderPath(path, RelativeSeparator);
        static private bool IsExplicitFolderPath(string path, char endSeparator) => path[path.Length - 1] == endSeparator;
    }
}
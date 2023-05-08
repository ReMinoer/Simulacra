using System;
using System.Linq;
using Simulacra.IO.Utils;

namespace Simulacra.IO
{
    static public class PathSystemExtension
    {
        static public bool IsValidPath(this IPathSystem pathSystem, string path) => IsValidPathInternal(pathSystem, path) && (IsValidAbsolutePathInternal(pathSystem, path) || IsValidRelativePathInternal(pathSystem, path));
        static public bool IsValidAbsolutePath(this IPathSystem pathSystem, string path) => IsValidPathInternal(pathSystem, path) && IsValidAbsolutePathInternal(pathSystem, path);
        static public bool IsValidRelativePath(this IPathSystem pathSystem, string path) => IsValidPathInternal(pathSystem, path) && IsValidRelativePathInternal(pathSystem, path);

        static private bool IsValidPathInternal(IPathSystem pathSystem, string path) => !pathSystem.InvalidPathChars.Any(path.Contains);
        static private bool IsValidAbsolutePathInternal(IPathSystem pathSystem, string path) => pathSystem.IsPathRooted(path);
        static private bool IsValidRelativePathInternal(IPathSystem pathSystem, string path) => !pathSystem.IsPathRooted(path) && path[0] != pathSystem.AbsoluteSeparator && path[0] != pathSystem.RelativeSeparator;

        static public string Normalize(this IPathSystem pathSystem, string path) => Normalize(pathSystem, path, out _);
        static private string Normalize(IPathSystem pathSystem, string path, out char separator)
        {
            if (!IsValidPath(pathSystem, path))
                throw new ArgumentException();

            bool isAbsolute = pathSystem.IsPathRooted(path);
            separator = isAbsolute ? pathSystem.AbsoluteSeparator : pathSystem.RelativeSeparator;
            char otherSeparator = isAbsolute ? pathSystem.RelativeSeparator : pathSystem.AbsoluteSeparator;

            // Use unique separator
            return path.Replace(otherSeparator, separator);
        }

        static public string UniqueFile(this IPathSystem pathSystem, string path) => UniqueFile(pathSystem, path, PathCaseComparison.SystemDefault);
        static public string UniqueFile(this IPathSystem pathSystem, string path, PathCaseComparison caseComparison)
        {
            if (!IsValidPath(pathSystem, path))
                throw new ArgumentException();
            if (IsExplicitFolderPath(pathSystem, path))
                throw new ArgumentException();

            // Normalize
            path = Normalize(pathSystem, path);

            // Change case if necessary
            return PathComparer.ApplyCaseComparison(Normalize(pathSystem, path), pathSystem, caseComparison);
        }

        static public string UniqueFolder(this IPathSystem pathSystem, string path) => UniqueFolder(pathSystem, path, PathCaseComparison.SystemDefault);
        static public string UniqueFolder(this IPathSystem pathSystem, string path, PathCaseComparison caseComparison)
        {
            if (!IsValidPath(pathSystem, path))
                throw new ArgumentException();

            // Normalize
            path = Normalize(pathSystem, path, out char separator);

            // Change case if necessary
            path = PathComparer.ApplyCaseComparison(path, pathSystem, caseComparison);

            // Add end separator
            if (!IsExplicitFolderPath(path, separator))
                path += separator;

            return path;
        }

        static public string TrimEndSeparator(this IPathSystem pathSystem, string path) => path.TrimEnd(pathSystem.Separators);

        static public string GetFolderPath(this IPathSystem pathSystem, string path)
        {
            if (!IsValidPath(pathSystem, path))
                throw new ArgumentException();
            
            string trimmedPath = TrimEndSeparator(pathSystem, path);

            int lastSeparatorIndex = trimmedPath.LastIndexOfAny(pathSystem.Separators);
            if (lastSeparatorIndex == -1)
                return null;

            string folderPath = trimmedPath.Substring(0, lastSeparatorIndex);
            return UniqueFolder(pathSystem, folderPath, PathCaseComparison.IgnoreCase);
        }

        static public string GetName(this IPathSystem pathSystem, string path)
        {
            if (!IsValidPath(pathSystem, path))
                throw new ArgumentException();

            string trimmedPath = TrimEndSeparator(pathSystem, path);

            int lastSeparatorIndex = trimmedPath.LastIndexOfAny(pathSystem.Separators);
            if (lastSeparatorIndex == -1)
                return trimmedPath;

            return trimmedPath.Substring(lastSeparatorIndex + 1);
        }

        static public string Combine(this IPathSystem pathSystem, string left, string right)
        {
            if (!IsValidPath(pathSystem, left))
                throw new ArgumentException();
            if (!IsValidRelativePath(pathSystem, right))
                throw new ArgumentException();

            string normalizedLeft = Normalize(pathSystem, left, out char separator);
            if (!normalizedLeft.EndsWith(separator.ToString()))
                normalizedLeft += separator;

            return Normalize(pathSystem, normalizedLeft + right);
        }

        static public bool IsExplicitFolderPath(this IPathSystem pathSystem, string path) => IsExplicitAbsoluteFolderPath(pathSystem, path) || IsExplicitRelativeFolderPath(pathSystem, path);
        static public bool IsExplicitAbsoluteFolderPath(this IPathSystem pathSystem, string path) => IsExplicitFolderPath(path, pathSystem.AbsoluteSeparator);
        static public bool IsExplicitRelativeFolderPath(this IPathSystem pathSystem, string path) => IsExplicitFolderPath(path, pathSystem.RelativeSeparator);
        static private bool IsExplicitFolderPath(string path, char endSeparator) => path[path.Length - 1] == endSeparator;
    }
}
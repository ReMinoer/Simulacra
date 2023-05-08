using System;
using System.Collections.Generic;

namespace Simulacra.IO.Utils
{
    public enum PathCaseComparison
    {
        SystemDefault,
        RespectCase,
        IgnoreCase
    }

    public enum FolderPathEquality
    {
        RespectAmbiguity,
        RequireExplicitEndSeparator
    }

    public class PathComparer : IEqualityComparer<string>, IComparer<string>
    {
        public IPathSystem PathSystem { get; }
        public PathCaseComparison CaseComparison { get; }
        public FolderPathEquality FolderEquality { get; }
        
        public PathComparer()
            : this(IO.PathSystem.Instance)
        {
        }

        public PathComparer(IPathSystem pathSystem)
            : this(pathSystem, PathCaseComparison.SystemDefault, FolderPathEquality.RespectAmbiguity)
        {
        }

        public PathComparer(IPathSystem pathSystem,
            PathCaseComparison caseComparison,
            FolderPathEquality folderEquality)
        {
            PathSystem = pathSystem;
            CaseComparison = caseComparison;
            FolderEquality = folderEquality;
        }

        public bool Equals(string x, string y) => Equals(x, y, PathSystem, CaseComparison, FolderEquality);
        public int Compare(string x, string y) => Compare(x, y, PathSystem, CaseComparison, FolderEquality);
        public int GetHashCode(string obj) => ApplyCaseComparison(ApplyFolderEquality(PathSystem.Normalize(obj), PathSystem, FolderEquality), PathSystem, CaseComparison).GetHashCode();

        static public bool Equals(string first, string second, IPathSystem pathSystem, PathCaseComparison caseComparison, FolderPathEquality folderEquality)
        {
            if (first == null && second == null)
                return true;
            if (first == null ^ second == null)
                return false;

            return string.Equals(
                ApplyFolderEquality(pathSystem.Normalize(first), pathSystem, folderEquality),
                ApplyFolderEquality(pathSystem.Normalize(second), pathSystem, folderEquality),
                GetStringComparison(pathSystem, caseComparison));
        }

        static public int Compare(string first, string second, IPathSystem pathSystem, PathCaseComparison caseComparison, FolderPathEquality folderEquality)
        {
            if (first == null && second == null)
                return 0;
            if (first == null)
                return -1;
            if (second == null)
                return 1;

            return string.Compare(
                ApplyFolderEquality(pathSystem.Normalize(first), pathSystem, folderEquality),
                ApplyFolderEquality(pathSystem.Normalize(second), pathSystem, folderEquality),
                GetStringComparison(pathSystem, caseComparison));
        }

        static public string ApplyFolderEquality(string path, IPathSystem pathSystem, FolderPathEquality folderEquality)
        {
            if (path == null)
                throw new ArgumentNullException();

            if (folderEquality == FolderPathEquality.RespectAmbiguity)
                path = pathSystem.TrimEndSeparator(path);

            return path;
        }

        static public string ApplyCaseComparison(string path, IPathSystem pathSystem, PathCaseComparison caseComparison)
        {
            if (path == null)
                throw new ArgumentNullException();

            switch (caseComparison)
            {
                case PathCaseComparison.SystemDefault:
                    if (pathSystem.PathsCaseSensitive)
                        goto default;
                    goto case PathCaseComparison.IgnoreCase;
                case PathCaseComparison.IgnoreCase:
                    return path.ToLowerInvariant();
                default:
                    return path;
            }
        }

        static public StringComparison GetStringComparison(IPathSystem pathSystem, PathCaseComparison caseComparison)
        {
            switch (caseComparison)
            {
                case PathCaseComparison.SystemDefault:
                    return pathSystem.PathsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                case PathCaseComparison.RespectCase:
                    return StringComparison.Ordinal;
                case PathCaseComparison.IgnoreCase:
                    return StringComparison.OrdinalIgnoreCase;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
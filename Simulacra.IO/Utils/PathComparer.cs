using System;
using System.Collections.Generic;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Utils
{
    public enum PathCaseComparison
    {
        EnvironmentDefault,
        RespectCase,
        IgnoreCase
    }

    public enum FolderPathEquality
    {
        RespectAmbiguity,
        ExplicitEndSeparator
    }

    public class PathComparer : IEqualityComparer<string>, IComparer<string>
    {
        public PathCaseComparison CaseComparison { get; }
        public FolderPathEquality FolderEquality { get; }

        public PathComparer()
            : this(PathCaseComparison.EnvironmentDefault, FolderPathEquality.RespectAmbiguity)
        {
        }

        public PathComparer(PathCaseComparison caseComparison, FolderPathEquality folderEquality)
        {
            CaseComparison = caseComparison;
            FolderEquality = folderEquality;
        }

        public bool Equals(string x, string y) => Equals(x, y, CaseComparison, FolderEquality);
        public int Compare(string x, string y) => Compare(x, y, CaseComparison, FolderEquality);
        public int GetHashCode(string obj) => TransformCase(TransformFolder(PathUtils.Normalize(obj), FolderEquality), CaseComparison).GetHashCode();

        static public bool Equals(string first, string second, PathCaseComparison caseComparison, FolderPathEquality folderEquality)
        {
            return string.Equals(TransformFolder(PathUtils.Normalize(first), folderEquality), TransformFolder(PathUtils.Normalize(second), folderEquality), GetStringComparison(caseComparison));
        }

        static public int Compare(string first, string second, PathCaseComparison caseComparison, FolderPathEquality folderEquality)
        {
            return string.Compare(TransformFolder(PathUtils.Normalize(first), folderEquality), TransformFolder(PathUtils.Normalize(second), folderEquality), GetStringComparison(caseComparison));
        }

        static public string TransformFolder(string path, FolderPathEquality folderEquality)
        {
            if (folderEquality == FolderPathEquality.RespectAmbiguity)
                path = path.TrimEnd(PathUtils.AbsoluteSeparator, PathUtils.RelativeSeparator);

            return path;
        }

        static public string TransformCase(string path, PathCaseComparison caseComparison)
        {
            switch (caseComparison)
            {
                case PathCaseComparison.EnvironmentDefault:
                    if (IsEnvironmentCaseSensitive())
                        goto case PathCaseComparison.IgnoreCase;
                    else goto default;
                case PathCaseComparison.IgnoreCase:
                    return path.ToLowerInvariant();
                default:
                    return path;
            }
        }

        static public StringComparison GetStringComparison(PathCaseComparison caseComparison)
        {
            switch (caseComparison)
            {
                case PathCaseComparison.EnvironmentDefault:
                    return IsEnvironmentCaseSensitive() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                case PathCaseComparison.RespectCase:
                    return StringComparison.Ordinal;
                case PathCaseComparison.IgnoreCase:
                    return StringComparison.OrdinalIgnoreCase;
                default:
                    throw new NotSupportedException();
            }
        }

        static public bool IsEnvironmentCaseSensitive()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix: return true;
                default: return false;
            }
        }
    }
}
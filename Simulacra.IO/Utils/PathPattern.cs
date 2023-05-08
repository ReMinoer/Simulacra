using System;
using System.Text.RegularExpressions;

namespace Simulacra.IO.Utils
{
    public class PathPattern
    {
        public IPathSystem PathSystem { get; }

        public string Pattern { get; }
        public bool CaseSensitive { get; }

        public string FolderPath { get; }
        public string NamePattern { get; }

        private readonly Regex _regex;

        public PathPattern(string pattern, IPathSystem pathSystem, bool caseSensitive = true)
        {
            if (!IsValid(pattern, pathSystem))
                throw new ArgumentException();
            
            PathSystem = pathSystem;

            Pattern = pattern;
            CaseSensitive = caseSensitive;

            FolderPath = PathSystem.GetFolderPath(pattern);
            NamePattern = PathSystem.GetName(pattern);

            if (IsSimplePath(NamePattern))
                return;

            var regexOptions = RegexOptions.Compiled;
            if (!caseSensitive)
                regexOptions |= RegexOptions.IgnoreCase;

            _regex = new Regex(ConvertPatternToRegex(Pattern, PathSystem), regexOptions);
        }

        static private readonly Regex ValidPatternRegex = new Regex(@"^[\/\\]*(?:[^*?\/\\]*[\/\\])*[^\/\\]+[\/\\]?$", RegexOptions.Compiled);

        static public bool IsValid(string pattern, IPathSystem pathSystem)
        {
            return pathSystem.IsValidPath(pattern) && ValidPatternRegex.IsMatch(pattern);
        }

        public bool Match(string path)
        {
            return _regex?.IsMatch(PathSystem.TrimEndSeparator(path)) ?? MatchSimplePath(path, Pattern, PathSystem, CaseSensitive);
        }

        static public bool Match(string path, string pattern, IPathSystem pathSystem, bool caseSensitive = true)
        {
            if (!IsValid(pattern, pathSystem))
                throw new ArgumentException();

            if (IsSimplePath(pattern))
                return MatchSimplePath(path, pattern, pathSystem, caseSensitive);

            var regexOptions = RegexOptions.None;
            if (!caseSensitive)
                regexOptions |= RegexOptions.IgnoreCase;

            return Regex.IsMatch(pathSystem.TrimEndSeparator(path), ConvertPatternToRegex(pattern, pathSystem), regexOptions);
        }

        static private bool IsSimplePath(string namePattern)
        {
            return !namePattern.Contains("*") && !namePattern.Contains("?");
        }

        static private bool MatchSimplePath(string path, string pattern, IPathSystem pathSystem, bool caseSensitive = true)
        {
            PathCaseComparison pathCaseComparison = caseSensitive ? PathCaseComparison.RespectCase : PathCaseComparison.IgnoreCase;
            return PathComparer.Equals(path, pattern, pathSystem, pathCaseComparison, FolderPathEquality.RespectAmbiguity);
        }

        static private string ConvertPatternToRegex(string pattern, IPathSystem pathSystem)
        {
            return "^"
                + pathSystem.TrimEndSeparator(pattern)
                    .Replace(@"/", @"\/")
                    .Replace(@"\", @"\\")
                    .Replace(@".", @"\.")
                    .Replace("*", @"[^\/\\]*")
                    .Replace("?", @"[^\/\\]?")
                + "$";
        }
    }
}
using System;
using System.Text.RegularExpressions;

namespace Simulacra.IO.Utils
{
    public class PathPattern
    {
        public string Pattern { get; }
        public bool CaseSensitive { get; }

        public string FolderPath { get; }
        public string NamePattern { get; }

        private readonly Regex _regex;

        public PathPattern(string pattern, bool caseSensitive = true)
        {
            if (!IsValid(pattern))
                throw new ArgumentException();

            Pattern = pattern;
            CaseSensitive = caseSensitive;

            FolderPath = PathUtils.GetFolderPath(pattern);
            NamePattern = PathUtils.GetName(pattern);

            if (IsSimplePath(NamePattern))
                return;

            var regexOptions = RegexOptions.Compiled;
            if (!caseSensitive)
                regexOptions |= RegexOptions.IgnoreCase;

            _regex = new Regex(ConvertPatternToRegex(Pattern), regexOptions);
        }

        static private readonly Regex ValidPatternRegex = new Regex(@"^[\/\\]*(?:[^*?\/\\]*[\/\\])*[^\/\\]+[\/\\]?$", RegexOptions.Compiled);

        static public bool IsValid(string pattern)
        {
            return PathUtils.IsValidPath(pattern) && ValidPatternRegex.IsMatch(pattern);
        }

        public bool Match(string path)
        {
            return _regex?.IsMatch(PathUtils.TrimEndSeparator(path)) ?? MatchSimplePath(path, Pattern, CaseSensitive);
        }

        static public bool Match(string path, string pattern, bool caseSensitive = true)
        {
            if (!IsValid(pattern))
                throw new ArgumentException();

            if (IsSimplePath(pattern))
                return MatchSimplePath(path, pattern, caseSensitive);

            var regexOptions = RegexOptions.None;
            if (!caseSensitive)
                regexOptions |= RegexOptions.IgnoreCase;

            return Regex.IsMatch(PathUtils.TrimEndSeparator(path), ConvertPatternToRegex(pattern), regexOptions);
        }

        static private bool IsSimplePath(string namePattern)
        {
            return !namePattern.Contains("*") && !namePattern.Contains("?");
        }

        static private bool MatchSimplePath(string path, string pattern, bool caseSensitive = true)
        {
            PathCaseComparison pathCaseComparison = caseSensitive ? PathCaseComparison.RespectCase : PathCaseComparison.IgnoreCase;
            return PathComparer.Equals(path, pattern, pathCaseComparison, FolderPathEquality.RespectAmbiguity);
        }

        static private string ConvertPatternToRegex(string pattern)
        {
            return "^"
                + PathUtils.TrimEndSeparator(pattern)
                    .Replace(@"/", @"\/")
                    .Replace(@"\", @"\\")
                    .Replace(@".", @"\.")
                    .Replace("*", @"[^\/\\]*")
                    .Replace("?", @"[^\/\\]?")
                + "$";
        }
    }
}

using System.Text.RegularExpressions;

namespace YooAsset.Editor
{

    static class Rule
    {

        /// <summary>
        /// Parse assetPath and replace all elements that match this.path regex
        /// with the <paramref name="name"/>
        /// Returns null if this.path or  <paramref name="name"/> is empty.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ParseReplacement(string assetPath, string name, string regex)
        {
            if (string.IsNullOrWhiteSpace(regex) || string.IsNullOrWhiteSpace(name))
                return null;

            var cleanedName = name.Trim();

            // Parse path elements.
            var replacement = RuleImportRegex.ParsePath(assetPath, cleanedName);
            // Parse this.path regex.
            //if (matchType == AddressableImportRuleMatchType.Regex)
            {
                string pathRegex = regex;
                replacement = Regex.Replace(assetPath, pathRegex, replacement);
            }
            return replacement;
        }


    }

    /// <summary>
    /// Helper class for regex replacement.
    /// </summary>
    static class RuleImportRegex
    {
        const string pathregex = @"\$\{PATH\[\-{0,1}\d{1,3}\]\}"; // ie: ${PATH[0]} ${PATH[-1]}

        static public string[] GetPathArray(string path)
        {
            return path.Split('/');
        }

        static public string GetPathAtArray(string path, int idx)
        {
            return GetPathArray(path)[idx];
        }

        /// <summary>
        /// Parse assetPath and replace all matched path elements (i.e. `${PATH[0]}`)
        /// with a specified replacement string.
        /// </summary>
        static public string ParsePath(string assetPath, string replacement)
        {
            var _path = assetPath;
            int i = 0;
            var slashSplit = _path.Split('/');
            var len = slashSplit.Length - 1;
            var matches = Regex.Matches(replacement, pathregex);
            string[] parsedMatches = new string[matches.Count];
            foreach (var match in matches)
            {
                string v = match.ToString();
                var sidx = v.IndexOf('[') + 1;
                var eidx = v.IndexOf(']');
                int idx = int.Parse(v.Substring(sidx, eidx - sidx));
                while (idx > len)
                {
                    idx -= len;
                }
                while (idx < 0)
                {
                    idx += len;
                }
                //idx = Mathf.Clamp(idx, 0, slashSplit.Length - 1);
                parsedMatches[i++] = GetPathAtArray(_path, idx);
            }

            i = 0;
            var splitpath = Regex.Split(replacement, pathregex);
            string finalPath = string.Empty;
            foreach (var split in splitpath)
            {
                finalPath += splitpath[i];
                if (i < parsedMatches.Length)
                {
                    finalPath += parsedMatches[i];
                }
                i++;
            }
            return finalPath;
        }
    }
}

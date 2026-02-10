using System.Collections.Generic;

namespace SchoolAdBlocker.Core.Blocklists
{
    public sealed class HostsParser
    {
        public IEnumerable<string> ParseDomains(string hostsText)
        {
            var result = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var rawLine in hostsText.Split('\n'))
            {
                var line = rawLine.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("#") || line.StartsWith("!")) continue;

                var commentIndex = line.IndexOf('#');
                if (commentIndex >= 0) line = line.Substring(0, commentIndex).Trim();
                if (line.Length == 0) continue;

                var parts = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                var ip = parts[0];
                if (ip != "0.0.0.0" && ip != "127.0.0.1") continue;

                for (var i = 1; i < parts.Length; i++)
                {
                    var domain = parts[i].Trim().TrimEnd('.');
                    if (domain.Length > 0)
                    {
                        result.Add(domain);
                    }
                }
            }

            return result;
        }
    }
}
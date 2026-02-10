using System.Collections.Generic;

namespace SchoolAdBlocker.Core.Filtering
{
    public static class DomainMatcher
    {
        public static bool ShouldBlockHost(string host, HashSet<string> domains)
        {
            if (string.IsNullOrWhiteSpace(host)) return false;

            host = host.Trim().TrimEnd('.').ToLowerInvariant();

            if (domains.Contains(host)) return true;

            var parts = host.Split('.');
            for (int i = 1; i < parts.Length; i++)
            {
                var suffix = parts[i];
                for (int j = i + 1; j < parts.Length; j++)
                    suffix += "." + parts[j];

                if (domains.Contains(suffix))
                    return true;
            }

            return false;
        }
    }
}
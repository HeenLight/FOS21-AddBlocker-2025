using System.Collections.Generic;
using System.Threading;

namespace SchoolAdBlocker.Core.Filtering
{
    public sealed class RuleEngine
    {
        private HashSet<string> _domains = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        public bool ShouldBlockHost(string host)
        {
            var snapshot = Volatile.Read(ref _domains);
            return DomainMatcher.ShouldBlockHost(host, snapshot);
        }

        public void UpdateDomains(IEnumerable<string> domains)
        {
            var newSet = new HashSet<string>(domains, System.StringComparer.OrdinalIgnoreCase);
            Interlocked.Exchange(ref _domains, newSet);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolAdBlocker.Core.Blocklists
{
    public sealed class BlocklistUpdater
    {
        private readonly BlocklistCacheStore _cache;
        private readonly HostsParser _parser;
        private readonly RemoteListFetcher _fetcher;

        public BlocklistUpdater(BlocklistCacheStore cache, HostsParser parser, RemoteListFetcher fetcher)
        {
            _cache = cache;
            _parser = parser;
            _fetcher = fetcher;
        }

        public IReadOnlyCollection<string> LoadFromCacheOrEmpty()
        {
            var domains = new HashSet<string>(_cache.ReadDomains(), StringComparer.OrdinalIgnoreCase);
            foreach (var d in _cache.ReadCustomDomains())
                domains.Add(d);
            return domains;
        }

        public async Task<IReadOnlyCollection<string>> UpdateNowAsync(IEnumerable<BlocklistSource> sources, CancellationToken ct)
        {
            var meta = _cache.ReadSourcesMeta();
            var metaByUrl = meta.ToDictionary(x => x.Url, StringComparer.OrdinalIgnoreCase);
            var finalDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var src in sources.Where(s => s.Enabled))
                {
                    if (metaByUrl.TryGetValue(src.Url, out var cached))
                    {
                        src.ETag = cached.ETag;
                        src.LastModified = cached.LastModified;
                    }

                    var fetch = await _fetcher.FetchAsync(src, ct).ConfigureAwait(false);

                    if (fetch.IsModified)
                    {
                        _cache.WriteSourceRaw(src, fetch.Content ?? string.Empty);
                        src.ETag = fetch.ETag;
                        src.LastModified = fetch.LastModified;
                    }

                    var raw = _cache.ReadSourceRaw(src) ?? string.Empty;
                    foreach (var d in _parser.ParseDomains(raw))
                        finalDomains.Add(d);
                }

                _cache.WriteDomains(finalDomains);
                _cache.WriteSourcesMeta(sources);
                foreach (var d in _cache.ReadCustomDomains())
                    finalDomains.Add(d);
                return finalDomains;
            }
            catch
            {
                var fallback = new HashSet<string>(_cache.ReadDomains(), StringComparer.OrdinalIgnoreCase);
                foreach (var d in _cache.ReadCustomDomains())
                    fallback.Add(d);
                return fallback;
            }
        }
    }
}
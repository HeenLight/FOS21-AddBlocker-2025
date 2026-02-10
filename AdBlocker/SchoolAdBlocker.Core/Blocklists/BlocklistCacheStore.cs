using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SchoolAdBlocker.Core.Utils;

namespace SchoolAdBlocker.Core.Blocklists
{
    public sealed class BlocklistCacheStore
    {
        private readonly string _baseDir;
        private readonly string _cacheDir;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public BlocklistCacheStore(string baseDir)
        {
            Guard.EnsureWritable(baseDir);
            _baseDir = baseDir;
            _cacheDir = Guard.EnsureDirectory(Path.Combine(baseDir, "cache"));
        }

        public string CacheDir => _cacheDir;

        public string DomainsPath => Path.Combine(_cacheDir, "domains.txt");
        public string SourcesPath => Path.Combine(_cacheDir, "sources.json");
        public string CustomPath => Path.Combine(_baseDir, "custom.txt");

        public IReadOnlyCollection<string> ReadDomains()
        {
            if (!File.Exists(DomainsPath)) return Array.Empty<string>();
            return File.ReadAllLines(DomainsPath);
        }

        public void WriteDomains(IEnumerable<string> domains)
        {
            WriteAtomic(DomainsPath, string.Join(Environment.NewLine, domains));
        }

        public List<BlocklistSource> ReadSourcesMeta()
        {
            if (!File.Exists(SourcesPath)) return new List<BlocklistSource>();
            var json = File.ReadAllText(SourcesPath);
            return JsonSerializer.Deserialize<List<BlocklistSource>>(json, JsonOptions) ?? new List<BlocklistSource>();
        }

        public void WriteSourcesMeta(IEnumerable<BlocklistSource> sources)
        {
            var json = JsonSerializer.Serialize(sources, JsonOptions);
            WriteAtomic(SourcesPath, json);
        }

        public string SourceRawPath(BlocklistSource source)
        {
            return Path.Combine(_cacheDir, $"source_{source.HashId()}.txt");
        }

        public string ReadSourceRaw(BlocklistSource source)
        {
            var path = SourceRawPath(source);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public void WriteSourceRaw(BlocklistSource source, string content)
        {
            WriteAtomic(SourceRawPath(source), content);
        }

        public IReadOnlyCollection<string> ReadCustomDomains()
        {
            if (!File.Exists(CustomPath)) return Array.Empty<string>();
            return File.ReadAllLines(CustomPath);
        }

        private static void WriteAtomic(string path, string content)
        {
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, content);

            if (File.Exists(path))
            {
                File.Replace(tempPath, path, null);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
    }
}
using System;
using System.IO;
using System.Text.Json;
using SchoolAdBlocker.Core.Models;
using SchoolAdBlocker.Core.Utils;

namespace SchoolAdBlocker.Core.Storage
{
    public sealed class SettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public AppSettings LoadOrCreateDefault(string baseDir)
        {
            Guard.EnsureWritable(baseDir);
            var path = Path.Combine(baseDir, "config.json");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }

            var settings = new AppSettings();
            Save(baseDir, settings);
            return settings;
        }

        public void Save(string baseDir, AppSettings settings)
        {
            Guard.EnsureWritable(baseDir);
            var path = Path.Combine(baseDir, "config.json");
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(path, json);
        }
    }
}
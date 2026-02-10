using System.Collections.Generic;

namespace SchoolAdBlocker.Core.Models
{
    public sealed class AppSettings
    {
        public int Port { get; set; } = 8888;
        public List<Blocklists.BlocklistSource> Sources { get; set; } = new List<Blocklists.BlocklistSource>
        {
            new Blocklists.BlocklistSource { Url = "https://someone.example/hosts.txt", Enabled = true }
        };
    }
}
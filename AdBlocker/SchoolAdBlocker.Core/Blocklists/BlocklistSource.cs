using System;
using System.Security.Cryptography;
using System.Text;

namespace SchoolAdBlocker.Core.Blocklists
{
    public sealed class BlocklistSource
    {
        public string Url { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public string ETag { get; set; }
        public DateTimeOffset? LastModified { get; set; }

        public string HashId()
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(Url));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
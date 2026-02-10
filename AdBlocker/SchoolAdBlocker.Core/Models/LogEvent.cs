using System;

namespace SchoolAdBlocker.Core.Models
{
    public sealed class LogEvent
    {
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string Level { get; set; } = "Info";
        public string Message { get; set; } = string.Empty;
    }
}
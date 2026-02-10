namespace SchoolAdBlocker.Core.Models
{
    public sealed class ProxyStats
    {
        public long Total { get; set; }
        public long Blocked { get; set; }
        public long Allowed { get; set; }
    }
}
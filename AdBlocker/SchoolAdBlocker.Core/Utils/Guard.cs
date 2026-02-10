using System;
using System.IO;

namespace SchoolAdBlocker.Core.Utils
{
    public static class Guard
    {
        public static void EnsureWritable(string baseDir)
        {
            try
            {
                var testFile = Path.Combine(baseDir, $".write_test_{Guid.NewGuid():N}.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No write permission in application directory", ex);
            }
        }

        public static string EnsureDirectory(string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
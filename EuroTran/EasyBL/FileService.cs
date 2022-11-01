using System;

namespace EasyBL
{
    public class FileService
    {
        public static string ToKMGTByte(string file_size)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            var s = 0;
            var size = Convert.ToInt64(file_size);

            while (size >= 1024)
            {
                s++;
                size /= 1024;
            }
            return $"{size}{suffixes[s]}";
        }
    }
}
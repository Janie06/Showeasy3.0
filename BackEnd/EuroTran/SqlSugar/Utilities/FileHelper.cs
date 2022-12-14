using System;
using System.IO;
using System.Text;

namespace SqlSugar
{
    internal class FileHelper
    {
        public static void CreateFile(string filePath, string text, Encoding encoding)
        {
            try
            {
                if (IsExistFile(filePath))
                {
                    DeleteFile(filePath);
                }
                if (!IsExistFile(filePath))
                {
                    var directoryPath = GetDirectoryFromFilePath(filePath);
                    CreateDirectory(directoryPath);

                    //Create File
                    var file = new FileInfo(filePath);
                    using (FileStream stream = file.Create())
                    {
                        using (StreamWriter writer = new StreamWriter(stream, encoding))
                        {
                            writer.Write(text);
                            writer.Flush();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public static bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
        public static void CreateDirectory(string directoryPath)
        {
            if (!IsExistDirectory(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        public static void DeleteFile(string filePath)
        {
            if (IsExistFile(filePath))
            {
                File.Delete(filePath);
            }
        }
        public static string GetDirectoryFromFilePath(string filePath)
        {
            var file = new FileInfo(filePath);
            var directory = file.Directory;
            return directory.FullName;
        }
        public static bool IsExistFile(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}

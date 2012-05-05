using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GameService.DataAccessLayer.Contracts;
using GameService.BusinessObjects.CustomClasses;
using System.Data.Linq;

namespace GameService.DataAccessLayer.TestImplementations
{
    internal class TestFileRepository : IFileRepository
    {
        public DataContext GetCurrentDataContext()
        {
            return null;
        }

        public File InsertFile(string filePath, bool isCheckedOut, bool isDirectory)
        {
            return new File
            {
                FileId = -1,
                FilePath = filePath,
                CheckedOut = isCheckedOut,
                IsDirectory = isDirectory
            };
        }

        public File GetFile(int fileId)
        {
            return new File
            {
                FileId = fileId
            };
        }

        public File GetFile(string filePath)
        {
            return new File
            {
                FileId = -1,
                FilePath = filePath
            };
        }
        public File SaveFile(File file)
        {
            return file;
        }
        public File UpdateFile(int fileId, string filePath, bool isCheckedOut, int? userIdCheckedOutTo, bool isDirectory)
        {
            return new File
            {
                FileId = fileId,
                FilePath = filePath,
                CheckedOut = isCheckedOut,
                IsDirectory = isDirectory,
                UserCheckedOutTo = userIdCheckedOutTo
            };
        }

        public File CheckoutFile(int fileId, int userId)
        {
            return new File
            {
                FileId = fileId,
                CheckedOut = true,
                UserCheckedOutTo = userId
            };
        }

        public File CheckInFile(int fileId)
        {
            return new File
            {
                FileId = fileId,
                CheckedOut = false
            };
        }

        public void DeleteFile(int fileId)
        {
            return;
        }
    }
}
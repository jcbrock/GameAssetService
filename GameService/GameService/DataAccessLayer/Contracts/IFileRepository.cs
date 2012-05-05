using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GameService.BusinessObjects.CustomClasses;
using System.Data.Linq;

namespace GameService.DataAccessLayer.Contracts
{
    public interface IFileRepository
    {
        File InsertFile(string filePath, bool isCheckedOut, bool isDirectory);
        File GetFile(int fileId);
        File GetFile(string filePath);
        File UpdateFile(int fileId, string filePath, bool isCheckedOut, int? userId, bool isDirectory);
        
        /// <summary>
        /// Inserts or updates the file based on if it is found in the database
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        File SaveFile(File file);
        File CheckoutFile(int fileId, int userId); //A little redundant of the UpdateFile function, but convienent
        File CheckInFile(int fileId); //A little redundant of the UpdateFile function, but convienent
        void DeleteFile(int fileId);
    }
}

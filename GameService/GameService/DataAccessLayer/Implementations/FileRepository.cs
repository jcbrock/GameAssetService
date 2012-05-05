using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GameService.DataAccessLayer.Contracts;
using System.Data.Linq;
using GameService.BusinessObjects.CustomClasses;
using System.Configuration;
using System.Web.Configuration;

namespace GameService.DataAccessLayer.Implementations
{
    internal class FileRepository : IFileRepository
    {
        DataContext _db;

        public FileRepository(DataContext existingDataContext)
        {
            _db = existingDataContext;
        }

        public FileRepository()
        {
            //Get the connection string from the web.config
            Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/GameAssetService");
            ConnectionStringSettings connString = new ConnectionStringSettings();
            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
                connString = rootWebConfig.ConnectionStrings.ConnectionStrings["GameAssetConnectionString"];
            if (connString == null)
                throw new System.ServiceModel.FaultException("conn string null");

            _db = new DataContext(connString.ConnectionString);
        }

        public File InsertFile(string filePath, bool isCheckedOut, bool isDirectory)
        {
            // Get a typed table to run queries.
            Table<File> files = _db.GetTable<File>();

            // Create a new Files object.
            File file = new File
            {
                FilePath = filePath,
                CheckedOut = isCheckedOut,
                IsDirectory = isDirectory
            };

            // Add the new object to the Files collection.
            files.InsertOnSubmit(file);

            // Submit the change to the database.
            _db.SubmitChanges();

            return file;
        }

        public File GetFile(int fileId)
        {
            // Get a typed table to run queries.
            Table<File> files = _db.GetTable<File>();

            // Query
            IQueryable<File> fileQuery =
                from file in files
                where file.FileId == fileId
                select file;

            File foundFile = fileQuery.FirstOrDefault();
            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", fileId));

            return foundFile;
        }

        public File GetFile(string filePath)
        {
            // Get a typed table to run queries.
            Table<File> files = _db.GetTable<File>();

            // Query
            IQueryable<File> fileQuery =
                from file in files
                where file.FilePath == filePath
                select file;

            File foundFile = fileQuery.FirstOrDefault();
            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file name of {0} was not found", filePath));

            return foundFile;
        }

        public File SaveFile(File file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            // Get a typed table to run queries.
            Table<File> files = _db.GetTable<File>();

            // Query
            IQueryable<File> fileQuery =
                from dbFile in files
                where dbFile.FileId == file.FileId
                select dbFile;

            File foundFile = fileQuery.FirstOrDefault();

            if (foundFile == null)
            {
                // Create a new Files object.
                // foundFile = file;
                //foundFile = new File
                //{
                //    FilePath = filePath,
                //    CheckedOut = isCheckedOut,
                //    IsDirectory = isDirectory
                //};

                // Add the new object to the Files collection.
                files.InsertOnSubmit(file);
                return file;

            }
            else

                //int tempFileId = foundFile.FileId;
                foundFile = file;
            // foundFile.FileId = tempFileId; //no point in doing file swap, it will be same
            //foundFile.FilePath = filePath;
            //foundFile.CheckedOut = isCheckedOut;
            //foundFile.IsDirectory = isDirectory;
            //foundFile.UserCheckedOutTo = userIdCheckedOutTo;

            _db.SubmitChanges();

            return foundFile;
        }

        public File UpdateFile(int fileId, string filePath, bool isCheckedOut, int? userIdCheckedOutTo, bool isDirectory)
        {
            // Get a typed table to run queries.
            Table<File> files = _db.GetTable<File>();

            // Query
            IQueryable<File> fileQuery =
                from file in files
                where file.FileId == fileId
                select file;

            File foundFile = fileQuery.FirstOrDefault();

            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", fileId));

            foundFile.FilePath = filePath;
            foundFile.CheckedOut = isCheckedOut;
            foundFile.IsDirectory = isDirectory;
            foundFile.UserCheckedOutTo = userIdCheckedOutTo;

            _db.SubmitChanges();

            return foundFile;
        }

        public File CheckInFile(int fileId)
        {
            // Get a typed table to run queries.
            Table<File> files = _db.GetTable<File>();

            // Query
            IQueryable<File> fileQuery =
                from file in files
                where file.FileId == fileId
                select file;

            File foundFile = fileQuery.FirstOrDefault();

            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", fileId));

            foundFile.CheckedOut = false;
            foundFile.UserCheckedOutTo = null;

            _db.SubmitChanges();

            return foundFile;
        }

        public File CheckoutFile(int fileId, int userId)
        {
            // Get a typed table to run queries.
            Table<File> files = _db.GetTable<File>();

            // Query
            IQueryable<File> fileQuery =
                from file in files
                where file.FileId == fileId
                select file;

            File foundFile = fileQuery.FirstOrDefault();

            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", fileId));

            foundFile.CheckedOut = true;
            foundFile.UserCheckedOutTo = userId;

            _db.SubmitChanges();

            return foundFile;
        }

        public void DeleteFile(int fileId)
        {
            // Get a typed table to run queries.
            Table<File> files = _db.GetTable<File>();

            // Query
            IQueryable<File> fileQuery =
                from file in files
                where file.FileId == fileId
                select file;

            File foundFile = fileQuery.FirstOrDefault();
            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", fileId));

            files.DeleteOnSubmit(foundFile);
            _db.SubmitChanges(); //todo - maybe put try/catches around these?
        }
    }
}

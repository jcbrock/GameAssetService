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
    internal class DownloadHistoryRepository : IDownloadHistoryRepository
    {
        DataContext _db;

        public DownloadHistoryRepository()
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

        public DownloadHistoryRepository(DataContext existingDataContext)
        {
            _db = existingDataContext;
        }

        //public DataContext GetCurrentDataContext()
        //{
        //    return db;
        //}

        public DownloadHistory InsertDownloadHistory(int userId, int fileId, DateTime downloadTime)
        {
            // Get a typed table to run queries.
            Table<DownloadHistory> files = _db.GetTable<DownloadHistory>();

            // Create a new Files object.
            DownloadHistory file = new DownloadHistory
            {
                //FilePath = filePath
                UserId = userId,
                FileId = fileId,
                DownloadTime = downloadTime
            };

            // Add the new object to the Files collection.
            files.InsertOnSubmit(file);

            // Submit the change to the database.
            _db.SubmitChanges();

            return file;
        }
        public DownloadHistory GetDownloadHistory(int downloadHistoryId)
        {
            // Get a typed table to run queries.
            Table<DownloadHistory> files = _db.GetTable<DownloadHistory>();

            // Query
            IQueryable<DownloadHistory> fileQuery =
                from file in files
                where file.DownloadHistoryId == downloadHistoryId
                select file;

            DownloadHistory foundFile = fileQuery.FirstOrDefault();
            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", downloadHistoryId));

            return foundFile;
        }
        public DownloadHistory UpdateDownloadHistory(int downloadHistoryId, int userId, int fileId, DateTime downloadTime)
        {
            // Get a typed table to run queries.
            Table<DownloadHistory> files = _db.GetTable<DownloadHistory>();

            // Query
            IQueryable<DownloadHistory> fileQuery =
                from file in files
                where file.DownloadHistoryId == downloadHistoryId
                select file;

            DownloadHistory foundFile = fileQuery.FirstOrDefault();

            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", downloadHistoryId));

            foundFile.UserId = userId;
            foundFile.FileId = fileId;
            foundFile.DownloadTime = downloadTime;

            _db.SubmitChanges();

            return foundFile;
        }
        public void DeleteDownloadHistory(int downloadHistoryId)
        {
            // Get a typed table to run queries.
            Table<DownloadHistory> files = _db.GetTable<DownloadHistory>();

            // Query
            IQueryable<DownloadHistory> fileQuery =
                from file in files
                where file.DownloadHistoryId == downloadHistoryId
                select file;

            DownloadHistory foundFile = fileQuery.FirstOrDefault();
            if (foundFile == null)
                throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", downloadHistoryId));

            files.DeleteOnSubmit(foundFile);
            _db.SubmitChanges(); //todo - maybe put try/catches around these?
        }

       public DownloadHistory GetDownloadHistory(int userId, int fileId)
        {
            // Get a typed table to run queries.
            Table<DownloadHistory> files = _db.GetTable<DownloadHistory>();

            // Query
            IQueryable<DownloadHistory> fileQuery =
                from file in files
                where file.UserId == userId
                && file.FileId == fileId
                select file;

           return fileQuery.FirstOrDefault();
           // if (foundFile == null)
           //     throw new GameService.Common.DatabaseRecordNotFoundException(string.Format("File with a file ID of {0} was not found", downloadHistoryId));

           // files.DeleteOnSubmit(foundFile);
            //db.SubmitChanges(); //todo - maybe put try/catches around these?
        }
    }
}
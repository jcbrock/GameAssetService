using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GameService.DataAccessLayer.Contracts;
using GameService.BusinessObjects.CustomClasses;
using System.Data.Linq;

namespace GameService.DataAccessLayer.TestImplementations
{
    internal class TestDownloadHistoryRepository : IDownloadHistoryRepository
    {
        public DataContext GetCurrentDataContext()
        {
            return null;
        }
        public DownloadHistory InsertDownloadHistory(int userId, int fileId, DateTime downloadTime)
        {
            return new DownloadHistory
            {
                DownloadHistoryId = -1,
                UserId = userId,
                FileId = fileId,
                DownloadTime = downloadTime
            };
        }
        public DownloadHistory GetDownloadHistory(int downloadHistoryId)
        {
            return new DownloadHistory
            {
                DownloadHistoryId = downloadHistoryId
            };
        }
        public DownloadHistory UpdateDownloadHistory(int downloadHistoryId, int userId, int fileId, DateTime downloadTime)
        {
            return new DownloadHistory
            {
                DownloadHistoryId = downloadHistoryId,
                UserId = userId,
                FileId = fileId,
                DownloadTime = downloadTime
            };
        }
        public void DeleteDownloadHistory(int downloadHistoryId)
        {
            return;
        }

        public DownloadHistory GetDownloadHistory(int userId, int fileId)
        {
            return null; 
        }
    }
}
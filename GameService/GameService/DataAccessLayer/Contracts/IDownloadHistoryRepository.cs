using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GameService.BusinessObjects.CustomClasses;
using System.Data.Linq;

namespace GameService.DataAccessLayer.Contracts
{
    public interface IDownloadHistoryRepository
    {
        DownloadHistory InsertDownloadHistory(int userId, int fileId, DateTime downloadTime);
        DownloadHistory GetDownloadHistory(int downloadHistoryId);
        DownloadHistory GetDownloadHistory(int userId, int fileId);
        DownloadHistory UpdateDownloadHistory(int downloadHistoryId, int userId, int fileId, DateTime downloadTime);
        void DeleteDownloadHistory(int downloadHistoryId);
    }
}

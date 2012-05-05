using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Linq.Mapping;
namespace GameService.BusinessObjects.CustomClasses
{
     [Table(Name = "DownloadHistory")]
    public class DownloadHistory
    {
        private int _DownloadHistoryId;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, Storage = "_DownloadHistoryId")]
        public int DownloadHistoryId
        {
            get
            {
                return this._DownloadHistoryId;
            }
            set
            {
                this._DownloadHistoryId = value;
            }

        }

        private int _UserId;
        [Column(Storage = "_UserId", CanBeNull = false)]
        public int UserId
        {
            get
            {
                return this._UserId;
            }
            set
            {
                this._UserId = value;
            }
        }

        private int _FileId;
        [Column(Storage = "_FileId", CanBeNull = false)]
        public int FileId
        {
            get
            {
                return this._FileId;
            }
            set
            {
                this._FileId = value;
            }
        }

        private DateTime _DownloadTime;
        [Column(Storage = "_DownloadTime", CanBeNull = false)]
        public DateTime DownloadTime
        {
            get
            {
                return this._DownloadTime;
            }
            set
            {
                this._DownloadTime = value;
            }
        }
    }
}
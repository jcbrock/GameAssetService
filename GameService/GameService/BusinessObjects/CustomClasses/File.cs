using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Linq.Mapping;

namespace GameService.BusinessObjects.CustomClasses
{
    [Table(Name = "File")]
    public class File
    {
        private int _FileId;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, Storage = "_FileId")]
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

        private string _FilePath;
        [Column(Storage = "_FilePath", CanBeNull = false)]
        public string FilePath
        {
            get
            {
                return this._FilePath;
            }
            set
            {
                this._FilePath = value;
            }
        }

        private bool _CheckedOut;
        [Column(Storage = "_CheckedOut", CanBeNull = false)]
        public bool CheckedOut
        {
            get
            {
                return this._CheckedOut;
            }
            set
            {
                this._CheckedOut = value;
            }
        }

        private int? _UserCheckedOutTo;
        [Column(Storage = "_UserCheckedOutTo", CanBeNull = true)]
        public int? UserCheckedOutTo
        {
            get
            {
                return this._UserCheckedOutTo;
            }
            set
            {
                this._UserCheckedOutTo = value;
            }
        }

        private bool _IsDirectory;
        [Column(Storage = "_IsDirectory", CanBeNull = false)]
        public bool IsDirectory
        {
            get
            {
                return this._IsDirectory;
            }
            set
            {
                this._IsDirectory = value;
            }
        }
    }
}
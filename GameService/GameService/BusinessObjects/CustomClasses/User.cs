using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Linq.Mapping;
namespace GameService.BusinessObjects.CustomClasses
{
    [Table(Name = "User")]
    public class User
    {
        private int _UserId;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, Storage = "_UserId")]
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

        private string _UserName;
        [Column(Storage = "_UserName", CanBeNull = false)]
        public string UserName
        {
            get
            {
                return this._UserName;
            }
            set
            {
                this._UserName = value;
            }
        }
    }
}
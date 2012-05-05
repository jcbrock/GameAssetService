using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GameService.BusinessObjects.CustomClasses;
using System.Data.Linq;

namespace GameService.DataAccessLayer.Contracts
{
    public interface IUserRepository
    {
        User InsertUser(string userName);
        User GetUser(int userId);
        User UpdateUser(int userId, string userName);
        void DeleteUser(int userId);
    }
}

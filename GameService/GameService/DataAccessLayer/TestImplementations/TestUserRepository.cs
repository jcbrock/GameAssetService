using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GameService.DataAccessLayer.Contracts;
using GameService.BusinessObjects.CustomClasses;
using System.Data.Linq;

namespace GameService.DataAccessLayer.TestImplementations
{
    internal class TestUserRepository : IUserRepository
    {
        public DataContext GetCurrentDataContext()
        {
            return null;
        }

        public User InsertUser(string userName)
        {
            return new User
            {
                UserId = -1,
                UserName = userName
            };
        }

        public User GetUser(int userId)
        {
            return new User
            {
                UserId = userId
            };
        }

        public User UpdateUser(int userId, string userName)
        {
            return new User
            {
                UserId = userId,
                UserName = userName
            };
        }
        public void DeleteUser(int userId)
        {
            return;
        }
    }
}
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
    //Note: This doesn't force classes in this assembly to go through the factory to get a Repo object 
    //(couldn't figure it out), but at least it protects against outside of the assembly
    internal class UserRepository : IUserRepository, IDisposable
    {
        DataContext _db;

        public UserRepository(DataContext existingDataContext)
        {
            _db = existingDataContext;
        }

        public UserRepository()
        {
            //ok, so I have trouble with error handling when deployed, but at least for the conn string issue:
            //When it is encrypted it seems like the server can't get the conn string (but it works fine
            //if not encrypted)

            //throw new System.ServiceModel.FaultException("test");
            // //Get the connection string from the web.config
            Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/GameAssetService");
            ConnectionStringSettings connString = new ConnectionStringSettings();
            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
                connString = rootWebConfig.ConnectionStrings.ConnectionStrings["GameAssetConnectionString"];
            if (connString == null)
                throw new System.ServiceModel.FaultException("conn string null");



            //throw new System.Configuration.ConfigurationErrorsException("Unable to access connection string");

            /*
            string configString = string.Empty;
            string ftpAddress = string.Empty;
            
            try
            {
               // XDocument xmlDoc = XDocument.Load(System.AppDomain.CurrentDomain.BaseDirectory + "\\config.xml");
                XDocument xmlDoc = XDocument.Load("..\\config.xml");

                configString = xmlDoc.Descendants("configuration").Elements("configurationString").First().Value;
                ftpAddress = xmlDoc.Descendants("configuration").Elements("ftpRootAddress").First().Value;
            }
            catch (Exception ex)
            {
                throw;
                //log - todo
            }
            */

            _db = new DataContext(connString.ConnectionString);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public User InsertUser(string userName)
        {           
            // Get a typed table to run queries.
            Table<User> users = _db.GetTable<User>();

            // Create a new Users object.
            User user = new User
            {
                UserName = userName
            };

            // Add the new object to the Users collection.
            users.InsertOnSubmit(user);

            // Submit the change to the database.
            _db.SubmitChanges();

            return user;
        }

        public User GetUser(int userId)
        {
            // Get a typed table to run queries.
            Table<User> users = _db.GetTable<User>();

            // Query
            IQueryable<User> userQuery =
                from user in users
                where user.UserId == userId
                select user;

            User foundUser = userQuery.FirstOrDefault();
            if (foundUser == null)
                throw new GameService.Common.UserNotFoundException(string.Format("User with a user ID of {0} was not found", userId));

            return foundUser;
        }

        public User UpdateUser(int userId, string userName)
        {
            // Get a typed table to run queries.
            Table<User> users = _db.GetTable<User>();

            // Query
            IQueryable<User> userQuery =
                from user in users
                where user.UserId == userId
                select user;

            User foundUser = userQuery.FirstOrDefault();

            if (foundUser == null)
                throw new GameService.Common.UserNotFoundException(string.Format("User with a user ID of {0} was not found", userId));

            foundUser.UserName = userName;

            _db.SubmitChanges();

            return foundUser;
        }

        public void DeleteUser(int userId)
        {
            // Get a typed table to run queries.
            Table<User> users = _db.GetTable<User>();

            // Query
            IQueryable<User> userQuery =
                from user in users
                where user.UserId == userId
                select user;

            User foundUser = userQuery.FirstOrDefault();
            if (foundUser == null)
                throw new GameService.Common.UserNotFoundException(string.Format("User with a user ID of {0} was not found", userId));

            users.DeleteOnSubmit(foundUser);
            _db.SubmitChanges(); //todo - maybe put try/catches around these?
        }
    }
}
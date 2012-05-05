using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using GameService.DataAccessLayer.Contracts;
using GameService.DataAccessLayer.Implementations;
using GameService.DataAccessLayer.TestImplementations;
using System.Data.Linq;

namespace GameService.DataAccessLayer
{
    /// <summary>
    /// I didn't make this a static class because I wanted the functionality of having multiple factories that each point
    /// to a different database in case I need that in the future.
    /// </summary>
    public class RepositoryFactory : IDisposable
    {
        private DataContext _db;

        public RepositoryFactory()
        {}

        public RepositoryFactory(string connectionString)
        {
            _db = new DataContext(connectionString);
        }

        public RepositoryFactory(DataContext db)
        {
            _db = db;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
     
        public IUserRepository GetUserRepository(bool isTesting)
        {
            return GetUserRepositoryHelper(isTesting, _db);
        }

        private IUserRepository GetUserRepositoryHelper(bool isTesting, DataContext existingDataContext)
        {
            if (isTesting)
                return new TestUserRepository();
            else
            {
                if (existingDataContext != null)
                    return new UserRepository(_db);
                else
                    return new UserRepository();
            }
        }

        public IDownloadHistoryRepository GetDownloadHistoryRepository(bool isTesting)
        {
            return GetDownloadHistoryRepositoryHelper(isTesting, _db);
        }

        private IDownloadHistoryRepository GetDownloadHistoryRepositoryHelper(bool isTesting, DataContext existingDataContext)
        {
            if (isTesting)
                return new TestDownloadHistoryRepository();
            else
            {
                if (existingDataContext != null)
                    return new DownloadHistoryRepository(_db);
                else
                    return new DownloadHistoryRepository();
            }
        }       

        public IFileRepository GetFileRepository(bool isTesting)
        {
            return GetFileRepositoryHelper(isTesting, _db);
        }

        private IFileRepository GetFileRepositoryHelper(bool isTesting, DataContext existingDataContext)
        {
            if (isTesting)
                return new TestFileRepository();
            else
            {
                if (existingDataContext != null)
                    return new FileRepository(_db);
                else
                    return new FileRepository();
            }
        }     
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//using System.Web.Configuration;
using GameService.DataAccessLayer.Contracts;
using GameService.DataAccessLayer.Implementations;
using GameService.BusinessObjects.CustomClasses;
using GameService.Common;
using System.Data.Linq;
using FTPWrapper;
//using System.Xml.Linq;
//using System.Configuration;

namespace GameService.ServiceAccessLayer.ServiceImplementations
{
    public class GameAssetService : IGameAssetService
    {
        IFileRepository _fileRepo;
        IUserRepository _userRepo;
        IDownloadHistoryRepository _downloadHistoryRepo;
        IFTPWrapperRepository _ftpWrapper;

        public GameAssetService()
        {
         

            DataAccessLayer.RepositoryFactory factory = new DataAccessLayer.RepositoryFactory();
            SetDependancies(factory.GetFileRepository(false), factory.GetUserRepository(false),
                            factory.GetDownloadHistoryRepository(false),
                            new FTPWrapperRepository("ftp://www.jeffandlainawedding.com/jeffandlainawedding.com/wwwroot/GameAssets/"));
            //I'm fine with FTP url being hardcoded - creds are needed to access anyway   
        }

        public GameAssetService(string connectionString)
        {
            DataAccessLayer.RepositoryFactory factory = new DataAccessLayer.RepositoryFactory(connectionString);
            SetDependancies(factory.GetFileRepository(false), factory.GetUserRepository(false),
                            factory.GetDownloadHistoryRepository(false),
                            new FTPWrapperRepository("ftp://www.jeffandlainawedding.com/jeffandlainawedding.com/wwwroot/GameAssets/"));
            //I'm fine with FTP url being hardcoded - creds are needed to access anyway  
        }

        public GameAssetService(DataContext db)
        {
            DataAccessLayer.RepositoryFactory factory = new DataAccessLayer.RepositoryFactory(db);
            SetDependancies(factory.GetFileRepository(false), factory.GetUserRepository(false),
                            factory.GetDownloadHistoryRepository(false),
                            new FTPWrapperRepository("ftp://www.jeffandlainawedding.com/jeffandlainawedding.com/wwwroot/GameAssets/"));
            //I'm fine with FTP url being hardcoded - creds are needed to access anyway  
        }

        public GameAssetService(IFileRepository fileRepo, IUserRepository userRepo,
                                   IDownloadHistoryRepository downloadHistoryRepo, IFTPWrapperRepository ftpWrapper)
        {
            SetDependancies(fileRepo, userRepo, downloadHistoryRepo, ftpWrapper);
        }

        //Ideally I'd just have the constructors all call one and pass in the right parameters, 
        //but due to Factory configuration I couldn't do that
        private void SetDependancies(IFileRepository fileRepo, IUserRepository userRepo,
                                     IDownloadHistoryRepository downloadHistoryRepo, IFTPWrapperRepository ftpWrapper)
        {
            _fileRepo = fileRepo;
            _userRepo = userRepo;
            _downloadHistoryRepo = downloadHistoryRepo;
            _ftpWrapper = ftpWrapper;
        }

        //[OperationContract]
        //CompositeType GetDataUsingDataContract(CompositeType composite);


        public bool? IsFileCheckedOut(string fileName)
        {
            return null;
        }

        //      [OperationContract]
        //        void WriteDownloadTimesToDB(string userName, List<string> fileNames);

        //Can you do a GetLatest on a file not in DB? No... must have checked in first

        public List<string> GetLatest(int fileId, int userId)
        {
            //Verify user exists before checking out files to that user
            if (_userRepo.GetUser(userId) == null)
                throw new UserNotFoundException(string.Format("No user was found with this ID: {0} " + Environment.NewLine + "No checkouts were made.", userId));

            //GetFileList
            List<File> getLatestFileList = BuildUpFileList(fileId);

            //Update DownloadHistory table
            foreach (File file in getLatestFileList)
            {
                DownloadHistory downloadHistory = _downloadHistoryRepo.GetDownloadHistory(userId, file.FileId);
                if (downloadHistory == null)
                    _downloadHistoryRepo.InsertDownloadHistory(userId, file.FileId, DateTime.Now);
                else
                    _downloadHistoryRepo.UpdateDownloadHistory(downloadHistory.DownloadHistoryId,
                        downloadHistory.UserId, downloadHistory.FileId, DateTime.Now);
            }

            return getLatestFileList.Select(i => i.FilePath).ToList();
        }

        //public List<KeyValuePair<int, CheckInStatus>> CheckIn(int? userId, List<int> filePaths)
        //{
        //    List<KeyValuePair<int, CheckInStatus>> statusList = new List<KeyValuePair<int, CheckInStatus>>();
        //    //Verify user exists before checking out files to that user
        //    if (_userRepo.GetUser(userId) == null)
        //        throw new UserNotFoundException(string.Format("No user was found with this ID: {0} " + Environment.NewLine + "No checkouts were made.", userId));

        //    return statusList;
        //}
        public CheckInStatus CheckIn(int userId, File file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            //Verify user exists before checking out files to that user
            if (_userRepo.GetUser(userId) == null)
                throw new UserNotFoundException(string.Format("No user was found with this ID: {0} " + Environment.NewLine + "No checkouts were made.", userId));

            //pass in File that is brand new
            //pass in used File
            //ah, i dont't want to be able to edit a File that is checked out...
            //TODO - START HERE , move insertFile into history == null part
            //move updateFile into history != null part

            //so if htis doesn't find a file... what does that mean?
            //that means that the file doesn't exist on the server, but we're not sure yet if it 
            //used to exist and was deleted from underneath us, or if it is a brand new file

            File checkedOutFile = _fileRepo.GetFile(file.FileId);
            if (checkedOutFile != null && checkedOutFile.CheckedOut && checkedOutFile.UserCheckedOutTo.HasValue && checkedOutFile.UserCheckedOutTo.Value != userId)
                return CheckInStatus.FileCheckedOutBySomeoneElse;  //I could probably pass back who

            DownloadHistory history = _downloadHistoryRepo.GetDownloadHistory(userId, file.FileId);
            if (history == null)
            {
                file = _fileRepo.SaveFile(file);
                if (file == null)
                    return CheckInStatus.ErrorOccurred;
                _downloadHistoryRepo.InsertDownloadHistory(userId, file.FileId, DateTime.Now);
                _fileRepo.CheckInFile(file.FileId);
                return CheckInStatus.FileNotFoundOnServer;
            }
            else
            {
                DateTime serverTime = _ftpWrapper.GetUploadTime(file.FilePath);
                if (history.DownloadTime >= serverTime)
                {
                    file = _fileRepo.SaveFile(file);
                    if (file == null)
                        return CheckInStatus.ErrorOccurred;
                    _downloadHistoryRepo.UpdateDownloadHistory(history.DownloadHistoryId, userId,
                        history.FileId, DateTime.Now);
                    _fileRepo.CheckInFile(history.FileId);
                    return CheckInStatus.Successful;
                }
                else
                    return CheckInStatus.NewerVersionFoundOnServer;
            }
        }

        //Crap, todo - Do I have to recurse over sub folders and check that stuff out too?
        //Well, crap.. I'd have to detect if that is a folder or not
        //Can I do that via ListDirectory, or do I need to do ListDirectoryDetails?
        //Also, is this even worth it... damn... I'm just gunna go with NO for now
        /*
        public void Checkout2(int userId, int fileId)
        {
            File file = _fileRepo.GetFile(fileId);
            List<string> fileNamesInDirectory;
            List<int> checkoutList = new List<int>();
            checkoutList.Add(fileId);

            //If checking out a folder, then look for sub files on File Server
            if (file.IsDirectory)
            {
                fileNamesInDirectory = _ftpWrapper.ListDirectory(file.FilePath);

                // if (fileNamesInDirectory != null)
                //    foreach (string fileName in fileNamesInDirectory)
                //      checkoutList.Add(_fileRepo.GetFile(fileName).FileId);
            }

            //Verify user exists before checking out files to that user
            if (_userRepo.GetUser(userId) == null)
                throw new UserNotFoundException(string.Format("No user was found with this ID: {0} " + Environment.NewLine + "No checkouts were made.", userId));

            foreach (int fileIdToCheckout in checkoutList)
                _fileRepo.CheckoutFile(fileIdToCheckout, userId);
        }
         */

        public void Checkout(int userId, int fileId)
        {
            List<File> checkoutList = BuildUpFileList(fileId);

            //Verify user exists before checking out files to that user
            if (_userRepo.GetUser(userId) == null)
                throw new UserNotFoundException(string.Format("No user was found with this ID: {0} " + Environment.NewLine + "No checkouts were made.", userId));

            foreach (File fileIdToCheckout in checkoutList)
                _fileRepo.CheckoutFile(fileIdToCheckout.FileId, userId);
        }

        //Do I include directories? I dunno... I think not now. I would need them if checkout was recursive
        private List<File> BuildUpFileList(int fileId)
        {
            File file = _fileRepo.GetFile(fileId);
            List<string> fileNamesInDirectory;
            List<File> fileList = new List<File>();

            //If checking out a folder, then look for sub files on File Server
            if (file.IsDirectory)
            {
                fileNamesInDirectory = _ftpWrapper.ListDirectory(file.FilePath);

                if (fileNamesInDirectory != null)
                    foreach (string fileName in fileNamesInDirectory)
                        fileList.Add(_fileRepo.GetFile(fileName));
            }
            else
                fileList.Add(file);

            return fileList;
        }
    }
}
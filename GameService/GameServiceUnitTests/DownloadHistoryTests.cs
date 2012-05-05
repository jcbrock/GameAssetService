using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient; //Had to add System.Data reference so I could anticipate returned SqlExceptions
using NUnit.Framework;
using System.Reflection;
using GameService;
using GameService.BusinessObjects.CustomClasses;
using GameService.DataAccessLayer;
using GameService.DataAccessLayer.Contracts;
using GameService.DataAccessLayer.Implementations;
using GameService.DataAccessLayer.TestImplementations;
using GameService.Common;
using System.Xml.Linq;

//Test names: MethodName_StateUnderTest_ExpectedBehavior
[TestFixture]
public class DownloadHistoryTests
{
    private IDownloadHistoryRepository downloadHistoryRepo;
    private IUserRepository userRepo;
    private IFileRepository fileRepo;
    private IDownloadHistoryRepository mockDownloadHistoryRepo;
    private RepositoryFactory repositoryFactory;
    string configString = string.Empty;

    public DownloadHistoryTests()
    {
        //Get connection string from config file
        XDocument xmlDoc = XDocument.Load("..\\..\\config.xml");
        configString = xmlDoc.Descendants("configuration").Elements("configurationString").First().Value;
    }

    [SetUp]
    public void Setup()
    {
        repositoryFactory = new RepositoryFactory(configString);
        Assert.NotNull(repositoryFactory);

        userRepo = repositoryFactory.GetUserRepository(false);
        Assert.NotNull(userRepo);

        fileRepo = repositoryFactory.GetFileRepository(false); //Need to use the existing DataContext for this test already setup in UserRepository
        Assert.NotNull(fileRepo);

        downloadHistoryRepo = repositoryFactory.GetDownloadHistoryRepository(false); //Need to use the existing DataContext for this test already setup in UserRepository
        Assert.NotNull(downloadHistoryRepo);

        mockDownloadHistoryRepo = repositoryFactory.GetDownloadHistoryRepository(true); //This returns a test repo that doesn't hit the database
        Assert.NotNull(mockDownloadHistoryRepo);
    }

    #region UNIT TESTS
    /// <summary>
    /// Retrieve actual user repository and verify that the right type was returned. Do
    /// the same for the test version of the repository.
    /// 
    /// //This will break if the class is renamed, but I can't get at the actual type because
    /// //the class is "internal", so this will have to do. For example, this doesn't work:
    /// //Assert.IsInstanceOf(typeof(TestDownloadHistoryRepository), downloadHistoryRepo);
    /// </summary>
    [Test]
    [Category("Unit")]
    public void GetDownloadHistoryRepository_RetrieveTestAndActualRepos_SuccessfulRetrieval()
    {
        downloadHistoryRepo = repositoryFactory.GetDownloadHistoryRepository(false);
        Assert.NotNull(downloadHistoryRepo);
        Assert.AreEqual(downloadHistoryRepo.GetType().Name, "DownloadHistoryRepository");

        downloadHistoryRepo = repositoryFactory.GetDownloadHistoryRepository(true);
        Assert.NotNull(downloadHistoryRepo);
        Assert.AreEqual(downloadHistoryRepo.GetType().Name, "TestDownloadHistoryRepository");
    }
    #endregion

    #region INTEGRATION TESTS

    /// <summary>
    /// Testing the object inserted by the mock repository to an inserted object by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertDownloadHistory_CompareRealInsertToMockInsert_EqualDownloadHistorysReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            User insertedUser = userRepo.InsertUser("newInsertUserName");
            File insertedFile = fileRepo.InsertFile("newInsertFileName", false, false);
            DateTime today = System.DateTime.Today;

            //Insert a downloadHistory to update
            DownloadHistory realInsertedDownloadHistory = InsertDownloadHistory(insertedUser.UserId, insertedFile.FileId, today);
            DownloadHistory mockInsertedDownloadHistory = mockInsertDownloadHistory(insertedUser.UserId, insertedFile.FileId, today);

            //Run tests
            Assert.AreEqual(realInsertedDownloadHistory.FileId, mockInsertedDownloadHistory.FileId);
            Assert.AreEqual(realInsertedDownloadHistory.UserId, mockInsertedDownloadHistory.UserId);
            Assert.AreEqual(realInsertedDownloadHistory.DownloadTime, mockInsertedDownloadHistory.DownloadTime);
        }
    }

    /// <summary>
    /// Testing the object updated by the mock repository to a updated object by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateDownloadHistory_CompareRealUpdateToMockUpdate_EqualDownloadHistorysReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            DateTime today = System.DateTime.Today;

            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();

            //Do updates
            DownloadHistory realUpdatedDownloadHistory = downloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory.DownloadHistoryId, insertedDownloadHistory.UserId, insertedDownloadHistory.FileId, today);
            DownloadHistory mockUpdatedDownloadHistory = mockDownloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory.DownloadHistoryId, insertedDownloadHistory.UserId, insertedDownloadHistory.FileId, today);

            //Run tests
            Assert.AreEqual(realUpdatedDownloadHistory.DownloadHistoryId, mockUpdatedDownloadHistory.DownloadHistoryId);
            Assert.AreEqual(realUpdatedDownloadHistory.UserId, mockUpdatedDownloadHistory.UserId);
            Assert.AreEqual(realUpdatedDownloadHistory.FileId, mockUpdatedDownloadHistory.FileId);
            Assert.AreEqual(realUpdatedDownloadHistory.DownloadTime, mockUpdatedDownloadHistory.DownloadTime);
        }
    }

    /// <summary>
    /// Testing the object returned by the mock repository to an retrieval by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void RetrieveDownloadHistory_CompareRealRetrievalToMockRetrieval_EqualDownloadHistorysReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();

            //Do retreivals
            DownloadHistory realRetrievedDownloadHistory = downloadHistoryRepo.GetDownloadHistory(insertedDownloadHistory.DownloadHistoryId);
            DownloadHistory mockRetrievedDownloadHistory = mockDownloadHistoryRepo.GetDownloadHistory(insertedDownloadHistory.DownloadHistoryId);

            //Run tests
            Assert.AreEqual(realRetrievedDownloadHistory.DownloadHistoryId, mockRetrievedDownloadHistory.DownloadHistoryId);
        }
    }
    /// <summary>
    /// Testing insertion into the database with a unique downloadHistory name.
    /// The test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertDownloadHistory_UserIdAndFileIdAreUnique_SuccessfulInsert()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();
            DownloadHistory retrievedDownloadHistory = RetrieveDownloadHistory(insertedDownloadHistory.DownloadHistoryId);

            Assert.AreEqual(retrievedDownloadHistory.DownloadHistoryId, insertedDownloadHistory.DownloadHistoryId);
            Assert.AreEqual(retrievedDownloadHistory.UserId, insertedDownloadHistory.UserId);
            Assert.AreEqual(retrievedDownloadHistory.FileId, insertedDownloadHistory.FileId);
            Assert.AreEqual(retrievedDownloadHistory.DownloadTime, insertedDownloadHistory.DownloadTime);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a non unique user id, but a unique file id.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertDownloadHistory_UserIdIsNotUniqueFileIdIsUnique_SuccessfulInsert()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            File insertedFile = fileRepo.InsertFile("newInsertFileName", false, false);
            DateTime today = System.DateTime.Today;

            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();
            InsertDownloadHistory(insertedDownloadHistory.UserId, insertedFile.FileId, today);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a non unique file id, but a unique user id.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertDownloadHistory_UserIdIsUniqueFileIdIsNotUnique_SuccessfulInsert()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            User insertedUser = userRepo.InsertUser("newInsertUserName");
            DateTime today = System.DateTime.Today;

            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();
            InsertDownloadHistory(insertedUser.UserId, insertedDownloadHistory.FileId, today);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a non unique user id and file id.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void InsertDownloadHistory_UserIdAndFileIdIsNotUnique_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            DateTime today = System.DateTime.Today;

            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();
            InsertDownloadHistory(insertedDownloadHistory.UserId, insertedDownloadHistory.FileId, today);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a nonvalid user id.
    /// The test should cause an exception.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void InsertDownloadHistory_UserIdIsNonvalid_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            File insertedFile = fileRepo.InsertFile("newInsertFileName", false, false);
            DateTime today = System.DateTime.Today;

            InsertDownloadHistory(-1, insertedFile.FileId, today);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a nonvalid file id.
    /// The test should cause an exception.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void InsertDownloadHistory_FileIdIsNonvalid_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            User insertedUser = userRepo.InsertUser("newInsertUserName");
            DateTime today = System.DateTime.Today;

            InsertDownloadHistory(insertedUser.UserId, -1, today);
        }
    }

    /// <summary>
    /// Testing the retrieve database transaction. 
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void RetrieveDownloadHistory_DownloadHistoryFoundInDatabase_SuccessfulRetrieval()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();
            DownloadHistory retrievedDownloadHistory = downloadHistoryRepo.GetDownloadHistory(insertedDownloadHistory.DownloadHistoryId);

            Assert.NotNull(retrievedDownloadHistory);
            Assert.Greater(retrievedDownloadHistory.DownloadHistoryId, 0);
            Assert.AreEqual(retrievedDownloadHistory.DownloadHistoryId, insertedDownloadHistory.DownloadHistoryId);
            Assert.AreEqual(retrievedDownloadHistory.UserId, insertedDownloadHistory.UserId);
            Assert.AreEqual(retrievedDownloadHistory.FileId, insertedDownloadHistory.FileId);
            Assert.AreEqual(retrievedDownloadHistory.DownloadTime, insertedDownloadHistory.DownloadTime);
        }
    }

    /// <summary>
    // Testing to make sure exception is thrown when trying to get a nonexistent record.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void RetrieveDownloadHistory_DownloadHistoryNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            DownloadHistory retrievedDownloadHistory = downloadHistoryRepo.GetDownloadHistory(-1);
        }
    }

    /// <summary>
    /// Testing the update database transaction.
    /// This test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateDownloadHistory_GiveDownloadHistoryFoundInDatabaseAUniqueUserIdAndFileId_SuccessfulUpdate()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();

            User insertedUser = userRepo.InsertUser("newInsertUserName");
            File insertedFile = fileRepo.InsertFile("newInsertFileName", false, false);
            DateTime today = System.DateTime.Today;

            //Update the downloadHistory
            downloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory.DownloadHistoryId, insertedUser.UserId, insertedFile.FileId, today);

            //Retrieve updated downloadHistory from DB
            DownloadHistory updatedDownloadHistory = RetrieveDownloadHistory(insertedDownloadHistory.DownloadHistoryId);

            //Run tests
            Assert.NotNull(updatedDownloadHistory);
            Assert.Greater(updatedDownloadHistory.DownloadHistoryId, 0);
            Assert.AreEqual(updatedDownloadHistory.UserId, insertedUser.UserId);
            Assert.AreEqual(updatedDownloadHistory.FileId, insertedFile.FileId);
            Assert.AreEqual(updatedDownloadHistory.DownloadTime, today);
        }
    }

    /// <summary>
    /// Testing the update database transaction.
    /// This test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateDownloadHistory_GiveDownloadHistoryFoundInDatabaseAUniqueUserIdAndNonUniqueFileId_SuccessfulUpdate()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();
            User insertedUser = userRepo.InsertUser("newInsertUserName");
            DateTime today = System.DateTime.Today;

            //Update the downloadHistory
            downloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory.DownloadHistoryId, insertedUser.UserId, insertedDownloadHistory.FileId, today);

            //Retrieve updated downloadHistory from DB
            DownloadHistory updatedDownloadHistory = RetrieveDownloadHistory(insertedDownloadHistory.DownloadHistoryId);

            //Run tests
            Assert.NotNull(updatedDownloadHistory);
            Assert.Greater(updatedDownloadHistory.DownloadHistoryId, 0);
            Assert.AreEqual(updatedDownloadHistory.UserId, insertedUser.UserId);
            Assert.AreEqual(updatedDownloadHistory.FileId, insertedDownloadHistory.FileId);
            Assert.AreEqual(updatedDownloadHistory.DownloadTime, today);
        }
    }

    /// <summary>
    /// Testing the update database transaction.
    /// This test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateDownloadHistory_GiveDownloadHistoryFoundInDatabaseANonUniqueUserIdAndUniqueFileId_SuccessfulUpdate()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();
            File insertedFile = fileRepo.InsertFile("newInsertFileName", false, false);
            DateTime today = System.DateTime.Today;

            //Update the downloadHistory
            downloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory.DownloadHistoryId, insertedDownloadHistory.UserId, insertedFile.FileId, today);

            //Retrieve updated downloadHistory from DB
            DownloadHistory updatedDownloadHistory = RetrieveDownloadHistory(insertedDownloadHistory.DownloadHistoryId);

            //Run tests
            Assert.NotNull(updatedDownloadHistory);
            Assert.Greater(updatedDownloadHistory.DownloadHistoryId, 0);
            Assert.AreEqual(updatedDownloadHistory.UserId, insertedDownloadHistory.UserId);
            Assert.AreEqual(updatedDownloadHistory.FileId, insertedFile.FileId);
            Assert.AreEqual(updatedDownloadHistory.DownloadTime, today);
        }
    }

    /// <summary>
    /// Testing the update database transaction to see if an exception occurs.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void UpdateDownloadHistory_GiveDownloadHistoryFoundInDatabaseANonUniqueUserIdAndFileId_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory1 = InsertDownloadHistory("InsertUserName1", "InsertFileName1");
            DownloadHistory insertedDownloadHistory2 = InsertDownloadHistory("InsertUserName2", "InsertFileName2");

            //Update downloadHistory1's path to downloadHistory2's path
            downloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory1.DownloadHistoryId, insertedDownloadHistory2.UserId, insertedDownloadHistory2.FileId, insertedDownloadHistory2.DownloadTime);
        }
    }

    /// <summary>
    /// Testing to make sure exception is thrown when trying to update a nonexistent record.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void UpdateDownloadHistory_DownloadHistoryNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            File insertedFile = fileRepo.InsertFile("newInsertFileName", false, false);
            User insertedUser = userRepo.InsertUser("newInsertUserName");
            DateTime today = System.DateTime.Today;

            //Update a downloadHistory that won't be found
            downloadHistoryRepo.UpdateDownloadHistory(-1, insertedUser.UserId, insertedFile.FileId, today);
        }
    }

    /// <summary>
    /// Testing the update database transaction to see if an exception occurs if a non valid User Id is used.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void UpdateDownloadHistory_GiveDownloadHistoryFoundInDatabaseANonValidUserIdAndAValidFileId_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();

            //Update downloadHistory1's path to downloadHistory2's path
            downloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory.DownloadHistoryId, -1, insertedDownloadHistory.FileId, insertedDownloadHistory.DownloadTime);
        }
    }

    /// <summary>
    /// Testing the update database transaction to see if an exception occurs if a non valid File Id is used.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void UpdateDownloadHistory_GiveDownloadHistoryFoundInDatabaseAValidUserIdAndANonValidFileId_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();

            //Update downloadHistory1's path to downloadHistory2's path
            downloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory.DownloadHistoryId, insertedDownloadHistory.UserId, -1, insertedDownloadHistory.DownloadTime);
        }
    }

    /// <summary>
    /// Testing the update database transaction to see if an exception occurs if a non valid File Id and User Id are used.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void UpdateDownloadHistory_GiveDownloadHistoryFoundInDatabaseANonValidUserIdAndFileId_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a downloadHistory to update
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();

            //Update downloadHistory1's path to downloadHistory2's path
            downloadHistoryRepo.UpdateDownloadHistory(insertedDownloadHistory.DownloadHistoryId, -1, -1, insertedDownloadHistory.DownloadTime);
        }
    }

    /// <summary>
    /// Testing the delete transaction. 
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void DeleteDownloadHistory_DownloadHistoryFoundInDatabase_SuccessfulDelete()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            DownloadHistory insertedDownloadHistory = InsertDownloadHistory();
            try
            {
                downloadHistoryRepo.DeleteDownloadHistory(insertedDownloadHistory.DownloadHistoryId);
            }
            catch (DatabaseRecordNotFoundException e)
            {
                Assert.Fail("Tried to delete a database record that didn't exist");
            }

            //The retrieval should throw an exception
            downloadHistoryRepo.GetDownloadHistory(insertedDownloadHistory.DownloadHistoryId);
        }
    }

    /// <summary>
    /// Testing to make sure an DatabaseRecordNotFoundException is thrown
    /// when trying to delete a non-existent downloadHistory
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void DeleteDownloadHistory_DownloadHistoryNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            downloadHistoryRepo.DeleteDownloadHistory(-1);
        }
    }
    #endregion

    #region HELPER METHODS
    /// <summary>
    /// May only be used once within a transaction scope since it uses hardcoded user and file names.
    /// </summary>
    /// <returns></returns>
    private DownloadHistory InsertDownloadHistory()
    {
        User insertedUser = userRepo.InsertUser("InsertUserName");
        File insertedFile = fileRepo.InsertFile("InsertFileName", false, false);

        DateTime today = System.DateTime.Today;
        DownloadHistory insertedDownloadHistory = downloadHistoryRepo.InsertDownloadHistory(insertedUser.UserId, insertedFile.FileId, today);

        Assert.NotNull(insertedDownloadHistory);
        Assert.Greater(insertedDownloadHistory.DownloadHistoryId, 0);
        Assert.AreEqual(insertedDownloadHistory.DownloadTime, today);
        Assert.AreEqual(insertedDownloadHistory.UserId, insertedUser.UserId);
        Assert.AreEqual(insertedDownloadHistory.FileId, insertedFile.FileId);

        return insertedDownloadHistory;
    }

    private DownloadHistory InsertDownloadHistory(string userName, string fileName)
    {
        User insertedUser = userRepo.InsertUser(userName);
        File insertedFile = fileRepo.InsertFile(fileName, false, false);

        DateTime today = System.DateTime.Today;
        DownloadHistory insertedDownloadHistory = downloadHistoryRepo.InsertDownloadHistory(insertedUser.UserId, insertedFile.FileId, today);

        Assert.NotNull(insertedDownloadHistory);
        Assert.Greater(insertedDownloadHistory.DownloadHistoryId, 0);
        Assert.AreEqual(insertedDownloadHistory.DownloadTime, today);
        Assert.AreEqual(insertedDownloadHistory.UserId, insertedUser.UserId);
        Assert.AreEqual(insertedDownloadHistory.FileId, insertedFile.FileId);

        return insertedDownloadHistory;
    }

    private DownloadHistory mockInsertDownloadHistory(int userId, int fileId, DateTime today)
    {
        // User insertedUser = userRepo.InsertUser("InsertUserName");
        // File insertedFile = fileRepo.InsertFile("InsertFileName");

        // DateTime today = System.DateTime.Today;
        DownloadHistory insertedDownloadHistory = mockDownloadHistoryRepo.InsertDownloadHistory(userId, fileId, today);

        Assert.NotNull(insertedDownloadHistory);
        Assert.AreEqual(insertedDownloadHistory.DownloadTime, today);
        Assert.AreEqual(insertedDownloadHistory.UserId, userId);
        Assert.AreEqual(insertedDownloadHistory.FileId, fileId);

        return insertedDownloadHistory;
    }

    private DownloadHistory InsertDownloadHistory(int userId, int fileId, DateTime today)
    {
        DownloadHistory insertedDownloadHistory = downloadHistoryRepo.InsertDownloadHistory(userId, fileId, today);

        Assert.NotNull(insertedDownloadHistory);
        Assert.Greater(insertedDownloadHistory.DownloadHistoryId, 0);
        Assert.AreEqual(insertedDownloadHistory.DownloadTime, today);
        Assert.AreEqual(insertedDownloadHistory.UserId, userId);
        Assert.AreEqual(insertedDownloadHistory.FileId, fileId);

        return insertedDownloadHistory;
    }

    private DownloadHistory RetrieveDownloadHistory(int downloadHistoryId)
    {
        DownloadHistory insertedDownloadHistory = downloadHistoryRepo.GetDownloadHistory(downloadHistoryId);

        Assert.NotNull(insertedDownloadHistory);
        Assert.Greater(insertedDownloadHistory.DownloadHistoryId, 0);

        return insertedDownloadHistory;
    }
    #endregion
}

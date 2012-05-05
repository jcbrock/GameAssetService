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
public class FileTests
{
    private IFileRepository fileRepo;
    private IFileRepository mockFileRepo;
    private RepositoryFactory repositoryFactory;
     string configString = string.Empty;

     public FileTests()
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

        fileRepo = repositoryFactory.GetFileRepository(false);
        Assert.NotNull(fileRepo);

        mockFileRepo = repositoryFactory.GetFileRepository(true); //This returns a test repo that doesn't hit the database
        Assert.NotNull(mockFileRepo);
        // Assert.IsInstanceOf(typeof(FileRepository), fileRepo);
    }

    #region UNIT TESTS
    /// <summary>
    /// Retrieve actual user repository and verify that the right type was returned. Do
    /// the same for the test version of the repository.
    /// 
    /// //This will break if the class is renamed, but I can't get at the actual type because
    /// //the class is "internal", so this will have to do. For example, this doesn't work:
    /// //Assert.IsInstanceOf(typeof(TestFileRepository), fileRepo);
    /// </summary>
    [Test]
    [Category("Unit")]
    public void GetFileRepository_RetrieveTestAndActualRepos_SuccessfulRetrieval()
    {
        fileRepo = repositoryFactory.GetFileRepository(false);
        Assert.NotNull(fileRepo);
        Assert.AreEqual(fileRepo.GetType().Name, "FileRepository");

        fileRepo = repositoryFactory.GetFileRepository(true);
        Assert.NotNull(fileRepo);
        Assert.AreEqual(fileRepo.GetType().Name, "TestFileRepository");
    }
    
    #endregion

    #region INTEGRATION TESTS

    /// <summary>
    /// Testing the object inserted by the mock repository to an inserted object by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertFile_CompareRealInsertToMockInsert_EqualFilesReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a file to update
            File realInsertedFile = InsertFile();
            File mockInsertedFile = mockInsertFile();

            //Run tests
            //Assert.AreEqual(realInsertedFile.FileId, fakeInsertedFile.FileId); a fakeInsertedFile can't really get a FileId
            Assert.AreEqual(realInsertedFile.FilePath, mockInsertedFile.FilePath);
        }
    }

    /// <summary>
    /// Testing the object updated by the mock repository to a updated object by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateFile_CompareRealUpdateToMockUpdate_EqualFilesReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            string fileName = "UpdatedFilePath";

            //Insert a file to update
            File insertedFile = InsertFile();

            //Do updates
            //File realUpdatedFile = fileRepo.UpdateFile(insertedFile.FileId, fileName, false, false);
            //File mockUpdatedFile = mockFileRepo.UpdateFile(insertedFile.FileId, fileName, false, false);

            //Run tests
           // Assert.AreEqual(realUpdatedFile.FileId, mockUpdatedFile.FileId);
            //Assert.AreEqual(realUpdatedFile.FilePath, mockUpdatedFile.FilePath);
        }
    }

    /// <summary>
    /// Testing the object returned by the mock repository to an retrieval by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void RetrieveFile_CompareRealRetrievalToMockRetrieval_EqualFilesReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a file to update
            File insertedFile = InsertFile();

            //Do retreivals
            File realRetrievedFile = fileRepo.GetFile(insertedFile.FileId);
            File mockRetrievedFile = mockFileRepo.GetFile(insertedFile.FileId);

            //Run tests
            Assert.AreEqual(realRetrievedFile.FileId, mockRetrievedFile.FileId);
        }
    }
    /// <summary>
    /// Testing insertion into the database with a unique file name.
    /// The test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertFile_FilePathIsUnique_SuccessfulInsert()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            File insertedFile = InsertFile();
            File retrievedFile = RetrieveFile(insertedFile.FileId);

            Assert.AreEqual(retrievedFile.FileId, insertedFile.FileId);
            Assert.AreEqual(retrievedFile.FilePath, insertedFile.FilePath);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a non unique file path.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertFile_FilePathIsNotUnique_SuccessfulInsert()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            File insertedFile = InsertFile();
            InsertFile(insertedFile.FilePath);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a null file name.
    /// The test should cause an exception.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void InsertFile_FilePathIsNull_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            InsertFile(null);
        }
    }

    /// <summary>
    /// Testing the retrieve database transaction. 
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void RetrieveFile_FileFoundInDatabase_SuccessfulRetrieval()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            File insertedFile = InsertFile();
            File retrievedFile = fileRepo.GetFile(insertedFile.FileId);

            Assert.NotNull(retrievedFile);
            Assert.Greater(retrievedFile.FileId, 0);
            Assert.AreEqual(retrievedFile.FileId, insertedFile.FileId);
            Assert.AreEqual(retrievedFile.FilePath, insertedFile.FilePath);
        }
    }

    /// <summary>
    // Testing to make sure exception is thrown when trying to get a nonexistent record.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void RetrieveFile_FileNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            File retrievedFile = fileRepo.GetFile(-1);
        }
    }

    /// <summary>
    /// Testing the update database transaction.
    /// This test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateFile_GiveFileFoundInDatabaseAUnqiueName_SuccessfulUpdate()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a file to update
            File insertedFile = InsertFile();

            //Update the file
            string updatedFilePath = "updatedFilePath";
            fileRepo.UpdateFile(insertedFile.FileId, updatedFilePath, false, null, false);

            //Retrieve updated file from DB
            File updatedFile = RetrieveFile(insertedFile.FileId);

            //Run tests
            Assert.NotNull(updatedFile);
            Assert.Greater(updatedFile.FileId, 0);
            Assert.AreEqual(updatedFile.FilePath, updatedFilePath);
        }
    }

    /// <summary>
    /// Garbage test atm
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void SaveFile_SaveAnExistingFile_SuccessfulUpdate()
    {
       // using (var scop = new System.Transactions.TransactionScope())
        //{
             File insertedFile = null;
            try
            {
                //Insert a file to update
                insertedFile = InsertFile();
                //File insertedFile2 = InsertFile("C:\\Users\\G521214\\Documents"); //need to use another file cuz we're within
                //one datacontext, so if I change the first file it seems to automatically
                //get updated in the database beforeI call save

                //F it.. I think the fact that I'm using a single datacontext
                // it isn't actually going against the DB or something...
                //Update the file
                //insertedFile.FileId = insertedFile2.FileId;
                insertedFile.FilePath = "updatedFilePath";
                File updatedFile2 = RetrieveFile(insertedFile.FileId);
                fileRepo.SaveFile(insertedFile);

                //Retrieve updated file from DB
                File updatedFile = RetrieveFile(insertedFile.FileId);

                //Run tests
                Assert.NotNull(updatedFile);
                Assert.AreEqual(updatedFile.FileId, insertedFile.FileId);
                Assert.AreEqual(updatedFile.FilePath, insertedFile.FilePath);
            }
            finally
            {
                if (insertedFile != null)
                    fileRepo.DeleteFile(insertedFile.FileId);
            }


        //}
    }

    /// <summary>
    /// Testing the update database transaction to see if an exception occurs.
    /// This test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateFile_GiveFileFoundInDatabaseNonUniqueName_SuccessfulUpdate()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a file to update
            File insertedFile1 = InsertFile("InsertedFileName1");
            File insertedFile2 = InsertFile("InsertedFileName2");

            //Update file1's path to file2's path
            fileRepo.UpdateFile(insertedFile1.FileId, insertedFile2.FilePath, false, null, false);  
        }
    }

    /// <summary>
    /// Testing to make sure exception is thrown when trying to update a nonexistent record.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void UpdateFile_FileNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Update a file that won't be found
            string updatedFilePath = "updatedFilePath";
            fileRepo.UpdateFile(-1, updatedFilePath, false, null, false);
        }
    }

    /// <summary>
    /// Testing the delete transaction. 
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void DeleteFile_FileFoundInDatabase_SuccessfulDelete()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            File insertedFile = InsertFile();
            try
            {
                fileRepo.DeleteFile(insertedFile.FileId);
            }
            catch (DatabaseRecordNotFoundException e)
            {
                Assert.Fail("Tried to delete a database record that didn't exist");
            }

            //The retrieval should throw an exception
            fileRepo.GetFile(insertedFile.FileId);
        }
    }

    /// <summary>
    /// Testing to make sure an DatabaseRecordNotFoundException is thrown
    /// when trying to delete a non-existent file
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void DeleteFile_FileNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            fileRepo.DeleteFile(-1);
        }
    }
    #endregion

    #region HELPER METHODS
    private File InsertFile()
    {
        string fileName = "C:\\Users\\G521214\\Desktop";
        File insertedFile = fileRepo.InsertFile(fileName, false, false);

        Assert.NotNull(insertedFile);
        Assert.Greater(insertedFile.FileId, 0);
        Assert.AreEqual(insertedFile.FilePath, fileName);

        return insertedFile;
    }

    private File mockInsertFile()
    {
        string fileName = "C:\\Users\\G521214\\Desktop";
        File insertedFile = mockFileRepo.InsertFile(fileName, false, false);

        Assert.NotNull(insertedFile);
        //Assert.Greater(insertedFile.FileId, 0);
        Assert.AreEqual(insertedFile.FilePath, fileName);

        return insertedFile;
    }

    private File InsertFile(string fileName)
    {
        File insertedFile = fileRepo.InsertFile(fileName, false, false);

        Assert.NotNull(insertedFile);
        Assert.Greater(insertedFile.FileId, 0);
        Assert.AreEqual(insertedFile.FilePath, fileName);

        return insertedFile;
    }

    private File RetrieveFile(int fileId)
    {
        File insertedFile = fileRepo.GetFile(fileId);

        Assert.NotNull(insertedFile);
        Assert.Greater(insertedFile.FileId, 0);
        Assert.NotNull(insertedFile.FilePath);

        return insertedFile;
    }
    #endregion
}

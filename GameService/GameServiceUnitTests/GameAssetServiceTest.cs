using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Moq;
using NUnit.Framework;
using GameService;
using GameService.ServiceAccessLayer.ServiceImplementations;
using GameService.BusinessObjects.CustomClasses;
using GameService.DataAccessLayer.Contracts;
using GameService.Common;
using GameServiceTests.TestingExtensions;
//Included FTPWrapper in project so I could Mock it get its exceptions  classes
using FTPWrapper.CustomExceptions;
using FTPWrapper;


namespace GameServiceTests
{
    [TestFixture]
    class GameAssetServiceTest
    {
        private Mock<IFileRepository> _mockFileRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IDownloadHistoryRepository> _mockDownloadHistoryRepo;
        private Mock<IFTPWrapperRepository> _mockFtpWrapper;
        private GameAssetService _target2;

        [SetUp]
        public void Setup()
        {
            _mockFileRepo = new Mock<IFileRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockDownloadHistoryRepo = new Mock<IDownloadHistoryRepository>();
            _mockFtpWrapper = new Mock<IFTPWrapperRepository>();
            _target2 = new GameAssetService(_mockFileRepo.Object, _mockUserRepo.Object,
                _mockDownloadHistoryRepo.Object, _mockFtpWrapper.Object);
        }
        //It was a good strategy for me to come up with questions like
        //Well, what if file doesn't exist in DB, in FS, what if its a folder... etc


        //Q: How do I want exceptions to look like? DatabaseRecordNotFound for all dbs?
        //Or more specific?

        //This isn't testing Checkout... well, kind of is (functional test - so I should
        //not use mocking...
        //[Test]
        //[Category("Unit")]
        //[ExpectedException(typeof(DatabaseRecordNotFoundException))]
        //public void CheckOut_FileNotFoundInDatabase_ExceptionThrown()
        //{
        //    //arrange
        //    File file = null;
        //    //File file = new File
        //    //{
        //    //    FileId = 1,
        //    //    FilePath = "foo.mesh"
        //    //};
        //    _mock2.Setup(x => x.GetFile(It.IsAny<int>())).Returns(file);

        //    //act / assert
        //    _target2.Checkout(1, 1);
        //}

        //This isn't testing Checkout... well, kind of is (functional test - so I should
        //not use mocking...
        //[Test]
        //[Category("Unit")]
        //[ExpectedException(typeof(FileNotFoundOnServerException))]
        //public void CheckOut_FileNotFoundOnFileServer_ExceptionThrown()
        //{
        //    //arrange
        //    //arrange
        //    File file = new File
        //        {
        //            FileId = 1,
        //            FilePath = "foo.mesh"
        //        };
        //    _mock2.Setup(x => x.GetFile(It.IsAny<int>())).Returns(file);

        //    string[] directoryContents = new List<string>();
        //    _mock3.Setup(x => x.ListDirectory(It.IsAny<string>())).Returns(directoryContents);

        //    //act / assert
        //    _target2.Checkout(1, 1);
        //}

        //Do I need to test the file that comes back in order to add that code in?
        //Nah - I just need to test that "GetFile" is called once - I can rely on it
        //to do its job because these repos should have testing around them
        //basically like what Barry did

        [Test]
        [Category("Unit")]
        public void Checkout_UserIdLookUpInDatabase_GetUserIsCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user).Verifiable();

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockUserRepo.Verify(p => p.GetUser(It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void Checkout_UserIdFoundInDatabase_CheckoutFileIsCalled()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        [ExpectedException(typeof(UserNotFoundException))]
        public void Checkout_UserIdNotFoundInDatabase_ExceptionThrown()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert (in test header)
        }

        [Test]
        [Category("Unit")]
        public void Checkout_FileIdLookUpInDatabase_GetFileCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.GetFile(It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void Checkout_CheckingOutAFolderWithManyFiles_GetFileCalledManyTimes()
        {
            //arrange
            int numOfFiles = 100.GetRandom(2);
            List<string> directoryNameList = new List<string>();
            for (int i = 0; i < numOfFiles; i++)
                directoryNameList.Add(i.ToString()); //must populate list to get Checkout to iterate over it

            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryNameList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.GetFile(It.IsAny<string>()), Times.Exactly(numOfFiles)); //+1 for parent folder
        }

        [Test]
        [Category("Unit")]
        public void Checkout_CheckingOutAFolder_ListDirectoryCalledOneTime()
        {
            //arrange
            File file = new File
                {
                    FileId = 1,
                    FilePath = "foo.mesh",
                    IsDirectory = true
                };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFtpWrapper.Verify(p => p.ListDirectory(It.IsAny<string>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void Checkout_CheckingOutAFile_ListDirectoryCalledZeroTimes()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFtpWrapper.Verify(p => p.ListDirectory(It.IsAny<string>()), Times.Never());
        }

        [Test]
        [Category("Unit")]
        public void Checkout_CheckingOutAnEmptyFolder_CheckoutFileCalledZeroTimes()
        {
            //arrange
            List<string> directoryList = new List<string>();
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockFileRepo.Setup(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }
        [Test]
        [Category("Unit")]
        public void Checkout_CheckingOutAFolderWithOneFile_CheckoutFileCalledOneTime()
        {
            //arrange
            List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockFileRepo.Setup(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }
        [Test]
        [Category("Unit")]
        public void Checkout_CheckingOutASingleFile_CheckoutFileCalledOneTime()
        {
            //arrange
            List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockFileRepo.Setup(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }
        [Test]
        [Category("Unit")]
        public void Checkout_CheckingOutAFolderWithManyFiles_CheckoutFileCalledManyTimes()
        {
            //arrange            
            int numOfFiles = 100.GetRandom(2);
            List<string> directoryNameList = new List<string>();
            for (int i = 0; i < numOfFiles; i++)
                directoryNameList.Add(i.ToString()); //must populate list to get Checkout to iterate over it

            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryNameList);
            _mockFileRepo.Setup(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.Checkout(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(numOfFiles));
        }

        //[Test]
        //[Category("Unit")]
        //public void Checkout_CheckingOutAFolderWithSubFolders_HowToTestRecursiveCall?()
        //{
        //    //arrange            
        //    int numOfFiles = 100.GetRandom(2);
        //    List<string> directoryNameList = new List<string>();
        //    for (int i = 0; i < numOfFiles; i++)
        //        directoryNameList.Add(i.ToString()); //must populate list to get Checkout to iterate over it

        //    File file = new File
        //    {
        //        FileId = 1,
        //        FilePath = "foo.mesh",
        //        IsDirectory = true
        //    };
        //    User user = new User
        //    {
        //        UserId = 1,
        //        UserName = "foo"
        //    };
        //    _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
        //    _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
        //    _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryNameList);
        //    _mockFileRepo.Setup(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
        //    _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

        //    //act
        //    _target2.Checkout(1, 1);

        //    //assert
        //    _mockFileRepo.Verify(p => p.CheckoutFile(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(numOfFiles + 1)); //+1 for parent folder
        //}

        [Test]
        [Category("Unit")]
        public void GetLatest_UserIdLookUpInDatabase_GetUserIsCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user).Verifiable();

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockUserRepo.Verify(p => p.GetUser(It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_UserIdFoundInDatabase_FileListIsReturned()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            List<string> fileList = _target2.GetLatest(1, 1);

            //assert
            Assert.NotNull(fileList);
        }

        [Test]
        [Category("Unit")]
        [ExpectedException(typeof(UserNotFoundException))]
        public void GetLatest_UserIdNotFoundInDatabase_ExceptionThrown()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.GetLatest(1, 1);

            //assert (in test header)
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_FileIdLookUpInDatabase_GetFileCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.GetFile(It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_CalledOnAFolderWithManyFiles_GetFileCalledManyTimes()
        {
            //arrange
            int numOfFiles = 100.GetRandom(2);
            List<string> directoryNameList = new List<string>();
            for (int i = 0; i < numOfFiles; i++)
                directoryNameList.Add(i.ToString()); //must populate list to get GetLatest to iterate over it

            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryNameList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockFileRepo.Verify(p => p.GetFile(It.IsAny<string>()), Times.Exactly(numOfFiles)); //+1 for parent folder
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_CalledOnAFolder_ListDirectoryCalledOneTime()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockFtpWrapper.Verify(p => p.ListDirectory(It.IsAny<string>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_CalledOnAFile_ListDirectoryCalledZeroTimes()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockFtpWrapper.Verify(p => p.ListDirectory(It.IsAny<string>()), Times.Never());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_CalledOnAnEmptyFolder_EmptyFileListReturned()
        {
            //arrange
            List<string> directoryList = new List<string>();
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            List<string> fileList = _target2.GetLatest(1, 1);

            //assert
            Assert.NotNull(fileList);
            Assert.AreEqual(fileList.Count, 0);
        }
        [Test]
        [Category("Unit")]
        public void GetLatest_CalledOnAFolderWithOneFile_FileListWithOneFileReturned()
        {
            //arrange
            List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            List<string> fileList = _target2.GetLatest(1, 1);

            //assert
            Assert.NotNull(fileList);
            Assert.AreEqual(fileList.Count, 1);
        }
        [Test]
        [Category("Unit")]
        public void GetLatest_CalledOnASingleFile_FileListWithOneFileReturned()
        {
            //arrange
            List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            List<string> fileList = _target2.GetLatest(1, 1);

            //assert
            Assert.NotNull(fileList);
            Assert.AreEqual(fileList.Count, 1);
        }
        [Test]
        [Category("Unit")]
        public void GetLatest_CalledOnAFolderWithManyFiles_FileListWithManyFilesReturned()
        {
            //arrange            
            int numOfFiles = 100.GetRandom(2);
            List<string> directoryNameList = new List<string>();
            for (int i = 0; i < numOfFiles; i++)
                directoryNameList.Add(i.ToString()); //must populate list to get GetLatest to iterate over it

            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryNameList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            List<string> fileList = _target2.GetLatest(1, 1);

            //assert
            Assert.NotNull(fileList);
            Assert.AreEqual(fileList.Count, numOfFiles);
        }

        //Wait a sec... I think actually whether or not a user/file is found in DB matters for DLHistory
        //tests - these tests just care if something is found, right?

        [Test]
        [Category("Unit")]
        public void GetLatest_NoFilesLookedUp_GetDownloadHistoryCalledZeroTimes()
        {
            List<string> directoryList = new List<string>();
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory noneFound = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_NoFilesLookedUp_InsertDownloadHistoryCalledZeroTimes()
        {
            List<string> directoryList = new List<string>();
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory noneFound = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.InsertDownloadHistory(It.IsAny<int>(), It.IsAny<int>()
                , It.IsAny<DateTime>()), Times.Never());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_NoFilesLookedUp_UpdateDownloadHistoryCalledZeroTimes()
        {
            List<string> directoryList = new List<string>();
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory foundHistory = new DownloadHistory
            {
                DownloadHistoryId = 1,
                DownloadTime = System.DateTime.Now,
                FileId = 1,
                UserId = 1
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(foundHistory);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.UpdateDownloadHistory(It.IsAny<int>(), It.IsAny<int>()
                , It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_OneFileNotFoundInDownloadHistory_GetDownloadHistoryCalledOneTime()
        {
            //arrange
            List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory noneFound = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_OneFileNotFoundInDownloadHistory_InsertDownloadHistoryCalledOneTime()
        {
            //arrange
            List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory noneFound = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.InsertDownloadHistory(It.IsAny<int>(), It.IsAny<int>()
                , It.IsAny<DateTime>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_OneFileFoundInDownloadHistory_UpdateDownloadHistoryCalledOneTime()
        {
            List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory foundHistory = new DownloadHistory
            {
                DownloadHistoryId = 1,
                DownloadTime = System.DateTime.Now,
                FileId = 1,
                UserId = 1
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(foundHistory);

            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.UpdateDownloadHistory(It.IsAny<int>(), It.IsAny<int>()
                , It.IsAny<int>(), It.IsAny<DateTime>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_ManyFilesNotFoundInDownloadHistory_GetDownloadHistoryCalledManyTimes()
        {
            //arrange
            int numOfFiles = 100.GetRandom(2);
            List<string> directoryNameList = new List<string>();
            for (int i = 0; i < numOfFiles; i++)
                directoryNameList.Add(i.ToString()); //must populate list to get GetLatest to iterate over it

            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory noneFound = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryNameList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);
            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(numOfFiles));
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_ManyFilesNotFoundInDownloadHistory_InsertDownloadHistoryCalledManyTimes()
        {
            //arrange
            int numOfFiles = 100.GetRandom(2);
            List<string> directoryNameList = new List<string>();
            for (int i = 0; i < numOfFiles; i++)
                directoryNameList.Add(i.ToString()); //must populate list to get GetLatest to iterate over it

            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory noneFound = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryNameList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);
            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.InsertDownloadHistory(It.IsAny<int>(), It.IsAny<int>()
                , It.IsAny<DateTime>()), Times.Exactly(numOfFiles));
        }

        [Test]
        [Category("Unit")]
        public void GetLatest_ManyFilesFoundInDownloadHistory_UpdateDownloadHistoryCalledManyTimes()
        {
            //arrange
            int numOfFiles = 100.GetRandom(2);
            List<string> directoryNameList = new List<string>();
            for (int i = 0; i < numOfFiles; i++)
                directoryNameList.Add(i.ToString()); //must populate list to get GetLatest to iterate over it

            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory foundHistory = new DownloadHistory
            {
                DownloadHistoryId = 1,
                DownloadTime = System.DateTime.Now,
                FileId = 1,
                UserId = 1
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryNameList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(foundHistory);
            //act
            _target2.GetLatest(1, 1);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.UpdateDownloadHistory(It.IsAny<int>(), It.IsAny<int>()
                , It.IsAny<int>(), It.IsAny<DateTime>()), Times.Exactly(numOfFiles));
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_UserIdLookUpInDatabase_GetUserIsCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
           // _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user).Verifiable();

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockUserRepo.Verify(p => p.GetUser(It.IsAny<int>()), Times.AtLeastOnce());
        }

        //[Test]
        //[Category("Unit")]
        //public void CheckIn_UserIdFoundInDatabase_FileListIsReturned()
        //{
        //    //arrange
        //    //File file = new File
        //    //{
        //    //    FileId = 1,
        //    //    FilePath = "foo.mesh",
        //    //    IsDirectory = false
        //    //};
        //    User user = new User
        //    {
        //        UserId = 1,
        //        UserName = "foo"
        //    };
        //    //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
        //    _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

        //    //act
        //    List<KeyValuePair<int, CheckInStatus>> checkInStatusList = _target2.CheckIn(1, null);
            
        //    //assert
        //    Assert.NotNull(checkInStatusList);
        //}

        [Test]
        [Category("Unit")]
        [ExpectedException(typeof(UserNotFoundException))]
        public void CheckIn_UserIdNotFoundInDatabase_ExceptionThrown()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = null;
            //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);

            //act
            _target2.CheckIn(1, file);

            //assert (in test header)
        }

        [Test]
        [Category("Unit")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckIn_NullFilePassedIn_ExceptionThrown()
        {
            //arrange

            //act / assert (in test header)
            _target2.CheckIn(1, null);
        }

        /// <summary>
        /// Note: this is a valid test and not just one that is allowing
        /// me to write code because saving of the file needs to be done
        /// no matter what (if new insert, if old then it could have been
        /// renamed...etc)
        /// </summary>
        [Test]
        [Category("Unit")]        
        public void CheckIn_NonNullFilePassedIn_SaveFileCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            // _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user).Verifiable();

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockFileRepo.Verify(p => p.SaveFile(It.IsAny<File>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_SaveFileReturnedNull_ErrorOccurredCheckInStatusReturned()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            // _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user).Verifiable();

            //act / assert
            Assert.AreEqual(_target2.CheckIn(1, file), CheckInStatus.ErrorOccurred);   
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_NonNullFilePassedIn_GetDownloadHistoryCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
           // DownloadHistory history = null;
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file).Verifiable();
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
           // _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_NoDownloadHistoryFound_InsertDownloadHistoryCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = null;
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
           // _mockDownloadHistoryRepo.Setup(p => p.InsertDownloadHistory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(history).Verifiable();

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.InsertDownloadHistory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_NoDownloadHistoryFoundAndPathIsAvailable_SuccessfulCheckInStatusReturned()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = null;
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);

            //act / assert
            Assert.AreEqual(_target2.CheckIn(1, file), CheckInStatus.FileNotFoundOnServer);                     
        }

        

        //Think about it... So we have now, download time, and server time
        //success = download time > server time
        //outdated = download time < server time
        //now is only good for updating download time if successful (or in the case of an insert: FileNotFoundOnServer)
        [Test]
        [Category("Unit")]
        public void CheckIn_DownloadHistoryFound_GetUploadTimeCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = new DownloadHistory
            {
                DownloadHistoryId = 1,
                UserId = 1,
                FileId = 1,
                DownloadTime = DateTime.Now
            };
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
            // _mockDownloadHistoryRepo.Setup(p => p.InsertDownloadHistory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(history).Verifiable();

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockFtpWrapper.Verify(p => p.GetUploadTime(It.IsAny<string>()), Times.Once());
        }
        
        [Test]
        [Category("Unit")]
        public void CheckIn_DownloadHistoryTimeGreaterThanServerTime_SuccessfulCheckInStatusReturned()
        {
            //arrange
                    File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = new DownloadHistory
            {
                DownloadHistoryId = 1,
                UserId = 1,
                FileId = 1,
                DownloadTime = DateTime.Now
            };
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
            _mockFtpWrapper.Setup(p => p.GetUploadTime(It.IsAny<string>())).Returns(DateTime.Now.AddDays(-1));

             //act / assert
            Assert.AreEqual(_target2.CheckIn(1, file), CheckInStatus.Successful);
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_DownloadHistoryTimeEqualToServerTime_SuccessfulCheckInStatusReturned()
        {
            //arrange
            DateTime compareTimes = DateTime.Now;
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = new DownloadHistory
            {
                DownloadHistoryId = 1,
                UserId = 1,
                FileId = 1,
                DownloadTime = compareTimes
            };
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
            _mockFtpWrapper.Setup(p => p.GetUploadTime(It.IsAny<string>())).Returns(compareTimes);

            //act / assert
            Assert.AreEqual(_target2.CheckIn(1, file), CheckInStatus.Successful);
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_DownloadHistoryTimeGreaterThanServerTime_UpdateDownloadHistoryToNowCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = new DownloadHistory
            {
                DownloadHistoryId = 1,
                UserId = 1,
                FileId = 1,
                DownloadTime = DateTime.Now
            };
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
            _mockFtpWrapper.Setup(p => p.GetUploadTime(It.IsAny<string>())).Returns(DateTime.Now.AddDays(-1));

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.UpdateDownloadHistory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                   It.Is<DateTime>(j => (j > DateTime.Now.AddSeconds(-5) && j < DateTime.Now.AddSeconds(5)))), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_DownloadHistoryTimeEqualToServerTime_UpdateDownloadHistoryToNowCalledOnce()
        {
            //arrange
            DateTime compareTimes = DateTime.Now;
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = new DownloadHistory
            {
                DownloadHistoryId = 1,
                UserId = 1,
                FileId = 1,
                DownloadTime = compareTimes
            };
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
            _mockFtpWrapper.Setup(p => p.GetUploadTime(It.IsAny<string>())).Returns(compareTimes);

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.UpdateDownloadHistory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
              It.Is<DateTime>(j => (j > DateTime.Now.AddSeconds(-5) && j < DateTime.Now.AddSeconds(5)))), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_DownloadHistoryTimeLessThanServerTime_NewerVersionFoundOnServerCheckInStatusReturned()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = new DownloadHistory
            {
                DownloadHistoryId = 1,
                UserId = 1,
                FileId = 1,
                DownloadTime = DateTime.Now
            };
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
            _mockFtpWrapper.Setup(p => p.GetUploadTime(It.IsAny<string>())).Returns(DateTime.Now.AddDays(1));

            //act / assert
            Assert.AreEqual(_target2.CheckIn(1, file), CheckInStatus.NewerVersionFoundOnServer);
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_DownloadHistoryTimeLessThanServerTime_UpdateDownloadHistoryCalledZeroTimes()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = new DownloadHistory
            {
                DownloadHistoryId = 1,
                UserId = 1,
                FileId = 1,
                DownloadTime = DateTime.Now
            };
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
            _mockFtpWrapper.Setup(p => p.GetUploadTime(It.IsAny<string>())).Returns(DateTime.Now.AddDays(1));

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.UpdateDownloadHistory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never());
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_CheckInStatusSuccessful_RemoveCheckoutLock()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = new DownloadHistory
            {
                DownloadHistoryId = 1,
                UserId = 1,
                FileId = 1,
                DownloadTime = DateTime.Now
            };
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
            _mockFtpWrapper.Setup(p => p.GetUploadTime(It.IsAny<string>())).Returns(DateTime.Now.AddDays(-1));

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockFileRepo.Verify(p => p.CheckInFile(It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_CheckInStatusFileNotFoundOnServer_RemoveCheckoutLock()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = null;
            _mockFileRepo.Setup(p => p.SaveFile(It.IsAny<File>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);
        
            //act
            _target2.CheckIn(1, file);

            //assert
            _mockFileRepo.Verify(p => p.CheckInFile(It.IsAny<int>()), Times.Once());
        }


        [Test]
        [Category("Unit")]
        public void CheckIn_ValidParametersPassedIn_GetFileCalledOnce()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false,
                CheckedOut = true,
                UserCheckedOutTo = 12345
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            //  DownloadHistory history = null;
         //   _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            //  _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockFileRepo.Verify(p => p.GetFile(It.IsAny<int>()), Times.Once());
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_FileIsCheckedOutToDifferentUser_FileCheckedOutBySomeoneElseCheckInStatusReturned()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false,
                CheckedOut = true,
                UserCheckedOutTo = 12345
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
          //  DownloadHistory history = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
          //  _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);

            //act / assert
            Assert.AreEqual(_target2.CheckIn(1, file), CheckInStatus.FileCheckedOutBySomeoneElse);
        }

        [Test]
        [Category("Unit")]
        public void CheckIn_FileIsCheckedOutToSameUser_SuccessfulCheckInStatusReturned()
        {
            //arrange
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false,
                CheckedOut = true,
                UserCheckedOutTo = 1
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory history = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(history);

            //act / assert
            Assert.AreNotEqual(_target2.CheckIn(1, file), CheckInStatus.FileCheckedOutBySomeoneElse);
        }

        
       // [Test]
        [Category("Unit")]
        public void CheckIn_NoFileIdPassedIn_GetDownloadHistoryCalledZeroTimes()
        {
            //List<string> directoryList = new List<string>();
            //File file = new File
            //{
            //    FileId = 1,
            //    FilePath = "foo.mesh",
            //    IsDirectory = true
            //};
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
           // DownloadHistory noneFound = null;
            //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
           // _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
           // _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);

            //act
            _target2.CheckIn(1, null);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

       // [Test]
        [Category("Unit")]
        public void CheckIn_NoFileIdPassedIn_InsertDownloadHistoryCalledOneTime()
        {
             //arrange
            //List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = true
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
             DownloadHistory noneFound = null;
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
          //  _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);

            //act 
            _target2.CheckIn(1, null);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.InsertDownloadHistory(It.IsAny<int>(), It.IsAny<int>(),
               It.IsAny<DateTime>()), Times.Once());
        }

       // [Test]
        [Category("Unit")]
        public void CheckIn_NoFileIdPassedIn_GetFileCalledZeroTimes()
        {
            //List<string> directoryList = new List<string>();
            //File file = new File
            //{
            //    FileId = 1,
            //    FilePath = "foo.mesh",
            //    IsDirectory = true
            //};
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
           // DownloadHistory noneFound = null;
           // _mockFileRepo.Setup(p => p.GetFile(It.Is<int?>(null)));
//_mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
           // _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            //_mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);

            //act
            _target2.CheckIn(1, null);

            //assert
            _mockFileRepo.Verify(p => p.GetFile(It.IsAny<int>()), Times.Never());
        }

       // [Test]
        [Category("Unit")]
        public void CheckIn_NoFileIdPassedIn_InsertFileCalledOneTime()
        {
            //List<string> directoryList = new List<string>();
            //File file = new File
            //{
            //    FileId = 1,
            //    FilePath = "foo.mesh",
            //    IsDirectory = true
            //};
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            // DownloadHistory noneFound = null;
            // _mockFileRepo.Setup(p => p.GetFile(It.Is<int?>(null)));
            //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            // _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            //_mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(noneFound);

            //act
            _target2.CheckIn(1, null);

            //assert
            _mockFileRepo.Verify(p => p.InsertFile(It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<bool>()), Times.Once());
        }

       // [Test]
        [Category("Unit")]
        public void CheckIn_FileIdPassedIn_GetFileCalledOneTimes()
        {
            //List<string> directoryList = new List<string>();
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory foundHistory = new DownloadHistory
            {
                DownloadHistoryId = 1,
                DownloadTime = System.DateTime.Now,
                FileId = 1,
                UserId = 1
            };
            // _mockFileRepo.Setup(p => p.GetFile(It.Is<int?>(null)));
            //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
            // _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(foundHistory);

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockFileRepo.Verify(p => p.GetFile(It.IsAny<int>()), Times.Once());
        }

       // [Test]
        [Category("Unit")]
        public void CheckIn_FileIdPassedIn_GetDownloadHistoryCalledOneTime()
        {
            //arrange
            //List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
            DownloadHistory foundHistory = new DownloadHistory
            {
                DownloadHistoryId = 1,
                DownloadTime = System.DateTime.Now,
                FileId = 1,
                UserId = 1
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
          //  _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(foundHistory);

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        //[Test]
        [Category("Unit")]
        public void CheckIn_FileIdPassedIn_UpdateDownloadHistoryCalledOneTime()
        {
             //arrange
            //List<string> directoryList = new List<string>() { "file1.txt" };
            File file = new File
            {
                FileId = 1,
                FilePath = "foo.mesh",
                IsDirectory = false
            };
            User user = new User
            {
                UserId = 1,
                UserName = "foo"
            };
             DownloadHistory foundHistory = new DownloadHistory
            {
                DownloadHistoryId = 1,
                DownloadTime = System.DateTime.Now,
                FileId = 1,
                UserId = 1
            };
            _mockFileRepo.Setup(p => p.GetFile(It.IsAny<int>())).Returns(file);
            //_mockFileRepo.Setup(p => p.GetFile(It.IsAny<string>())).Returns(file);
          //  _mockFtpWrapper.Setup(p => p.ListDirectory(It.IsAny<string>())).Returns(directoryList);
            _mockUserRepo.Setup(p => p.GetUser(It.IsAny<int>())).Returns(user);
            _mockDownloadHistoryRepo.Setup(p => p.GetDownloadHistory(It.IsAny<int>(), It.IsAny<int>())).Returns(foundHistory);

            //act
            _target2.CheckIn(1, file);

            //assert
            _mockDownloadHistoryRepo.Verify(p => p.UpdateDownloadHistory(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<DateTime>()), Times.Once());
        }
    }
      
}

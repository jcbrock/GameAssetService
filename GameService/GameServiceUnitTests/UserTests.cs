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
//Ok, so after talking with Dad it seems impossible to "unit test" CRUD operations, unless you're using some sort of mocking database framework? (maybe mog?)
//So I'm going to call them integration tests (maybe with a little bit of regression)
//Now that brings up the question of how to setup my tests structure
//  Group by class/object (User stuff goes here, File stuff goes there...) what about the public facing stuff?
//  Eh, I can't think of a better way to group it...
//  Organize Unit Test / Regression Tests / Integration Tests by categories

//Unit Test - very small in scope, one operation or thing, ideally should be quick
//Functional Test - Feed in input, examine output (black box)
//Regression Test - Put in place after a bug fix to ensure that a bug doesn't remerge (or a new bug that emerges after a fix is made somewhere else)
//Integration Test - Goes accross multiple systems / classes
//White box - usually unit level, could be integration
//Black box - functional

//Definition on these tests can be such a grey area though... for example, some people have STRICT rules that a unit test can never
//talk to a DB, communicate across the network, touch the file system, run at the same time as any of your other unit tests, or if you
//have to do some environment things (like edit config files) to run it

//I'm going to call CRUD tests as integration since they test between two systems (web service and database), it requires
//and extranal dependency.

//I could unit test calling the CRUD functions w/o going to the database, but remember what you're testing there. You're not testing
//the database, but instead you are testing the function and what it returns - which is a functional test. 


//when do I want to throw an error vs just returning null?
//well, you have to think at what level this is. This is a service... updates can fail
//because of bad input... not my fault. The UI should be the one that handles those failures
//because they will also want to be notified that their update didn't work...

//do i throw an exception though? that seems kind of harsh... but at the same time it
//doesn't assume that the user will know a null equals update failed.
//I think the UI is the place for handling that stuff

//I think it is conveient for the functions to return the User object (I think delete should probably be changed to void because of following reason)
//However I think exceptions should be thrown with the functions fail in their actions - it may be the input's fault, but exceptions should
//still be thrown to a) give the calling code details of the failure, not just that one occured b) the operation failed in its action, exceptions
//are expected in that case. Its not like a lot of exceptions are expected (so don't do Tester-Doer pattern) - it is just clear on how exceptions
//could occur. (Crap... I'm doing the Tester-Doer pattern to throw a custom exception (worst of both worlds performance wise :/ )

//Now, do I use a custom error that I made... or perhaps a System.Execption one?
//I think it depends on what type of exception we are looking for... but I think in the case of missing a database object
//then I htink we should create our own because there isn't one that is similar. 

//Performance wise - use Tester-Doer pattern if lots of exceptions are expected, otherwise just use exception handling

//nope, SubmitChanges does not error out if there are no pending changes

//Test names: MethodName_StateUnderTest_ExpectedBehavior
[TestFixture]
public class UserTests
{
    //keeping the reflection stuff so I have it when I need it later... probably won't need it in "UserTests" though...
    //public GameAssetService reflectionObject = new GameAssetService();
    private IUserRepository userRepo;
    private IUserRepository mockUserRepo;
    private RepositoryFactory repositoryFactory;
    string configString = string.Empty;

    public UserTests()
    {
        //Get connection string from config file
        XDocument xmlDoc = XDocument.Load("..\\..\\config.xml");
        configString = xmlDoc.Descendants("configuration").Elements("configurationString").First().Value;
    }

    [SetUp]
    public void Setup()
    {
        //RepositoryFactory is public... should that be the casE?   
        //Uhh... crap, maybe that is okay because you would need to DLL to get it I believe
        //obivously it doesn't get exposed as part of the service. Well wtf am I so concerned about then...
        //well, stuff should be limited as much as possible
        //I'm able to hide the repos outside of the assembly, which forces the use of the factory
        //Its fine if the factory is public imo... use that to get all these repos, use those for testing
        //the data access layer part of the service
        //testing the Service Access layer should be easy... those methods should pretty much all be public or just use reflection

        repositoryFactory = new RepositoryFactory(configString);
        Assert.NotNull(repositoryFactory);

        userRepo = repositoryFactory.GetUserRepository(false);
        Assert.NotNull(userRepo);

        mockUserRepo = repositoryFactory.GetUserRepository(true); //This returns a test repo that doesn't hit the database      
        Assert.NotNull(mockUserRepo);
        // Assert.IsInstanceOf(typeof(UserRepository), userRepo);
    }



    //[Test]
    //public void InsertUserWithRollback()
    //{
    //    using (var scop = new System.Transactions.TransactionScope())
    //    {
    //        GameAssetService proxy = new GameAssetService();
    //        string user = "InsertUserWithRollback";
    //        MethodInfo reflectedMethod = GetMethod("InsertNewUser");

    //        //should I make the parameter getting/setting more automatic? or do I want that to fail if it is changed...
    //        object[] methodParms = new object[1];
    //        methodParms[0] = user;
    //        object result = reflectedMethod.Invoke(proxy, methodParms);

    //        Assert.NotNull(result);

    //        if (result != null)
    //        {
    //            User newUser = (User)result;
    //            Assert.Greater(newUser.UserId, 0);
    //            Assert.AreEqual(newUser.UserName, user);
    //        }     
    //        // to commit at the very end of this block,        // you would call        // scop.Complete();  // ..... but don't and all will be rolled back 
    //    }

    //}
    #region UNIT TESTS
    /// <summary>
    /// Retrieve actual user repository and verify that the right type was returned. Do
    /// the same for the test version of the repository.
    /// 
    /// //This will break if the class is renamed, but I can't get at the actual type because
    /// //the class is "internal", so this will have to do. For example, this doesn't work:
    /// //Assert.IsInstanceOf(typeof(TestUserRepository), userRepo);
    /// </summary>
    [Test]
    [Category("Unit")]
    public void GetUserRepository_RetrieveTestAndActualRepos_SuccessfulRetrieval()
    {
        userRepo = repositoryFactory.GetUserRepository(false);
        Assert.NotNull(userRepo);
        Assert.AreEqual(userRepo.GetType().Name, "UserRepository");

        userRepo = repositoryFactory.GetUserRepository(true);
        Assert.NotNull(userRepo);
        Assert.AreEqual(userRepo.GetType().Name, "TestUserRepository");
    }
    #endregion

    #region INTEGRATION TESTS

    /// <summary>
    /// Testing the object inserted by the mock repository to an inserted object by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertUser_CompareRealInsertToMockInsert_EqualUsersReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a user to update
            User realInsertedUser = InsertUser();
            User mockInsertedUser = mockInsertUser();

            //Run tests
            //Assert.AreEqual(realInsertedUser.UserId, fakeInsertedUser.UserId); a fakeInsertedUser can't really get a UserId
            Assert.AreEqual(realInsertedUser.UserName, mockInsertedUser.UserName);
        }
    }

    /// <summary>
    /// Testing the object updated by the mock repository to a updated object by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateUser_CompareRealUpdateToMockUpdate_EqualUsersReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            string userName = "UpdatedUserName";

            //Insert a user to update
            User insertedUser = InsertUser();

            //Do updates
            User realUpdatedUser = userRepo.UpdateUser(insertedUser.UserId, userName);
            User mockUpdatedUser = mockUserRepo.UpdateUser(insertedUser.UserId, userName);

            //Run tests
            Assert.AreEqual(realUpdatedUser.UserId, mockUpdatedUser.UserId);
            Assert.AreEqual(realUpdatedUser.UserName, mockUpdatedUser.UserName);
        }
    }

    /// <summary>
    /// Testing the object returned by the mock repository to an retrieval by the real repository
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void RetrieveUser_CompareRealRetrievalToMockRetrieval_EqualUsersReturned()
    {
        //test returned mock object to what would be returned from database - this tests the mock repo
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a user to update
            User insertedUser = InsertUser();

            //Do retreivals
            User realRetrievedUser = userRepo.GetUser(insertedUser.UserId);
            User mockRetrievedUser = mockUserRepo.GetUser(insertedUser.UserId);

            //Run tests
            Assert.AreEqual(realRetrievedUser.UserId, mockRetrievedUser.UserId);
        }
    }
    /// <summary>
    /// Testing insertion into the database with a unique user name.
    /// The test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void InsertUser_UserNameIsUnique_SuccessfulInsert()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            User insertedUser = InsertUser();
            User retrievedUser = RetrieveUser(insertedUser.UserId);

            Assert.AreEqual(retrievedUser.UserId, insertedUser.UserId);
            Assert.AreEqual(retrievedUser.UserName, insertedUser.UserName);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a non unique user name.
    /// The test should cause an exception.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void InsertUser_UserNameIsNotUnique_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            User insertedUser = InsertUser();
            InsertUser(insertedUser.UserName);
        }
    }

    /// <summary>
    /// Testing insertion into the database with a null user name.
    /// The test should cause an exception.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void InsertUser_UserNameIsNull_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            InsertUser(null);
        }
    }

    /// <summary>
    /// Testing the retrieve database transaction. 
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void RetrieveUser_UserFoundInDatabase_SuccessfulRetrieval()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            User insertedUser = InsertUser();
            User retrievedUser = userRepo.GetUser(insertedUser.UserId);

            Assert.NotNull(retrievedUser);
            Assert.Greater(retrievedUser.UserId, 0);
            Assert.AreEqual(retrievedUser.UserId, insertedUser.UserId);
            Assert.AreEqual(retrievedUser.UserName, insertedUser.UserName);
        }
    }

    /// <summary>
    // Testing to make sure exception is thrown when trying to get a nonexistent record.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void RetrieveUser_UserNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            User retrievedUser = userRepo.GetUser(-1);
        }
    }

    /// <summary>
    /// Testing the update database transaction.
    /// This test should pass.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    public void UpdateUser_GiveUserFoundInDatabaseAUnqiueName_SuccessfulUpdate()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a user to update
            User insertedUser = InsertUser();

            //Update the user
            string updatedUserName = "updatedUserName";
            userRepo.UpdateUser(insertedUser.UserId, updatedUserName);

            //Retrieve updated user from DB
            User updatedUser = RetrieveUser(insertedUser.UserId);

            //Run tests
            Assert.NotNull(updatedUser);
            Assert.Greater(updatedUser.UserId, 0);
            Assert.AreEqual(updatedUser.UserName, updatedUserName);
        }
    }

    /// <summary>
    /// Testing the update database transaction to see if an exception occurs.
    /// This test should cause an exception.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(SqlException))]
    public void UpdateUser_GiveUserFoundInDatabaseNonUniqueName_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Insert a user to update
            User insertedUser1 = InsertUser("InsertUserName1");
            User insertedUser2 = InsertUser("InsertUserName2");

            //Update the insertedUser1 to insertedUser2's user name
            userRepo.UpdateUser(insertedUser1.UserId, insertedUser2.UserName);
        }
    }

    /// <summary>
    /// Testing to make sure exception is thrown when trying to update a nonexistent record.
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void UpdateUser_UserNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            //Update a user that won't be found
            string updatedUserName = "updatedUserName";
            userRepo.UpdateUser(-1, updatedUserName);
        }
    }

    /// <summary>
    /// Testing the delete transaction. 
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void DeleteUser_UserFoundInDatabase_SuccessfulDelete()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            User insertedUser = InsertUser();
            try
            {
                userRepo.DeleteUser(insertedUser.UserId);
            }
            catch (DatabaseRecordNotFoundException e)
            {
                Assert.Fail("Tried to delete a database record that didn't exist");
            }

            //The retrieval should throw an exception
            userRepo.GetUser(insertedUser.UserId);
        }
    }

    /// <summary>
    /// Testing to make sure an DatabaseRecordNotFoundException is thrown
    /// when trying to delete a non-existent user
    /// </summary>
    [Test]
    [Category("Integration (CRUD Tests)")]
    [ExpectedException(typeof(DatabaseRecordNotFoundException))]
    public void DeleteUser_UserNotFoundInDatabase_ExceptionThrown()
    {
        using (var scop = new System.Transactions.TransactionScope())
        {
            userRepo.DeleteUser(-1);
        }
    }
    #endregion

    #region HELPER METHODS
    private User InsertUser()
    {
        string userName = "InsertUserName";
        User insertedUser = userRepo.InsertUser(userName);

        Assert.NotNull(insertedUser);
        Assert.Greater(insertedUser.UserId, 0);
        Assert.AreEqual(insertedUser.UserName, userName);

        return insertedUser;
    }

    private User mockInsertUser()
    {
        string userName = "InsertUserName";
        User insertedUser = mockUserRepo.InsertUser(userName);

        Assert.NotNull(insertedUser);
        //Assert.Greater(insertedUser.UserId, 0);
        Assert.AreEqual(insertedUser.UserName, userName);

        return insertedUser;
    }

    private User InsertUser(string userName)
    {
        User insertedUser = userRepo.InsertUser(userName);

        Assert.NotNull(insertedUser);
        Assert.Greater(insertedUser.UserId, 0);
        Assert.AreEqual(insertedUser.UserName, userName);

        return insertedUser;
    }

    private User RetrieveUser(int userId)
    {
        User insertedUser = userRepo.GetUser(userId);

        Assert.NotNull(insertedUser);
        Assert.Greater(insertedUser.UserId, 0);

        return insertedUser;
    }


    //private User InsertUserSetup()
    //{
    //    User insertedUser = null;
    //    string userName = "InsertUserUnitTest";
    //    try
    //    {
    //        insertedUser = InsertUser(userName); //handles transaction test, returned object validity test
    //    }
    //    catch (AssertionException e)
    //    {
    //        Assert.Inconclusive("Insert failed");
    //    }

    //    return insertedUser;
    //}

    /// <summary>
    /// Must catch if retrieval database action fails as well as
    /// check the validity of the returned object.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="expectedUser"></param>
    /// <returns>Returns a valid User or sets calling test to inconclusive</returns>
    //private User RetrieveUserSetup(int userId, User expectedUser)
    //{
    //    User retrievedUser = null;
    //    try
    //    {
    //        retrievedUser = RetrieveUser(userId); //handles transaction test
    //    }
    //    catch (AssertionException e)
    //    {
    //        Assert.Inconclusive("Retrieved User corrupted");
    //    }

    //    if (retrievedUser != expectedUser)
    //        Assert.Inconclusive("Returned User didn't match expected");

    //    return retrievedUser;
    //}

    //private MethodInfo GetMethod(string methodName)
    //{
    //    if (string.IsNullOrWhiteSpace(methodName))
    //        Assert.Fail("methodName cannot be null or whitespace");

    //    var method = this.reflectionObject.GetType()
    //        .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

    //    if (method == null)
    //        Assert.Fail(string.Format("{0} method not found", methodName));

    //    return method;
    //}
    #endregion
}

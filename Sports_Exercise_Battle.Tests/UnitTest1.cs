using Sports_Exercise_Battle.Database;
using Sports_Exercise_Battle.Models.Users;

namespace Sports_Exercise_Battle.Tests
{
    [TestFixture]
    public class Tests
    {
        DatabaseHandler _dbHandler = DatabaseHandler.Instance;

        public Tests()
        {
            _dbHandler.Connect();
        }

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
            _dbHandler.DeleteAllUserStats();
            _dbHandler.DeleteAllUser();
        }


        [Test]
        public void TestDB_RegisterUser()
        {
            // Create a new user object
            User newUser = new User("a", "test", "Ahmed", "my name is ahmed", ":-P");

            // Register the new user
            _dbHandler.RegisterUser(newUser);

            // Received user information from the database
            User receivedUser = _dbHandler.GetUserByUsername("a");

            // Assert that the retrieved user data matches the expected results
            Assert.AreEqual("a", receivedUser.Username);
            Assert.AreEqual("", receivedUser.Password);
            Assert.AreEqual("Ahmed", receivedUser.Name);
            Assert.AreEqual("my name is ahmed", receivedUser.Bio);
            Assert.AreEqual(":-P", receivedUser.Image);
           
        }

        [Test]
        public void TestDB_RegisterUser_WithExistingUsername_ThrowsException()
        {
            // Arrange
            User user1 = new User("d", "test", "Daniel", "Bio of Daniel", ":-|");
            _dbHandler.RegisterUser(user1);

            // Act & Assert
            User user2 = new User("d", "test123", "Danny", "Bio of Danny", ":-/");
            var ex = Assert.Throws<Exception>(() => _dbHandler.RegisterUser(user2));
            Assert.IsNotNull(ex);
            Assert.That(ex.Message, Is.EqualTo("User already exists!"));
        }

        [Test]
        public void TestDB_NotExistingUser()
        {
            User? randomUser = _dbHandler.GetUserByUsername("Thomas");

            Assert.AreEqual(null, randomUser);
        }
        [Test]
        public void TestDB_UserStats()
        {
            // Create a new user object
            User newUser = new User("a", "test", "Ahmed", "my name is ahmed", ":-P");

            // Register the new user
            _dbHandler.RegisterUser(newUser);

            // Received user information from the database
            UserStats receivedStats = _dbHandler.GetUserStatsByUsername("a");

            //Assert that the retrieved user stats match the expected results
            Assert.AreEqual("a", receivedStats.Username);
            Assert.AreEqual(0, receivedStats.Wins);
            Assert.AreEqual(0, receivedStats.Losses);
            Assert.AreEqual(100, receivedStats.Elo);
        }

        [Test]
        public void TestDB_UserStats_NotExistingUser()
        {
            UserStats? randomUserStats = _dbHandler.GetUserStatsByUsername("Ahmed");

            Assert.AreEqual(null, randomUserStats);
        }


        [Test]
        public void TestDB_Login_RightCredentials()
        {
            User b = new User("b", "test", "Ahmed", "my name is ahmed", ":-P");

            // Register the new user
            _dbHandler.RegisterUser(b);

            string? token = _dbHandler.LoginUser("b", "test");
            Assert.AreEqual("b-sebToken", token);
           

        }

        [Test]
        public void TestDB_Login_FalseCredentials_ThrowsException()
        {

            User a = new User("a", "test", "Ahmed", "my name is ahmad", ":-a");

            //    // Register the new user
            _dbHandler.RegisterUser(a);


            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _dbHandler.LoginUser("a", "wrong"));
            Assert.IsNotNull(ex);
            Assert.That(ex.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public void TestDB_AuthorizeToken_ExistingToken()
        {
            User b = new User("b", "test", "Ahmed", "my name is ahmed", ":-P");

            // Register the new user
            _dbHandler.RegisterUser(b);
            // Login User to generate Token
            _dbHandler.LoginUser("b", "test");

            _dbHandler.Token = "b-sebToken";

            Assert.AreEqual(true, _dbHandler.AuthorizeToken());
            Assert.AreEqual("b", _dbHandler.AuthorizedUser);
        }

        [Test]
        public void TestDB_AuthorizeToken_NoExistingToken()
        {
            _dbHandler.Token = "lol-sebToken";

            Assert.AreEqual(false, _dbHandler.AuthorizeToken());
            Assert.AreEqual(null, _dbHandler.AuthorizedUser);
        }


        [Test]
        public void TestDB_UpdateUser_UpdatesSuccessfully()
        {
            // Arrange
            User user = new User("c", "test", "Charlie", "Bio of Charlie", ":-O");
            _dbHandler.RegisterUser(user);

            // Act
            user.Name = "Charles";
            user.Bio = "Updated bio of Charles";
            _dbHandler.UpdateUser(user);

            // Assert
            User updatedUser = _dbHandler.GetUserByUsername("c");
            Assert.AreEqual("Charles", updatedUser.Name);
            Assert.AreEqual("Updated bio of Charles", updatedUser.Bio);
        }


        


    }
}
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Npgsql;
//using Sports_Exercise_Battle.Database;
//using Sports_Exercise_Battle.Models.Users;

//namespace Sports_Exercise_Battle.Tests
//{
//    [TestClass]
//    public class UnitTest1
//    {
//        [TestMethod]
//        public void TestDatabase_UserCanLogin()
//        {
//            // Arrange
//            var dbHandler = new DatabaseHandler(); // Directly using the real database handler
//            var expectedUsername = "kienboec";
//            var expectedToken = "kienboec-sebToken"; // The token you expect to be generated

//            // Act
//            string actualToken = dbHandler.LoginUser(expectedUsername, "daniel");

//            // Assert
//            Assert.AreEqual(expectedToken, actualToken, "The token should match the expected value.");
//        }





//        private Mock<NpgsqlConnection> _mockConnection;
//        private Mock<NpgsqlCommand> _mockCommand;
//        private Mock<NpgsqlDataReader> _mockDataReader;

//        [TestInitialize]
//        public void Setup()
//        {
//            // Mock all required database objects
//            _mockConnection = new Mock<NpgsqlConnection>();
//            _mockCommand = new Mock<NpgsqlCommand>();
//            _mockDataReader = new Mock<NpgsqlDataReader>();

//            _mockConnection.Setup(conn => conn.CreateCommand()).Returns(_mockCommand.Object);
//            _mockCommand.Setup(cmd => cmd.ExecuteReader(It.IsAny<System.Data.CommandBehavior>())).Returns(_mockDataReader.Object);
//            _mockCommand.SetupProperty(cmd => cmd.CommandText);
//            _mockCommand.SetupProperty(cmd => cmd.Connection);
//        }
//        [TestMethod]
//        public void Test_RegisterUser_Success()
//        {
//            var dbHandler = new DatabaseHandler(_mockConnection.Object); // Ideally use a mock or fake
//            var user = new User("testuser", "password123", "Test", "Bio", "Image.jpg");

//            int result = dbHandler.RegisterUser(user);

//            // Assert - Check that the user was added successfully
//            Assert.AreEqual(0, result, "User should be registered successfully.");
//        }

//        [TestMethod]
//        public void GetUserByID_ReturnsUser_WhenUserExists()
//        {
//            // Arrange
//            var handler = new DatabaseHandler(_mockConnection.Object);
//            _mockDataReader.SetupSequence(x => x.Read()).Returns(true).Returns(false); // First call returns true (data exists), then false (end of data)
//            _mockDataReader.Setup(x => x["username"]).Returns("john");
//            _mockDataReader.Setup(x => x["name"]).Returns("John Doe");

//            // Act
//            var result = handler.GetUserByID("john");

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("john", result.Username);
//            Assert.AreEqual("John Doe", result.Name);
//        }

//        [TestMethod]
//        public void GetUserByID_ReturnsNull_WhenUserDoesNotExist()
//        {
//            // Arrange
//            var handler = new DatabaseHandler(_mockConnection.Object);
//            _mockDataReader.Setup(x => x.Read()).Returns(false); // No rows found

//            // Act
//            var result = handler.GetUserByID("john");

//            // Assert
//            Assert.IsNull(result);
//        }



//    }
//using NUnit.Framework;
//using Moq;
//using System.Net;
//using Sports_Exercise_Battle.Server.HTTP;
//using Sports_Exercise_Battle.Database;
//using Sports_Exercise_Battle.Models.Users;



//namespace Sports_Exercise_Battle.Tests
//{
//    public class Tests
//    {
//        private Mock<DatabaseHandler> _dbHandlerMock;
//        private HttpServer _httpServer;
//        [SetUp]
//        public void Setup()
//        {
//            _dbHandlerMock = new Mock<DatabaseHandler>();
//            // Setup your mock behavior here if necessary

//            _httpServer = new HttpServer(IPAddress.Loopback, 10001);
//            // Register endpoints as needed, potentially with mocks as well
//        }

//        [Test]
//        public void User_Initialization_SetsPropertiesCorrectly()
//        {
//            // Arrange & Act
//            var user = new User("username", "password", "Test User", "A bio here", "image/path.jpg");

//            // Assert
//            Assert.AreEqual("username", user.Username);
//            Assert.AreEqual("password", user.Password);
//            // Continue with other properties...
//        }
//    }
//}
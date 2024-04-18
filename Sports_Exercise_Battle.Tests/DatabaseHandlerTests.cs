//using Moq;
//using Npgsql;
//using Sports_Exercise_Battle.Database;
//using Sports_Exercise_Battle.Models.Users;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Sports_Exercise_Battle.Tests
//{
//    [TestFixture]
//    public class DatabaseHandlerTests
//    {
//        private Mock<NpgsqlConnection> _connectionMock;
//        private Mock<NpgsqlCommand> _commandMock;
//        private DatabaseHandler _dbHandler;

//        [SetUp]
//        public void Setup()
//        {
//            _connectionMock = new Mock<NpgsqlConnection>();
//            _commandMock = new Mock<NpgsqlCommand>();

//            _connectionMock.Setup(conn => conn.CreateCommand()).Returns(_commandMock.Object);
//            _dbHandler = new DatabaseHandler(_connectionMock.Object);
//        }

//        [Test]
//        public void RegisterUser_AddsUserToDatabase()
//        {
//            // Arrange
//            var user = new User { Username = "test", Password = "password" };

//            _commandMock.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);

//            // Act
//            int result = _dbHandler.RegisterUser(user);

//            // Assert
//            Assert.AreEqual(1, result);
//            _commandMock.Verify(cmd => cmd.ExecuteNonQuery(), Times.Once());
//        }
//    }

//}

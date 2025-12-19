using AdminControl.BLL.Concrete;
using AdminControl.DAL;
using AdminControl.DTO;
using Moq;
using NUnit.Framework;

namespace AdminControl.BL.Tests
{
    [TestFixture]
    public class AuthManagerTests
    {
        private Mock<IUserDal> _userDalMock;
        private AuthManager _sut;

        [SetUp]
        public void SetUp()
        {
            _userDalMock = new Mock<IUserDal>(MockBehavior.Strict);
            _sut = new AuthManager(_userDalMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _userDalMock.VerifyAll();
        }

        #region Authenticate Tests

        [Test]
        public void Authenticate_ShouldReturnUser_WhenCredentialsValid()
        {
            
            var user = new UserDto 
            { 
                UserID = 1, 
                Login = "admin", 
                Email = "admin@test.com",
                RoleName = "Admin",
                IsActive = true
            };
            
           
            _userDalMock.Setup(m => m.Authenticate("admin", It.IsAny<string>())).Returns(user);

           
            var result = _sut.Authenticate("admin", "password123");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Login, Is.EqualTo("admin"));
        }

        [Test]
        public void Authenticate_ShouldThrowUnauthorizedException_WhenUserNotFound()
        {
            _userDalMock.Setup(m => m.Authenticate("wronguser", It.IsAny<string>())).Returns((UserDto?)null);

            var ex = Assert.Throws<UnauthorizedAccessException>(() => _sut.Authenticate("wronguser", "password"));
            Assert.That(ex.Message, Is.EqualTo("Невірний логін або пароль."));
        }

        [Test]
        public void Authenticate_ShouldThrowUnauthorizedException_WhenPasswordWrong()
        {
            _userDalMock.Setup(m => m.Authenticate("admin", It.IsAny<string>())).Returns((UserDto?)null);

            var ex = Assert.Throws<UnauthorizedAccessException>(() => _sut.Authenticate("admin", "wrongpassword"));
            Assert.That(ex.Message, Is.EqualTo("Невірний логін або пароль."));
        }

        #endregion

        #region GetUserById Tests

        [Test]
        public void GetUserById_ShouldReturnUser_WhenFound()
        {
            var user = new UserDto 
            { 
                UserID = 1, 
                Login = "admin", 
                Email = "admin@test.com"
            };
            _userDalMock.Setup(m => m.GetById(1)).Returns(user);

            var result = _sut.GetUserById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.UserID, Is.EqualTo(1));
        }

        [Test]
        public void GetUserById_ShouldReturnNull_WhenNotFound()
        {
            _userDalMock.Setup(m => m.GetById(999)).Returns((UserDto?)null);

            var result = _sut.GetUserById(999);

            Assert.That(result, Is.Null);
        }

        #endregion

        #region GetUserByLogin Tests

        [Test]
        public void GetUserByLogin_ShouldReturnUser_WhenFound()
        {
            var user = new UserDto 
            { 
                UserID = 1, 
                Login = "admin", 
                Email = "admin@test.com"
            };
            _userDalMock.Setup(m => m.GetByLogin("admin")).Returns(user);

            var result = _sut.GetUserByLogin("admin");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Login, Is.EqualTo("admin"));
        }

        [Test]
        public void GetUserByLogin_ShouldReturnNull_WhenNotFound()
        {
            _userDalMock.Setup(m => m.GetByLogin("unknown")).Returns((UserDto?)null);

            var result = _sut.GetUserByLogin("unknown");

            Assert.That(result, Is.Null);
        }

        #endregion

        #region GetUsers Tests

        [Test]
        public void GetUsers_ShouldReturnAllUsers()
        {
            
            var users = new List<UserDto>
            {
                new UserDto { UserID = 1, Login = "admin", Email = "admin@test.com" },
                new UserDto { UserID = 2, Login = "user", Email = "user@test.com" }
            };
            _userDalMock.Setup(m => m.GetAll()).Returns(users);

            
            var result = _sut.GetUsers();

           
            Assert.That(result, Is.SameAs(users));
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetUserByLogin_ShouldReturnNull_WhenNotFound()
        {
            _userDalMock.Setup(m => m.GetByLogin("unknown")).Returns((UserDto?)null);

            var result = _sut.GetUserByLogin("unknown");

            Assert.That(result, Is.Null);
        }

        #endregion

        // ТУТ БУВ РЕГІОН GetUsers Tests - ЙОГО ТРЕБА ВИДАЛИТИ
    }
}

using AdminControl.BLL.Concrete;
using AdminControl.DAL;
using AdminControl.DTO;
using Moq;
using NUnit.Framework;

namespace AdminControl.BL.Tests
{
    [TestFixture]
    public class RoleManagerTests
    {
        private Mock<IRoleDal> _roleDalMock;
        private RoleManager _sut;

        [SetUp]
        public void SetUp()
        {
            _roleDalMock = new Mock<IRoleDal>(MockBehavior.Strict);
            _sut = new RoleManager(_roleDalMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _roleDalMock.VerifyAll();
        }

        #region GetAllRoles Tests

        [Test]
        public void GetAllRoles_ShouldReturnRolesFromDal()
        {
            var roles = new List<RoleDto>
            {
                new RoleDto { RoleID = 1, RoleName = "Admin" },
                new RoleDto { RoleID = 2, RoleName = "User" }
            };
            _roleDalMock.Setup(m => m.GetAll()).Returns(roles);

            var result = _sut.GetAllRoles();

            Assert.That(result, Is.SameAs(roles));
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetAllRoles_ShouldReturnEmptyList_WhenNoRoles()
        {
            _roleDalMock.Setup(m => m.GetAll()).Returns(new List<RoleDto>());

            var result = _sut.GetAllRoles();

            Assert.That(result, Is.Empty);
        }

        #endregion

        #region GetRoleById Tests

        [Test]
        public void GetRoleById_ShouldReturnRole_WhenFound()
        {
            var role = new RoleDto { RoleID = 1, RoleName = "Admin" };
            _roleDalMock.Setup(m => m.GetById(1)).Returns(role);

            var result = _sut.GetRoleById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.RoleName, Is.EqualTo("Admin"));
        }

        [Test]
        public void GetRoleById_ShouldReturnNull_WhenNotFound()
        {
            _roleDalMock.Setup(m => m.GetById(999)).Returns((RoleDto?)null);

            var result = _sut.GetRoleById(999);

            Assert.That(result, Is.Null);
        }

        #endregion

        #region CreateRole Tests

        [Test]
        public void CreateRole_ShouldCallDalAndReturnCreatedInstance()
        {
            var input = new RoleCreateDto { RoleName = "Moderator" };
            var created = new RoleDto { RoleID = 3, RoleName = "Moderator" };
            
            _roleDalMock.Setup(m => m.NameExists(input.RoleName, null)).Returns(false);
            _roleDalMock.Setup(m => m.Create(input)).Returns(created);

            var result = _sut.CreateRole(input);

            Assert.That(result.RoleID, Is.EqualTo(3));
            Assert.That(result.RoleName, Is.EqualTo("Moderator"));
        }

        [Test]
        public void CreateRole_ShouldThrowArgumentException_WhenRoleNameIsEmpty()
        {
            var input = new RoleCreateDto { RoleName = "" };

            var ex = Assert.Throws<ArgumentException>(() => _sut.CreateRole(input));
            Assert.That(ex.Message, Is.EqualTo("Назва ролі є обов'язковою"));
        }

        [Test]
        public void CreateRole_ShouldThrowArgumentException_WhenRoleNameIsWhitespace()
        {
            var input = new RoleCreateDto { RoleName = "   " };

            var ex = Assert.Throws<ArgumentException>(() => _sut.CreateRole(input));
            Assert.That(ex.Message, Is.EqualTo("Назва ролі є обов'язковою"));
        }

        [Test]
        public void CreateRole_ShouldThrowInvalidOperationException_WhenRoleNameExists()
        {
            var input = new RoleCreateDto { RoleName = "Admin" };
            _roleDalMock.Setup(m => m.NameExists("Admin", null)).Returns(true);

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.CreateRole(input));
            Assert.That(ex.Message, Is.EqualTo("Роль з такою назвою вже існує"));
        }

        #endregion

        #region UpdateRole Tests

        [Test]
        public void UpdateRole_ShouldCallDalAndReturnUpdatedInstance()
        {
            var input = new RoleUpdateDto { RoleID = 1, RoleName = "SuperAdmin" };
            var updated = new RoleDto { RoleID = 1, RoleName = "SuperAdmin" };
            
            _roleDalMock.Setup(m => m.Exists(1)).Returns(true);
            _roleDalMock.Setup(m => m.NameExists("SuperAdmin", 1)).Returns(false);
            _roleDalMock.Setup(m => m.Update(input)).Returns(updated);

            var result = _sut.UpdateRole(input);

            Assert.That(result.RoleName, Is.EqualTo("SuperAdmin"));
        }

        [Test]
        public void UpdateRole_ShouldThrowArgumentException_WhenRoleNameIsEmpty()
        {
            var input = new RoleUpdateDto { RoleID = 1, RoleName = "" };

            var ex = Assert.Throws<ArgumentException>(() => _sut.UpdateRole(input));
            Assert.That(ex.Message, Is.EqualTo("Назва ролі є обов'язковою"));
        }

        [Test]
        public void UpdateRole_ShouldThrowKeyNotFoundException_WhenRoleNotFound()
        {
            var input = new RoleUpdateDto { RoleID = 999, RoleName = "Test" };
            _roleDalMock.Setup(m => m.Exists(999)).Returns(false);

            var ex = Assert.Throws<KeyNotFoundException>(() => _sut.UpdateRole(input));
            Assert.That(ex.Message, Is.EqualTo("Роль не знайдено"));
        }

        [Test]
        public void UpdateRole_ShouldThrowInvalidOperationException_WhenRoleNameExistsForAnotherRole()
        {
            var input = new RoleUpdateDto { RoleID = 2, RoleName = "Admin" };
            _roleDalMock.Setup(m => m.Exists(2)).Returns(true);
            _roleDalMock.Setup(m => m.NameExists("Admin", 2)).Returns(true);

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.UpdateRole(input));
            Assert.That(ex.Message, Is.EqualTo("Роль з такою назвою вже існує"));
        }

        #endregion

        #region DeleteRole Tests

        [Test]
        public void DeleteRole_ShouldReturnTrue_WhenDeleted()
        {
            _roleDalMock.Setup(m => m.Exists(1)).Returns(true);
            _roleDalMock.Setup(m => m.Delete(1)).Returns(true);

            var result = _sut.DeleteRole(1);

            Assert.That(result, Is.True);
        }

        [Test]
        public void DeleteRole_ShouldThrowKeyNotFoundException_WhenRoleNotFound()
        {
            _roleDalMock.Setup(m => m.Exists(999)).Returns(false);

            var ex = Assert.Throws<KeyNotFoundException>(() => _sut.DeleteRole(999));
            Assert.That(ex.Message, Is.EqualTo("Роль не знайдено"));
        }

        #endregion
    }
}

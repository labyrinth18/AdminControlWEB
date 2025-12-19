using AdminControl.BLL.Interfaces;
using AdminControl.DTO;
using AdminControl.WebApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AdminControl.BL.Tests.Controllers
{
    [TestFixture]
    public class RoleControllerTests
    {
        // 1. Виправлено типи моків
        private Mock<IRoleManager> _roleManagerMock; // Було IUserManager
        private Mock<IUserManager> _userManagerMock; // Додано, бо контролер його потребує
        private Mock<ILogger<RoleController>> _loggerMock;
        private Mock<ITempDataDictionary> _tempDataMock;
        private RoleController _controller;

        [SetUp]
        public void Setup()
        {
            // 2. Ініціалізація правильних типів
            _roleManagerMock = new Mock<IRoleManager>();
            _userManagerMock = new Mock<IUserManager>();
            _loggerMock = new Mock<ILogger<RoleController>>();
            _tempDataMock = new Mock<ITempDataDictionary>();

            // 3. Передача аргументів у правильному порядку згідно з конструктором RoleController
            // public RoleController(IRoleManager roleManager, IUserManager userManager, ILogger<RoleController> logger)
            _controller = new RoleController(_roleManagerMock.Object, _userManagerMock.Object, _loggerMock.Object)
            {
                TempData = _tempDataMock.Object
            };
        }

        [Test]
        public void Create_Post_ShouldRedirectToIndex_WhenSuccessful()
        {
            // Arrange
            var roleDto = new RoleCreateDto { RoleName = "Manager" };
            var createdRole = new RoleDto { RoleID = 1, RoleName = "Manager" };

            // Виправлено виклик методу на правильному моку
            _roleManagerMock.Setup(m => m.CreateRole(roleDto)).Returns(createdRole);

            // Act
            var result = _controller.Create(roleDto) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            _roleManagerMock.Verify(m => m.CreateRole(roleDto), Times.Once);
        }

        [Test]
        public void Create_Post_ShouldReturnView_WhenNameExists()
        {
            // Arrange
            var roleDto = new RoleCreateDto { RoleName = "Admin" };
            _roleManagerMock.Setup(m => m.CreateRole(roleDto))
                .Throws(new InvalidOperationException("Role exists"));

            // Act
            var result = _controller.Create(roleDto) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.AreEqual("Role exists", _controller.ModelState["RoleName"]?.Errors[0].ErrorMessage);
        }

        [Test]
        public void Edit_Post_ShouldRedirectToIndex_WhenSuccessful()
        {
            // Arrange
            var updateDto = new RoleUpdateDto { RoleID = 1, RoleName = "SuperAdmin" };
            _roleManagerMock.Setup(m => m.UpdateRole(updateDto)).Returns(new RoleDto());

            // Act
            var result = _controller.Edit(1, updateDto) as RedirectToActionResult;

            // Assert
            Assert.AreEqual("Index", result?.ActionName);
        }
    }
}
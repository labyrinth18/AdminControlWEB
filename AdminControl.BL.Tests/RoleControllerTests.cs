using AdminControl.BLL.Interfaces;
using AdminControl.DTO;
using AdminControl.WebApp.Controllers;
using AutoMapper;
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
        private Mock<IRoleManager> _roleManagerMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ILogger<RoleController>> _loggerMock;
        private Mock<ITempDataDictionary> _tempDataMock;
        private RoleController _controller;

        [SetUp]
        public void Setup()
        {
            _roleManagerMock = new Mock<IRoleManager>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<RoleController>>();
            _tempDataMock = new Mock<ITempDataDictionary>();

            _controller = new RoleController(_roleManagerMock.Object, _mapperMock.Object, _loggerMock.Object)
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
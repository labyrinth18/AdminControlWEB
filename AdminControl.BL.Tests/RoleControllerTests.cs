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

        #region Index Tests

        [Test]
        public void Index_ShouldReturnViewWithRoles()
        {
            var roles = new List<RoleDto>
            {
                new RoleDto { RoleID = 1, RoleName = "Admin" },
                new RoleDto { RoleID = 2, RoleName = "User" }
            };
            _roleManagerMock.Setup(m => m.GetAllRoles()).Returns(roles);

            var result = _controller.Index() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreSame(roles, result.Model);
        }

        #endregion

        #region Details Tests

        [Test]
        public void Details_ShouldReturnView_WhenRoleExists()
        {
            var role = new RoleDto { RoleID = 1, RoleName = "Admin" };
            _roleManagerMock.Setup(m => m.GetRoleById(1)).Returns(role);

            var result = _controller.Details(1) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreSame(role, result.Model);
        }

        [Test]
        public void Details_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            _roleManagerMock.Setup(m => m.GetRoleById(999)).Returns((RoleDto?)null);

            var result = _controller.Details(999);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        #endregion

        #region Create Tests

        [Test]
        public void Create_Get_ShouldReturnView()
        {
            var result = _controller.Create() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RoleCreateDto>(result.Model);
        }

        [Test]
        public void Create_Post_ShouldRedirectToIndex_WhenModelIsValid()
        {

            var roleDto = new RoleCreateDto { RoleName = "NewRole" };
            var createdRole = new RoleDto { RoleID = 1, RoleName = "NewRole" };

            _roleManagerMock.Setup(m => m.CreateRole(roleDto)).Returns(createdRole);


            var result = _controller.Create(roleDto) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            
            _roleManagerMock.Verify(m => m.CreateRole(roleDto), Times.Once);
        }

        [Test]
        public void Create_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {

            var roleDto = new RoleCreateDto(); 
            _controller.ModelState.AddModelError("RoleName", "Required");

         
            var result = _controller.Create(roleDto) as ViewResult;

     
            Assert.IsNotNull(result);
            Assert.AreEqual(roleDto, result.Model);
   
            _roleManagerMock.Verify(m => m.CreateRole(It.IsAny<RoleCreateDto>()), Times.Never);
        }

        [Test]
        public void Create_Post_ShouldReturnView_WhenExceptionOccurs()
        {
            var roleDto = new RoleCreateDto { RoleName = "ExistingRole" };
            _roleManagerMock.Setup(m => m.CreateRole(roleDto))
                .Throws(new InvalidOperationException("Role exists"));

            var result = _controller.Create(roleDto) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(roleDto, result.Model);
            Assert.False(_controller.ModelState.IsValid); 
        }

        #endregion

        #region Edit Tests

        [Test]
        public void Edit_Get_ShouldReturnView_WhenRoleExists()
        {
            // Arrange
            var role = new RoleDto { RoleID = 1, RoleName = "Admin" };
            _roleManagerMock.Setup(m => m.GetRoleById(1)).Returns(role);

            // Act
            var result = _controller.Edit(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as RoleUpdateDto;
            Assert.IsNotNull(model);
            Assert.AreEqual(role.RoleID, model.RoleID);
            Assert.AreEqual(role.RoleName, model.RoleName);
        }

        [Test]
        public void Edit_Post_ShouldRedirectToIndex_WhenUpdateIsSuccessful()
        {
            var updateDto = new RoleUpdateDto { RoleID = 1, RoleName = "UpdatedRole" };
            var resultDto = new RoleDto { RoleID = 1, RoleName = "UpdatedRole" };

            _roleManagerMock.Setup(m => m.UpdateRole(updateDto)).Returns(resultDto);

            var result = _controller.Edit(1, updateDto) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void Edit_Post_ShouldReturnNotFound_WhenIdsDoNotMatch()
        {
            var updateDto = new RoleUpdateDto { RoleID = 2, RoleName = "Role" };

            var result = _controller.Edit(1, updateDto); 

      
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_Get_ShouldReturnView_WhenRoleExists()
        {
      
            var role = new RoleDto { RoleID = 1, RoleName = "ToDelete" };
            _roleManagerMock.Setup(m => m.GetRoleById(1)).Returns(role);

            var result = _controller.Delete(1) as ViewResult;

     
            Assert.IsNotNull(result);
            Assert.AreSame(role, result.Model);
        }

        [Test]
        public void DeleteConfirmed_ShouldRedirectToIndex_WhenDeleteIsSuccessful()
        {
           
            _roleManagerMock.Setup(m => m.DeleteRole(1)).Returns(true);

     
            var result = _controller.DeleteConfirmed(1) as RedirectToActionResult;

           
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [Test]
        public void DeleteConfirmed_ShouldReturnNotFound_WhenKeyNotFoundException()
        {
         
            _roleManagerMock.Setup(m => m.DeleteRole(999)).Throws(new KeyNotFoundException());

         
            var result = _controller.DeleteConfirmed(999);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        #endregion
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolskaPaliwo.Controllers;
using PolskaPaliwo.Models;
using PolskaPaliwo.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public class CRUD_Tests
    {
        private readonly HttpClient _client;

        public CRUD_Tests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }


        [Fact]
        public void CreateCarAdTest()
        {
            // Arrange
            var carAdRepositoryMock = new Mock<ICarAdRepository>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var fileContent = "Test file content";
            var fileName = "testFile.txt";
            var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)), 0, fileContent.Length, "Data", fileName);
            file.Headers = new HeaderDictionary();
            file.ContentType = "text/plain";


            var expectedUserId = "testUserId";

            var carAd = new CarAd
            {
                Brand = "Toyota"
            };

            userManagerMock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                           .Returns(expectedUserId); // Return the expected user ID

            var controller = new CarAdController(carAdRepositoryMock.Object, userManagerMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUserId),
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Create(carAd, file);
            var redirectToAction = (RedirectToActionResult)result;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", redirectToAction.ActionName);
            Assert.Equal("Home", redirectToAction.ControllerName);
        }

        [Fact]
        public void CreateCarAd_FromRepo()
        {
            // Arrange
            var carAdRepositoryMock = new Mock<ICarAdRepository>();
            var carAd = new CarAd { Brand = "Toyota" };

            // Act
            carAdRepositoryMock.Setup(x => x.CreateCarAd(carAd));

            carAdRepositoryMock.Object.CreateCarAd(carAd);

            // Assert
            carAdRepositoryMock.Verify(x => x.CreateCarAd(carAd), Times.Once);
        }
    }
}

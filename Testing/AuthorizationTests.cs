using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using Moq;
using PolskaPaliwo.Controllers;
using PolskaPaliwo.Models;
using PolskaPaliwo.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public class AuthorizationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthorizationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Disable auto-redirect to capture the redirect response
            });
        }

        //Accessing CreateForm with athenticated and unathenticated user
        [Fact]
        public void AuthenticatedUser_CanAccessCreateForm()
        {
            // Arrange
            var carAdRepositoryMock = new Mock<ICarAdRepository>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var controller = new CarAdController(carAdRepositoryMock.Object, userManagerMock.Object);

            var httpContext = new DefaultHttpContext(); //context represents an HTTP request and response.
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] //authenticated user with claim
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId"),
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.CreateToGenerateForm() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CreateFormView", result.ViewName);
        }


        [Fact]
        public async Task UnauthenticatedUser_CannotAccessCreateForm()
        {
            // Arrange
            // Not needed - any request sent by _client is unauthentication on default

            // Act
            var response = await _client.GetAsync("/CarAd/CreateToGenerateForm");
            var expectedRedirectUrl = "/Identity/Account/Login";
            var redirectedUrl = response.Headers.Location?.OriginalString;

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode); //302 - redirect code
            Assert.Contains(expectedRedirectUrl, redirectedUrl); //asks for login
        }





        [Fact]
        public void AuthenticatedUser_CanAccessUserListings()
        {
            // Arrange
            var carAdRepositoryMock = new Mock<ICarAdRepository>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var expectedUserId = "testUserId";

            var carAds = new List<CarAd>
            {
                new CarAd { CreatorId = expectedUserId },
            };
            carAdRepositoryMock.Setup(m => m.GetAllCarAds()).Returns(carAds);
            
            userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
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
            var result = controller.UserListings() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UserListingsView", result.ViewName);
        }


        [Fact]
        public async Task UnauthenticatedUser_CannotAccessUserListings()
        {
            // Arrange
            // Not needed - any request sent by _client is unauthentication on default

            // Act
            var response = await _client.GetAsync("/CarAd/UserListings");
            var expectedRedirectUrl = "/Identity/Account/Login";
            var redirectedUrl = response.Headers.Location?.OriginalString;

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode); //302 - redirect code
            Assert.Contains(expectedRedirectUrl, redirectedUrl); //asks for login
        }



        [Fact]
        public async Task UnauthenticatedUser_CannotAccessPrivacy()
        {
            // Arrange
            // Not needed - any request sent by _client is unauthentication on default

            // Act
            var response = await _client.GetAsync("/Home/Privacy");
            var expectedRedirectUrl = "/Identity/Account/Login";
            var redirectedUrl = response.Headers.Location?.OriginalString;

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode); //302 - redirect code
            Assert.Contains(expectedRedirectUrl, redirectedUrl); //asks for login
        }

    }
}

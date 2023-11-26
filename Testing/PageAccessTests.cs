using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public class PageAccessTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PageAccessTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Disable auto-redirect to capture the redirect response
            });
        }

        [Fact]
        public async Task CarAd_Index_ReturnsOK()
        {
            // Act
            var response = await _client.GetAsync("/CarAd/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        

        [Fact]
        public async Task CarAd_CreateToGenerateForm_ReturnsRedirect()
        {
            // Act
            var response = await _client.GetAsync("/CarAd/CreateToGenerateForm");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }


        [Fact]
        public async Task CarAd_UpdateToGenerateForm_ReturnsOK()
        {
            // Act
            var response = await _client.GetAsync("/CarAd/UpdateToGenerateForm");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }



        [Fact]
        public async Task CarAd_DeleteToGenerateForm_ReturnsOK()
        {
            // Act
            var response = await _client.GetAsync("/CarAd/DeleteToGenerateForm");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
    }
}

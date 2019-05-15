
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace GlobalExceptionHandling.Tests.Integration
{
    public class ValueSpec
    {
        private string WebAppName => "GlobalExceptionHandling.WebApp";
        private readonly HttpClient httpClient;
        private string testLibPath;
        private string Url => "api/Values";

        public ValueSpec()
        {
            var pathRoot = this.GetAppBasePath(this.WebAppName);
            var builder = new WebHostBuilder().UseContentRoot(pathRoot).UseEnvironment("Development").UseStartup(this.WebAppName);
            var testServer = new TestServer(builder);
            this.httpClient = testServer.CreateClient();
        }

        [Fact]
        public async Task GetAsync()
        {
            var responseMessage = await this.httpClient.GetAsync(this.Url);
            var content = await responseMessage.Content.ReadAsStringAsync();
            responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be("Ok");
        }

        [Fact]
        public async Task GetExceptionAsync()
        {
            var responseMessage = await this.httpClient.GetAsync($"{this.Url}/Exception");

            var content = await responseMessage.Content.ReadAsStringAsync();
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(content);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            responseMessage.Content.Headers.ContentType.ToString().Should().Be("application/problem+json");

            problemDetails.Status.Should().Be(500);
            problemDetails.Title.Should().Be("Throw Exception");
            problemDetails.Instance.Should().Be("/api/Values/Exception");
            problemDetails.Detail.Should().NotBeNullOrEmpty();
        }
        
        private string GetAppBasePath(string applicationWebSiteName)
        {
            var binPath = Environment.CurrentDirectory;
            while (!string.Equals(Path.GetFileName(binPath), "bin", StringComparison.InvariantCultureIgnoreCase))
            {
                binPath = Path.GetFullPath(Path.Combine(binPath, ".."));
                if (string.Equals(Path.GetPathRoot(binPath), binPath, StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("Could not find bin directory for test library.");
            }

            this.testLibPath = Path.GetFullPath(Path.Combine(binPath, ".."));
            var testPath = Path.GetFullPath(Path.Combine(testLibPath, ".."));
            var srcPath = Path.GetFullPath(Path.Combine(testPath, "..", "src"));

            if (!Directory.Exists(srcPath))
                throw new Exception("Could not find src directory.");

            var appBasePath = Path.Combine(srcPath, applicationWebSiteName);
            if (!Directory.Exists(appBasePath))
                throw new Exception("Could not find directory for application.");

            return appBasePath;
        }
    }
}

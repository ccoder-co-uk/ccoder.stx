using Core.DMS.Api;
using Core.DMS.Api.Authentication;
using Core.DMS.E2E.Tests.FakeAuthentication;
using Core.DMS.Objects.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Core.DMS.E2E.Tests.API_Tests
{
    public class BaseAPITest : BaseDatabaseTest
    {
        public HttpClient ApiClient { get; set; }
        public HttpClient SSOClient { get; set; }
        public string SSOUserId { get; set; } = "test.user";

        public void SetupAPI()
        {
            SetupDatabase();

            //Commit the default generated app in the transaction
            Transaction.Commit();
            Transaction = null;

            var factory = new CustomWebApplicationFactory<Program>(SSOClient, Configuration.GetConnectionString("TestServer")
                .Replace("Initial Catalog=master", $"Initial Catalog={DatabaseName}"));

            var authInfo = factory.Services.GetService<CoreAuthInfo>().Name = "test.user";

            ApiClient = factory.CreateClient();


            ApiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", "testtoken");
        }

        public class CustomWebApplicationFactory<TProgram>(HttpClient ssoClient, string coreConnectionString) : WebApplicationFactory<TProgram> where TProgram : class
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                base.ConfigureWebHost(builder);
                builder.UseSetting("ConnectionStrings:Core", coreConnectionString);

                builder.ConfigureTestServices((IServiceCollection services) =>
                {
                    services.AddSingleton(new CoreAuthInfo() { Name = "Guest", AuthenticationType = "bearer", IsAuthenticated = true });

                    services.AddAuthentication("bearer")
                        .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>("test", opts => { });

                    services.AddTransient<IAuthenticationSchemeProvider, FakeAuthenticationSchemeProvider>();
                });
            }
        }

        public void CleanupAPI()
        {
            CleanupDatabase();
        }
    }
}

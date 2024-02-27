using Core.DMS.E2E.Tests.FakeSSO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Core.DMS.E2E.Tests
{
    public class FakeSSOProgram
    {
        public static Task SetupFakeSSO()
        {
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());

            builder.Configuration.Sources.Add(new JsonConfigurationSource { Path = System.IO.Directory.GetCurrentDirectory() + "/appsettings.json" });

            builder.Services.AddSingleton<AuthInfo>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.MapPost("/Core/User", ([FromServices] AuthInfo authInfo) => authInfo.SSOUserId = authInfo.SSOUserId);
            app.MapGet("/Core/User/Me()", ([FromServices] AuthInfo authInfo) => new CoreUser { Id = authInfo.SSOUserId });

            return app.RunAsync();
        }
    }
}

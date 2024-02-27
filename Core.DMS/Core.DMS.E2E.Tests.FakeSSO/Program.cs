using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Core.DMS.E2E.Tests.FakeSSO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<AuthInfo>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.MapPost("/Core/User", ([FromServices] AuthInfo authInfo) => authInfo.SSOUserId = authInfo.SSOUserId);
            app.MapGet("/Core/User/Me()", ([FromServices] AuthInfo authInfo) => new CoreUser { Id = authInfo.SSOUserId });

            app.Run();
        }
    }
}

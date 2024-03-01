using Core.DMS.Api.Authentication;
using Core.DMS.Data.Brokers;
using Core.DMS.Objects.DTOs;
using Core.DMS.Services.Foundations;
using Core.DMS.Services.Orchestration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Data;
using System.IO;

namespace Core.DMS.Api;
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCors();

        builder.Services.AddAuthentication("bearer")
            .AddScheme<AuthenticationSchemeOptions, SSOAuthenticationHandler>("bearer", opts => { });

        builder.Services.AddAuthorization();

        builder.Services.AddScoped<IAuthenticationBroker, AuthenticationBroker>();
        builder.Services.AddScoped<IFolderBroker, FolderBroker>();
        builder.Services.AddScoped<IFileBroker, FileBroker>();
        builder.Services.AddScoped<IFileContentBroker, FileContentBroker>();
        builder.Services.AddScoped<ILoggingBroker, LoggingBroker>();
        builder.Services.AddScoped<IAppBroker, AppBroker>();

        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IFileContentService, FileContentService>();
        builder.Services.AddScoped<IFolderService, FolderService>();
        builder.Services.AddScoped<IAppService, AppService>();

        builder.Services.AddScoped<IDMSResultOrchestrationService, DMSResultOrchestrationService>();

        builder.Services.AddScoped<IDbConnection>((provider) => new SqlConnection(builder.Configuration.GetConnectionString("Core")));
        builder.Services.AddScoped((provider) =>
            new AppView
            {
                Domain = provider.GetService<IHttpContextAccessor>().HttpContext.Request.Host.Host
            });
        builder.Services.AddScoped((provider) => provider.GetService<IHttpContextAccessor>().HttpContext.User);

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/Api/Folder/GetFoldersForParentId", ([FromServices] IFolderService folderService,
            [FromServices] IAuthenticationBroker authenticationBroker,
            [FromServices] IAppService appService,
            Guid? parentId) 
            => folderService.GetFoldersForParentId(appService.GetAppId(), authenticationBroker.GetUserId(), parentId))
        .WithOpenApi()
        .RequireAuthorization();

        app.MapGet("/Api/File/GetFilesInFolder", ([FromServices] IFileService fileService,
            [FromServices] IAuthenticationBroker authenticationBroker,
            [FromServices] IAppService appService,
            Guid folderId,
            int skip = 0,
            int take = 1000)
            => fileService.GetFilesInFolder(appService.GetAppId(), authenticationBroker.GetUserId(), folderId, skip, take))
        .WithOpenApi()
        .RequireAuthorization();

        app.MapGet("/DMS/{*path}", async ([FromServices] IDMSResultOrchestrationService godClass, [FromServices] IHttpContextAccessor accessor, string path, int version = 0)
            =>
        {
            var dmsResult = await godClass.Get(new Core.DMS.Objects.Path(path));
            return TypedResults.Stream(dmsResult.Data, dmsResult.MimeType);
        })
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/DMS/{*path}", async ([FromServices] IDMSResultOrchestrationService godClass, [FromServices] IHttpContextAccessor accessor, string moveTo = "", string path = "") =>
        {
            using var memoryStream = new MemoryStream();
            await accessor.HttpContext.Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            if (!string.IsNullOrEmpty(moveTo))
                await godClass.Move(new Core.DMS.Objects.Path(path), new Core.DMS.Objects.Path(moveTo));
            else
                await godClass.Save(new Core.DMS.Objects.Path(path), memoryStream);

            return TypedResults.NoContent();
        })
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPut("/DMS/{*path}", async ([FromServices] IDMSResultOrchestrationService godClass, [FromServices] IHttpContextAccessor accesor, string moveTo = "", string path = "") =>
        {
            using var memoryStream = new MemoryStream();
            await accesor.HttpContext.Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await godClass.Save(new Core.DMS.Objects.Path(path), memoryStream);

            return TypedResults.NoContent();
        })
        .WithOpenApi()
        .RequireAuthorization();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.Run();
    }
}
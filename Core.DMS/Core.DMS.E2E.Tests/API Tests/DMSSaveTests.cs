using Core.DMS.Objects.Entities;
using Dapper;
using FluentAssertions;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Core.DMS.E2E.Tests.API_Tests
{
    public class DMSSaveTests : BaseAPITest
    {
        #region Guest Tests

        [Fact]
        public async Task ShouldThrowAccessDeniedWhenSavingFileWhenUserHasNoRoles()
        {
            //given
            SetupAPI();

            //when
            var response = await ApiClient.PostAsync("DMS/test/foo/test.txt", new StringContent("test", Encoding.UTF8, "text/plain"));

            var actualFolders = Connection.Query<Folder>("SELECT * FROM [DMS].[Folders]");
            var actualFiles = Connection.Query<File>("SELECT * FROM [DMS].[Files]");

            //then
            CleanupAPI();

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.Unauthorized);

            Assert.True(actualFolders.Count() == 0);
            Assert.True(actualFiles.Count() == 0);
        }

#endregion

        #region App Admin Tests

        [Fact]
        public async Task ShouldSaveFileCorrectlyAssumingUserIsAppAdmin()
        {
            //given
            SetupAPI();

            var expectedFile = new File()
            {
                CreatedBy = UserId,
                Description = null,
                MimeType = "text/plain",
                Size = "4 B",
                Name = "test.txt",
                Path = "test/foo/test.txt",
            };

            var expectedFiles = new []
            {
                expectedFile
            };

            Connection.Execute($@"
DECLARE @RoleId as uniqueidentifier = NEWID();

INSERT INTO [Security].[Roles] ([Id], [Name], [Description], [Privs], [AppId])
VALUES (@RoleId, 'App Admin', 'App Administrator Role', 'app_admin', {AppId});

INSERT INTO [Security].[UserRoles] ([UserId], [RoleId]) VALUES ('{UserId}', @RoleId);");

            //when
            var response = await ApiClient.PostAsync("DMS/test/foo/test.txt", new StringContent("test", Encoding.UTF8, "text/plain"));

            var actualFolders = Connection.Query<Folder>("SELECT * FROM [DMS].[Folders]");
            var actualFiles = Connection.Query<File>("SELECT * FROM [DMS].[Files]");

            //then
            CleanupAPI();

            response.EnsureSuccessStatusCode();

            Assert.True(actualFolders.Count() == 2
                && actualFolders.Any(f => f.Path == "test")
                && actualFolders.Any(f => f.Path == "test/foo"));

            Assert.True(actualFiles.Count() == 1);

            Assert.True(actualFiles.All(f => f.Id != Guid.Empty));

            expectedFile.FolderId = actualFolders.First(f => f.Path == "test/foo" && f.AppId == AppId).Id;

            actualFiles.Should().BeEquivalentTo(expectedFiles, opts => opts.Excluding(f => f.Id)
                .Excluding(f => f.CreatedOn));
        }

        #endregion

        #region Regular User Tests

        [Fact]
        public async Task ShouldSaveFileCorrectlyAssumingUserHasFileCreateInFolder()
        {
            //given
            SetupAPI();

            var expectedFile = new File()
            {
                CreatedBy = UserId,
                Description = null,
                MimeType = "text/plain",
                Size = "4 B",
                Name = "test.txt",
                Path = "test/test.txt",
            };

            var expectedFiles = new[]
            {
                expectedFile
            };

            Connection.Execute($@"
DECLARE @RoleId as uniqueidentifier = NEWID();

INSERT INTO [Security].[Roles] ([Id], [Name], [Description], [Privs], [AppId])
VALUES (@RoleId, 'App Admin', 'App Administrator Role', 'file_create', {AppId});

INSERT INTO [Security].[UserRoles] ([UserId], [RoleId]) VALUES ('{UserId}', @RoleId);

DECLARE @FolderId as uniqueidentifier = NEWID();

INSERT INTO [DMS].[Folders] ([Id], [AppId], [ParentId], [Name], [Path])
VALUES (@FolderId, {AppId}, NULL, 'test', 'test')

INSERT INTO [Security].[FolderRoles] (FolderId, RoleId) VALUES (@FolderId, @RoleId)
");

            //when
            var response = await ApiClient.PostAsync("DMS/test/test.txt", new StringContent("test", Encoding.UTF8, "text/plain"));

            var actualFolders = Connection.Query<Folder>("SELECT * FROM [DMS].[Folders]");
            var actualFiles = Connection.Query<File>("SELECT * FROM [DMS].[Files]");

            //then
            CleanupAPI();

            response.EnsureSuccessStatusCode();

            Assert.True(actualFolders.Count() == 1
                && actualFolders.Any(f => f.Path == "test"));

            Assert.True(actualFiles.Count() == 1);

            Assert.True(actualFiles.All(f => f.Id != Guid.Empty));

            expectedFile.FolderId = actualFolders.First(f => f.Path == "test" && f.AppId == AppId).Id;

            actualFiles.Should().BeEquivalentTo(expectedFiles, opts => opts.Excluding(f => f.Id)
                .Excluding(f => f.CreatedOn));
        }

        [Fact]
        public async Task ShouldSaveFileCorrectlyAssumingUserHasFolderCreateInFolder()
        {
            //given
            SetupAPI();

            var expectedFile = new File()
            {
                CreatedBy = UserId,
                Description = null,
                MimeType = "text/plain",
                Size = "4 B",
                Name = "test.txt",
                Path = "test/test.txt",
            };

            var expectedFiles = new[]
            {
                expectedFile
            };

            Connection.Execute($@"
DECLARE @RoleId as uniqueidentifier = NEWID();

INSERT INTO [Security].[Roles] ([Id], [Name], [Description], [Privs], [AppId])
VALUES (@RoleId, 'App Admin', 'App Administrator Role', 'folder_create', {AppId});

INSERT INTO [Security].[UserRoles] ([UserId], [RoleId]) VALUES ('{UserId}', @RoleId);

DECLARE @FolderId as uniqueidentifier = NEWID();

INSERT INTO [DMS].[Folders] ([Id], [AppId], [ParentId], [Name], [Path])
VALUES (@FolderId, {AppId}, NULL, 'test', 'test')

INSERT INTO [Security].[FolderRoles] (FolderId, RoleId) VALUES (@FolderId, @RoleId)
");

            //when
            var response = await ApiClient.PostAsync("DMS/test/foo", new StringContent("", Encoding.UTF8, "text/plain"));

            var actualFolders = Connection.Query<Folder>("SELECT * FROM [DMS].[Folders]");
            var actualFiles = Connection.Query<File>("SELECT * FROM [DMS].[Files]");
            var actualFolderRoles = Connection.Query<FolderRole>("SELECT * FROM [DMS].[Folders]");


            //then
            CleanupAPI();

            response.EnsureSuccessStatusCode();

            Assert.True(actualFolders.Count() == 2
                && actualFolders.Any(f => f.Path == "test") && actualFolders.Any(f => f.Path == "test/foo"));

            Assert.True(actualFiles.Count() == 1);

            Assert.True(actualFiles.All(f => f.Id != Guid.Empty));

            expectedFile.FolderId = actualFolders.First(f => f.Path == "test" && f.AppId == AppId).Id;

            actualFiles.Should().BeEquivalentTo(expectedFiles, opts => opts.Excluding(f => f.Id)
                .Excluding(f => f.CreatedOn));
        }

        #endregion
    }
}

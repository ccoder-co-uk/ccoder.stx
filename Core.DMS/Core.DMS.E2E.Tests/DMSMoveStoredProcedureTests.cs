using Core.DMS.Objects.Entities;
using Dapper;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Core.DMS.E2E.Tests
{
    public class DMSMoveStoredProcedureTests : BaseTest
    {
        [Fact]
        public void ShouldMoveFolderWithFileIntoFolderWithoutFile_Then_CreateFile()
        {
            //given
            Setup();

            Connection.Execute(
                sql: $@"
EXECUTE [DMS].[BuildFolderPath] 
   {AppId}
  ,'test/foo/bar';

EXECUTE [DMS].[BuildFolderPath] 
   {AppId}
  ,'test/bar';

EXECUTE [DMS].[CreateFile] 
   '{UserId}'
  ,'test.txt'
  ,'test/bar'
  ,{AppId}
  ,'text/plain'
  ,'0 B'
  ,0x2;
", transaction: Transaction);

            var expectedFile = new File
            {
                CreatedBy = UserId,
                Description = null,
                MimeType = "text/plain",
                Name = "test.txt",
                Path = "test/foo/bar/test.txt",
                Size = "0 B"
            };

            var expectedFiles = new[] { expectedFile };

            //when
            Connection.Execute(sql: $@"
EXECUTE [DMS].[MoveFolderToFolder] 
   '{UserId}'
  ,{AppId}
  ,'test/bar'
  ,'test/foo';",
  transaction: Transaction);

            var actualFolders = Connection.Query<Folder>($"SELECT * FROM [DMS].[Folders]", transaction: Transaction);
            var actualFiles = Connection.Query<File>("SELECT * FROM [DMS].[Files]", transaction: Transaction);
            var actualFileContents = Connection.Query<FileContent>("SELECT * FROM [DMS].[FileContents]", transaction: Transaction);

            //then
            Cleanup();

            Assert.True(actualFolders.Count() == 3
                && actualFolders.Any(f => f.Path == "test")
                && actualFolders.Any(f => f.Path == "test/foo")
                && actualFolders.Any(f => f.Path == "test/foo/bar"));

            //Files should've been merged into one.
            var correctFolder = actualFolders.First(f => f.Path == "test/foo/bar");
            expectedFile.FolderId = correctFolder.Id;

            Assert.True(actualFiles.Count() == 1);

            actualFiles.Should().BeEquivalentTo(expectedFiles, opts => opts.Excluding(e => e.CreatedOn).Excluding(e => e.Id));

            Assert.True(actualFiles.First().FolderId == correctFolder.Id);
            Assert.True(actualFiles.First().FolderId == correctFolder.Id);

            var contentFileIds = actualFileContents.Select(c => c.FileId).Distinct();

            Assert.True(contentFileIds.Count() == 1);

            //Verify contents
            Assert.Contains(actualFileContents, c => c.Version == 1 && c.RawData[0] == 0x02);
        }

        [Fact]
        public void ShouldMoveFolderWithFileIntoFolderWithSameFileName_Then_MergeResults()
        {
            //given
            Setup();

            Connection.Execute(
                sql: $@"
EXECUTE [DMS].[BuildFolderPath] 
   {AppId}
  ,'test/foo/bar';

EXECUTE [DMS].[BuildFolderPath] 
   {AppId}
  ,'test/bar';

EXECUTE [DMS].[CreateFile] 
   '{UserId}'
  ,'test.txt'
  ,'test/bar'
  ,{AppId}
  ,'text/plain'
  ,'0 B'
  ,0x2;

EXECUTE [DMS].[CreateFile] 
   '{UserId}'
  ,'test.txt'
  ,'test/foo/bar'
  ,{AppId}
  ,'text/plain'
  ,'0 B'
  ,0x3;", transaction: Transaction);

            //when
            Connection.Execute(sql: $@"
EXECUTE [DMS].[MoveFolderToFolder] 
   '{UserId}'
  ,{AppId}
  ,'test/bar'
  ,'test/foo';",
  transaction: Transaction);

            var actualFolders = Connection.Query<Folder>($"SELECT * FROM [DMS].[Folders]", transaction: Transaction);
            var actualFiles = Connection.Query<File>("SELECT * FROM [DMS].[Files]", transaction: Transaction);
            var actualContents = Connection.Query<FileContent>("SELECT * FROM [DMS].[FileContents]", transaction: Transaction);

            //then
            Cleanup();

            Assert.True(actualFolders.Count() == 3 
                && actualFolders.Any(f => f.Path == "test") 
                && actualFolders.Any(f => f.Path == "test/foo")
                && actualFolders.Any(f => f.Path == "test/foo/bar"));

            //Files should've been merged into one.
            Assert.True(actualFiles.Count() == 1);

            var contentFileIds = actualContents.Select(c => c.FileId).Distinct();

            Assert.True(contentFileIds.Count() == 1);

            //Verify contents
            Assert.Contains(actualContents, c => c.Version == 1 && c.RawData[0] == 0x03);
            Assert.Contains(actualContents, c => c.Version == 2 && c.RawData[0] == 0x02);
        }
    }
}

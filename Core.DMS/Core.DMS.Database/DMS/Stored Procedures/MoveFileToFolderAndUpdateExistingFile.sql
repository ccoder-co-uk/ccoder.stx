-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [DMS].[MoveFileToFolderAndUpdateExistingFile]
	-- Add the parameters for the stored procedure here
	@UserId as nvarchar(100),
    @AppId as int,
	@OldPath as nvarchar(max),
	@NewPath as nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldFileId as UNIQUEIDENTIFIER;
    DECLARE @NewFileId as UNIQUEIDENTIFIER;

    SET @OldFileId = (
        SELECT TOP 1 dmsFile.Id
        FROM [DMS].[Files] dmsFile
        INNER JOIN [DMS].[Folders] dmsFolder ON dmsFolder.Id=dmsFile.FolderId
        WHERE dmsFolder.[AppId]=@AppId AND dmsFile.Path=@OldPath
    );

    SET @NewFileId = (
        SELECT TOP 1 dmsFile.Id
        FROM [DMS].[Files] dmsFile
        INNER JOIN [DMS].[Folders] dmsFolder ON dmsFolder.Id=dmsFile.FolderId
        WHERE dmsFolder.[AppId]=@AppId AND dmsFile.Path=@NewPath
    );

	INSERT INTO [DMS].[FileContents] (
		[Id],
		[FileId],
		[CreatedBy],
		[CreatedOn],
		[Version],
		[Size],
		[RawData]
	)
    SELECT TOP 1 NEWID() as [Id], @NewFileId as [FileId], @UserId as [CreatedBy], GETDATE() as [CreatedOn], ([Version] + 1) as [Version], [Size], [RawData]
    FROM [DMS].[FileContents]
    WHERE FileId=@OldFileId
    ORDER BY [Version] DESC
    
    DELETE FROM [DMS].[FileContents] WHERE FileId=@OldFileId
    DELETE FROM [DMS].[Files] WHERE Id=@OldFileId
END
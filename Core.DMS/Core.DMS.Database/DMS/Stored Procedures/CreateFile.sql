-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [DMS].[CreateFile]
	-- Add the parameters for the stored procedure here
	@UserId as nvarchar(100),
	@Name as nvarchar(max),
	@FolderPath as nvarchar(max),
	@AppId as int,
	@MimeType as nvarchar(200),
	@Size as nvarchar(100),
	@RawData as varbinary(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @FileId as uniqueidentifier = NEWID();

	DECLARE @FolderId as uniqueidentifier = (SELECT TOP 1 Id FROM [DMS].[Folders] f WHERE f.AppId=@AppId AND f.[Path]=@FolderPath)

	INSERT INTO [DMS].[Files] (
		Id,
		[CreatedBy],
		[CreatedOn],
		[Name],
		[Path],
		[FolderId],
		[MimeType],
		[Size]
	) VALUES (
		@FileId,
		@UserId,
		GETDATE(),
		@Name,
		CONCAT(@FolderPath, '/', @Name),
		@FolderId,
		@MimeType,
		@Size
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
	VALUES (
		NEWID(),
		@FileId,
		@UserId,
		GETDATE(),
		1,
		@Size,
		@RawData
	)
END
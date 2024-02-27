
CREATE PROCEDURE [DMS].[CreateFileVersion]
	@UserId as nvarchar(100),
	@FileId as uniqueidentifier,
	@Size as nvarchar(100),
	@RawData as varbinary(max)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Version as int = (
		SELECT TOP 1 [Version] 
		FROM [DMS].[FileContents] fc 
		WHERE fc.FileId=@FileId
		ORDER BY [fc].[Version] DESC
	) + 1;

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
		@Version,
		@Size,
		@RawData
	)
END
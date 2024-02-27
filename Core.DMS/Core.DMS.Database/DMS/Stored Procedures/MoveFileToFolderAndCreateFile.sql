-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [DMS].[MoveFileToFolderAndCreateFile]
	-- Add the parameters for the stored procedure here
	@UserId as nvarchar(100),
    @AppId as nvarchar(100),
	@OldPath as nvarchar(max),
	@NewPath as nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
    UPDATE [DMS].[Files]
    SET [Path]=@NewPath
    FROM [DMS].[Files] dmsFile
    INNER JOIN [DMS].[Folders] dmsFolder ON dmsFolder.Id=dmsFile.FolderId
    WHERE dmsFile.[Path]=@OldPath AND dmsFolder.AppId=@AppId
END
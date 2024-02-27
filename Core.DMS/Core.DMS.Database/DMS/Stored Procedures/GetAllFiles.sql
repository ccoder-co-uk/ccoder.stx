
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [DMS].[GetAllFiles]
	@UserId as nvarchar(100),
	@Domain as nvarchar(100),
	@StartingPath as nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    DECLARE @AppId as int = (SELECT TOP 1 Id FROM [CMS].[Apps] WHERE Domain=@Domain);

	SELECT dmsFile.Id, dmsFile.[FolderId], dmsFile.[Name], dmsFile.[Description], dmsFile.[Path], dmsFile.[MimeType], dmsFile.[CreatedBy], dmsFile.[Size], dmsFile.[CreatedOn]
	FROM [DMS].[Files] dmsFile
	INNER JOIN [DMS].[Folders] dmsFolder ON dmsFolder.Id=dmsFile.FolderId
	WHERE dmsFolder.AppId=@AppId AND dmsFolder.[Path] LIKE CONCAT(@StartingPath, '%') AND (EXISTS (
		SELECT TOP 1 * FROM [Security].[FolderRoles] fr 
		INNER JOIN [Security].[Roles] r ON r.Id=fr.RoleId AND (r.Privs LIKE '%file_read%' OR r.Privs LIKE '%app_admin%')
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
		WHERE fr.FolderId=dmsFolder.Id
	) OR EXISTS (
        SELECT * FROM [Security].[Roles] r
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
		WHERE r.AppId=@AppId AND r.Privs LIKE '%app_admin%'
    ))
	ORDER BY dmsFolder.[Path]
	END
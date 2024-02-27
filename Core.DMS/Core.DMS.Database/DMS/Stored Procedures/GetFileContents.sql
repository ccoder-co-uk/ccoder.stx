
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [DMS].[GetFileContents]
	@UserId as nvarchar(100),
	@Domain as nvarchar(100),
	@Path as nvarchar(max),
	@Version as int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP 1 fc.RawData, dmsFile.MimeType, dmsFile.[Path]
	FROM [DMS].[FileContents] fc
	INNER JOIN [DMS].[Files] dmsFile ON fc.FileId=dmsFile.Id
	INNER JOIN [DMS].[Folders] dmsFolder ON dmsFolder.Id=dmsFile.FolderId
	INNER JOIN [CMS].[Apps] a ON a.Id = dmsFolder.AppId
	WHERE (@Version = 0 OR (@Version != 0 AND fc.[Version]=@Version)) AND a.Domain=@Domain AND dmsFolder.AppId=a.Id AND dmsFile.[Path]=@Path AND (EXISTS (
		SELECT TOP 1 * FROM [Security].[FolderRoles] fr 
		INNER JOIN [Security].[Roles] r ON r.Id=fr.RoleId AND (r.Privs LIKE '%file_read%' OR r.Privs LIKE '%app_admin%')
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
		WHERE fr.FolderId=dmsFolder.Id
	) OR EXISTS (
		SELECT * FROM [Security].[Roles] r
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
		WHERE r.AppId=a.Id AND r.Privs LIKE '%app_admin%'
    ))
	ORDER BY fc.[Version] DESC
END

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [DMS].[GetFolder]
	@UserId as nvarchar(100),
	@AppId as int,
	@Path as nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT dmsFolder.Id, dmsFolder.AppId, dmsFolder.[Name], dmsFolder.[ParentId], dmsFolder.[Path]
	FROM [DMS].[Folders] dmsFolder
	WHERE dmsFolder.AppId=@AppId AND dmsFolder.[Path]=@Path AND (EXISTS (
		SELECT TOP 1 * FROM [Security].[FolderRoles] fr 
		INNER JOIN [Security].[Roles] r ON r.Id=fr.RoleId AND (r.Privs LIKE '%folder_read%' OR r.Privs LIKE '%app_admin%')
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
		WHERE fr.FolderId=dmsFolder.Id
	) OR EXISTS (
        SELECT * FROM [Security].[Roles] r
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
		WHERE r.AppId=@AppId AND r.Privs LIKE '%app_admin%'
    ))
	END
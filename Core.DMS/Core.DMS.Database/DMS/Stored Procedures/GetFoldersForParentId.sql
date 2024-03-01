CREATE PROCEDURE [dbo].[GetFoldersForParentId]
	@AppId as int,
	@ParentId as uniqueidentifier,
	@UserId as nvarchar(200)
AS
BEGIN
	DECLARE @IsAppAdmin as tinyint = CASE WHEN EXISTS(
		SELECT TOP 1 r.Id FROM [Security].[Roles] r
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
		WHERE (r.[Privs] LIKE '%app_admin%') AND r.AppId=@AppId
	) THEN 1
	ELSE 0
	END

	SELECT f.[Id], f.[Name], f.[Path]
	FROM [DMS].[Folders] f
	WHERE f.AppId = @AppId AND ((@ParentId IS NULL AND f.ParentId IS NULL) OR f.ParentId=@ParentId) AND ((@IsAppAdmin = 1) OR EXISTS (
		SELECT TOP 1 r.Id FROM [Security].[Roles] r
		INNER JOIN [Security].[FolderRoles] fr ON fr.[FolderId]=f.[Id] AND fr.RoleId=r.Id
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=fr.RoleId
		WHERE (r.[Privs] LIKE '%folder_read%' OR r.[Privs] LIKE '%app_admin%') AND r.AppId=@AppId
	))
	ORDER BY f.[Name] ASC
END

--Assuming folder exists at path
CREATE FUNCTION [DMS].[CanCreateFile]
(
	@UserId as nvarchar(100),
	@AppId as int,
	@FolderPath as nvarchar(max)
)
RETURNS tinyint
AS
BEGIN
	DECLARE @FolderId as uniqueidentifier = (SELECT TOP 1 [Id] FROM [DMS].[Folders] f WHERE f.[Path] = @FolderPath AND f.AppId=@AppId)

	DECLARE @CanCreate as tinyint = (
		SELECT TOP 1 CASE 
			WHEN EXISTS (
				SELECT TOP 1 r.Id FROM [Security].[Roles] r
				INNER JOIN [Security].[FolderRoles] fr ON fr.[FolderId]=@FolderId AND fr.RoleId=r.Id
				INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=fr.RoleId
				WHERE (r.[Privs] LIKE '%file_create%' OR r.[Privs] LIKE '%app_admin%') AND r.AppId=@AppId
			) OR EXISTS (				
				SELECT TOP 1 r.Id FROM [Security].[Roles] r
				INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
				WHERE (r.[Privs] LIKE '%app_admin%') AND r.AppId=@AppId
			)
			THEN 1
			ELSE 0 
		END as [CanCreate]
	)

	RETURN @CanCreate
END
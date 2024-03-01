﻿CREATE PROCEDURE [dbo].[GetFileCountInFolder]
	@AppId as int,
	@FolderId as uniqueidentifier,
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

	SELECT COUNT(fi.[Id])
	FROM [DMS].[Files] fi
	INNER JOIN [DMS].[Folders] f ON f.[Id]=fi.[FolderId]
	WHERE f.AppId = @AppId AND ((@FolderId IS NULL AND f.ParentId IS NULL) OR f.Id=@FolderId) AND ((@IsAppAdmin = 1) OR EXISTS (
		SELECT TOP 1 r.Id FROM [Security].[Roles] r
		INNER JOIN [Security].[FolderRoles] fr ON fr.[FolderId]=f.[Id] AND fr.RoleId=r.Id
		INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=fr.RoleId
		WHERE ((r.[Privs] LIKE '%file_read%' AND r.[Privs] LIKE '%folder_read%') OR r.[Privs] LIKE '%app_admin%') AND r.AppId=@AppId
	))
END

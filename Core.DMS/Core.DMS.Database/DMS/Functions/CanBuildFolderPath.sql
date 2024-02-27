-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [DMS].[CanBuildFolderPath]
(
	@UserId as nvarchar(100),
	@AppId as int,
	@FolderPath as nvarchar(max)
)
RETURNS tinyint
AS
BEGIN
	DECLARE @ExistingFolders AS TABLE([Path] nvarchar(max), [FolderId] uniqueidentifier);

	INSERT INTO @ExistingFolders
	SELECT f.[Path], f.[Id]
	FROM [DMS].[GetPathSegments](@FolderPath) p
	INNER JOIN [DMS].[Folders] f ON f.[Path]=p.[Path] AND f.AppId=@AppId

	DECLARE @RelevantPath as uniqueidentifier = (SELECT TOP 1 FolderId FROM @ExistingFolders ORDER BY [Path] DESC);

	DECLARE @CanCreate as tinyint = (
		SELECT TOP 1 CASE 
			WHEN EXISTS (
				SELECT * FROM [Security].[Roles] r
				INNER JOIN [Security].[FolderRoles] fr ON fr.[FolderId]=@RelevantPath AND r.Id=fr.RoleId
				INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=fr.RoleId
				WHERE (r.[Privs] LIKE '%folder_create%') AND r.AppId=@AppId
			) OR EXISTS (
				SELECT * FROM [Security].[Roles] r
				INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
				WHERE  r.[Privs] LIKE '%app_admin%' AND r.AppId=@AppId
			)
			THEN 1
			ELSE 0 
		END as [CanCreate]
	)

	RETURN @CanCreate
END
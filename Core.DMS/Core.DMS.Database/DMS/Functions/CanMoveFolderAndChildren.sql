
CREATE FUNCTION [DMS].[CanMoveFolderAndChildren]
(
	@UserId as nvarchar(100),
	@AppId as int,
	@FolderPath as nvarchar(max)
)
RETURNS tinyint
AS
BEGIN
    DECLARE @IsAppAdmin as tinyint = (
        CASE WHEN EXISTS (				
        SELECT TOP 1 r.Id FROM [Security].[Roles] r
        INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
        WHERE (r.[Privs] LIKE '%app_admin%') AND r.AppId=@AppId
        ) THEN 1
        ELSE 0
        END
    );

    DECLARE @CanCreate as tinyint;

    ---Thrown it in reverse as it's probably easier to catch ones that we don't have access for..

    SET @CanCreate = CASE WHEN EXISTS(
        SELECT TOP 1 *
        FROM [DMS].[Folders] dmsFolder
        WHERE dmsFolder.AppId=@AppId AND dmsFolder.[Path] LIKE CONCAT(@FolderPath, '%') AND @IsAppAdmin = 0 AND NOT EXISTS (
            SELECT * FROM [Security].[Roles] r
            INNER JOIN [Security].[FolderRoles] fr ON fr.[FolderId]=dmsFolder.[Id] AND fr.RoleId=r.Id
            INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=fr.RoleId
            WHERE (r.[Privs] LIKE '%folder_update%') AND r.AppId=@AppId
        )
    ) THEN 0
    ELSE 1
    END;

	RETURN @CanCreate
END
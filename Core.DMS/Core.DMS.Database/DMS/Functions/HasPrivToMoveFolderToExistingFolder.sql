-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [DMS].[HasPrivToMoveFolderToExistingFolder]
(
	@UserId as nvarchar(100),
	@AppId as int,
	@OldPath as nvarchar(max),
    @NewPath as nvarchar(max)
)
RETURNS tinyint
AS
BEGIN
    DECLARE @IsAppAdmin as tinyint = (
        CASE WHEN EXISTS (				
            SELECT TOP 1 * FROM [Security].[Roles] r
            INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
            WHERE (r.[Privs] LIKE '%app_admin%') AND r.AppId=@AppId
        ) THEN 1
        ELSE 0
        END
    );

    DECLARE @OldName as nvarchar(max) = (
        SELECT TOP 1 [Name] FROM [DMS].[Folders] WHERE AppId=@AppId AND [Path]=@OldPath
    );

    DECLARE @TrueNewPath as nvarchar(max) = LOWER(CONCAT(@NewPath, '/', @OldName));

    DECLARE @NewPaths as TABLE(OldSourcePath nvarchar(max), NewDestinationPath nvarchar(max));

    INSERT INTO @NewPaths
    SELECT [Path] as [OldSourcePath], TRIM('/' FROM REPLACE([Path], @OldPath, @TrueNewPath)) as [NewDestinationPath]
    FROM [DMS].[Folders]
    WHERE AppId=@AppId 
        AND [Path] LIKE CONCAT(@OldPath, '%')

    DECLARE @UpdatablePaths AS TABLE([NewPath] nvarchar(max));

    INSERT INTO @UpdatablePaths
    SELECT np.NewDestinationPath
    FROM @NewPaths np
    LEFT JOIN [DMS].[Folders] newDMSFolder ON newDMSFolder.AppId=@AppId AND newDMSFolder.[Path]=np.NewDestinationPath
    WHERE newDMSFolder.Id IS NOT NULL AND ((@IsAppAdmin = 1) OR EXISTS (
        SELECT TOP 1 * FROM [Security].[Roles] r
        INNER JOIN [Security].[FolderRoles] fr ON fr.[FolderId]=newDMSFolder.Id AND fr.RoleId=r.Id
        INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=fr.RoleId
        WHERE (r.[Privs] LIKE '%folder_update%') AND r.AppId=@AppId
    ));

    DECLARE @CreatablePaths AS TABLE([NewPath] nvarchar(max));

    INSERT INTO @CreatablePaths
    SELECT np.NewDestinationPath
    FROM @NewPaths np
    LEFT JOIN [DMS].[Folders] newDMSFolder ON newDMSFolder.AppId=@AppId AND newDMSFolder.[Path]=np.NewDestinationPath
    WHERE newDMSFolder.Id IS NULL AND ((@IsAppAdmin = 1) OR ([DMS].[CanBuildFolderPath](@UserId, @AppId, newDMSFolder.[Path]) = 1))

    DECLARE @HasPriv as tinyint =  CASE WHEN EXISTS(
        SELECT TOP 1 *
        FROM @NewPaths np 
        WHERE 
            NOT EXISTS (SELECT TOP 1 * FROM @CreatablePaths cp WHERE cp.[NewPath]=np.[NewDestinationPath]) 
            AND NOT EXISTS (SELECT TOP 1 * FROM @UpdatablePaths up WHERE up.[NewPath]=np.[NewDestinationPath])
        ) THEN 0
        ELSE 1
    END;

	RETURN @HasPriv;
END
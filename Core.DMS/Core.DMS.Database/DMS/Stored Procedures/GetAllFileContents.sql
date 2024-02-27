

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [DMS].[GetAllFileContents]
	@UserId as nvarchar(100),
    @AppId as int,
	@Domain as nvarchar(100),
	@StartingPath as nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT b.[MimeType], b.[Path], b.[RawData]
	FROM (
		SELECT dmsFile.Id, dmsFile.MimeType, dmsFile.[Path]
		FROM [DMS].[Files] dmsFile
		INNER JOIN [DMS].[Folders] dmsFolder ON dmsFolder.Id=dmsFile.FolderId AND dmsFolder.AppId=@AppId
		WHERE dmsFile.[Path] LIKE CONCAT(@StartingPath, '%') AND (EXISTS (
			SELECT TOP 1 * FROM [Security].[FolderRoles] fr 
			INNER JOIN [Security].[Roles] r ON r.Id=fr.RoleId AND (r.Privs LIKE '%file_read%' OR r.Privs LIKE '%app_admin%')
			INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
			WHERE fr.FolderId=dmsFolder.Id
		) OR EXISTS (
            SELECT * FROM [Security].[Roles] r
            INNER JOIN [Security].[UserRoles] ur ON ur.UserId=@UserId AND ur.RoleId=r.Id
            WHERE r.AppId=@AppId AND r.Privs LIKE '%app_admin%'
        ))
	) a
	CROSS APPLY (
		SELECT TOP 1 fc.RawData, a.MimeType, a.[Path]
		FROM [DMS].[FileContents] fc
		WHERE fc.FileId=a.Id
		ORDER BY fc.[Version] DESC
	) b
	ORDER BY b.[Path] ASC
END
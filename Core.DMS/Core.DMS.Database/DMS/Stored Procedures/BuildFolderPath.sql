CREATE PROCEDURE [DMS].[BuildFolderPath]
	@AppId as int,
	@FolderPath as nvarchar(max)
AS
BEGIN
	
	DECLARE @Path nvarchar(max), @Name nvarchar(max), @ParentId uniqueidentifier;
	DECLARE @FolderPaths AS TABLE([Path] nvarchar(max), [Name] nvarchar(max));

	INSERT INTO @FolderPaths
	SELECT pathSegments.[Path], pathSegments.[Name] 
    FROM [DMS].[GetPathSegments](@FolderPath) pathSegments
	WHERE NOT EXISTS (
        SELECT * FROM [DMS].[Folders] f 
        WHERE f.AppId=@AppId AND f.[Path]=pathSegments.[Path]
    )
    ORDER BY pathSegments.[Path];

	SET @ParentId = (
        SELECT TOP 1 f.Id
        FROM [DMS].[GetPathSegments](@FolderPath) pathSegments
        LEFT JOIN [DMS].[Folders] f ON f.AppId=@AppId AND f.[Path]=pathSegments.[Path]
        WHERE f.Id IS NOT NULL
        ORDER BY pathSegments.[Path] DESC
	);

	DECLARE @Id as uniqueidentifier;

	DECLARE data_cursor CURSOR FOR
	SELECT paths.[Name], paths.[Path] FROM @FolderPaths paths;

	OPEN data_cursor;
	FETCH NEXT FROM data_cursor INTO @Name, @Path;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @Id=NEWID();

		-- Insert the current row into your target table
		INSERT INTO [DMS].[Folders] (Id, AppId, [Name], [Path], [ParentId])
		VALUES (@Id, @AppId, @Name, @Path, @ParentId);
        
		INSERT INTO [Security].[FolderRoles]
		SELECT @Id as [FolderId], RoleId 
		FROM [Security].[FolderRoles] fr
		WHERE fr.FolderId=@ParentId

		-- Set the ParentId of the next row to the Id of the current row
		SET @ParentId = @Id; -- Assuming your Id column is an identity column

		FETCH NEXT FROM data_cursor INTO @Name, @Path;
	END;

	CLOSE data_cursor;
	DEALLOCATE data_cursor;

END
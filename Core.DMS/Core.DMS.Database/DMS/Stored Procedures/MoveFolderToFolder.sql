-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [DMS].[MoveFolderToFolder]
	-- Add the parameters for the stored procedure here
	@UserId as nvarchar(100),
    @AppId as int,
	@OldPath as nvarchar(max),
	@NewPath as nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
    DECLARE @OldName as nvarchar(max) = (
        SELECT TOP 1 [Name] FROM [DMS].[Folders] WHERE AppId=@AppId AND [Path]=@OldPath
    );

    DECLARE @TrueNewPath as nvarchar(max) = LOWER(CONCAT(@NewPath, '/', @OldName));

    DECLARE @OldFolders AS TABLE([OldFolderId] uniqueidentifier, [OldFolderPath] nvarchar(max), [NewFolderPath] nvarchar(max), [NewFolderId] uniqueidentifier, [CreatedId] uniqueidentifier NULL);

    INSERT INTO @OldFolders ([OldFolderId], [OldFolderPath], [NewFolderPath], [NewFolderId])
    SELECT 
        oldDMSFolder.[Id] as [OldFolderId],
        oldDMSFolder.[Path] as [OldFolderPath],
        TRIM('/' FROM REPLACE(oldDMSFolder.[Path], @OldPath, @TrueNewPath)) as [NewFolderPath],
        newDMSFolder.[Id] as [NewFolderId]
        FROM [DMS].[Folders] oldDMSFolder
        LEFT JOIN [DMS].[Folders] newDMSFolder ON newDMSFolder.Path=TRIM('/' FROM REPLACE(oldDMSFolder.[Path], @OldPath, @TrueNewPath))

        WHERE oldDMSFolder.AppId=@AppId AND oldDMSFolder.[Path] LIKE CONCAT(@OldPath, '%')
        ORDER BY oldDMSFolder.[Path] ASC;


    IF (NOT EXISTS (SELECT * FROM [DMS].[Folders] dmsFolder WHERE dmsFolder.AppId=@AppId AND dmsFolder.[Path]=@TrueNewPath))
    BEGIN

        ---Build path at destination
        EXECUTE [DMS].[BuildFolderPath] 
        @AppId
        ,@NewPath;

        DECLARE @NewParentId as UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [DMS].[Folders] WHERE AppId=@AppId AND [Path]=@NewPath);

        UPDATE [DMS].[Folders] SET ParentId=@NewParentId WHERE [Path]=@OldPath AND AppId=@AppId

        UPDATE [dmsFolder]
        SET [dmsFolder].[Path] = LOWER(TRIM('/' FROM CONCAT(parentDMSFolder.[Path], '/', dmsFolder.[Name])))
        FROM 
            [DMS].[Folders] dmsFolder
            LEFT JOIN [DMS].[Folders] parentDMSFolder ON parentDMSFolder.Id=dmsFolder.ParentId
        WHERE dmsFolder.Id IN (SELECT [OldFolderId] FROM @OldFolders)

        UPDATE [dmsFile]
        SET [dmsFile].[Path] = LOWER(TRIM('/' FROM CONCAT(parentDMSFolder.[Path], '/', dmsFile.[Name])))
        FROM 
            [DMS].[Files] dmsFile
            LEFT JOIN [DMS].[Folders] parentDMSFolder ON parentDMSFolder.Id=dmsFile.FolderId
        WHERE dmsFile.FolderId IN (SELECT [OldFolderId] FROM @OldFolders)
    END
    ELSE
    BEGIN

        /* Create folders that don't exist at the destination and move the files over, cursor is used due to sproc limitations... */
        DECLARE @OldFolderId as uniqueidentifier;
        DECLARE @OldSourcePath as nvarchar(max);
        DECLARE @NewDestinationPath as nvarchar(max);

        DECLARE @CreatedFolderId as uniqueidentifier;

        DECLARE FolderCursor CURSOR FOR
            SELECT [OldFolderId], [OldFolderPath], [NewFolderPath] FROM @OldFolders
            WHERE [NewFolderId] IS NULL
            ORDER BY [NewFolderPath] ASC;

        OPEN FolderCursor;

        FETCH NEXT FROM FolderCursor INTO @OldFolderId, @OldSourcePath, @NewDestinationPath;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXECUTE [DMS].[BuildFolderPath] 
            @AppId
            ,@NewDestinationPath;

            SET @CreatedFolderId = (SELECT TOP 1 [Id] FROM [DMS].[Folders] newFolder WHERE newFolder.Path=@NewDestinationPath);

            ---Copy roles over
            INSERT INTO [Security].[FolderRoles] (FolderId, RoleId)
            SELECT @CreatedFolderId, [RoleId] FROM [Security].[FolderRoles] WHERE FolderId=@OldFolderId

            ---Move files over to the new folder
            UPDATE [dmsFile]
            SET [dmsFile].[FolderId] = @CreatedFolderId
            FROM 
                [DMS].[Files] dmsFile
                INNER JOIN [DMS].[Folders] parentDMSFolder ON parentDMSFolder.Id=dmsFile.FolderId
            WHERE parentDMSFolder.AppId=@AppId AND parentDMSFolder.[Path]=@OldSourcePath

            --Recompute file paths
            UPDATE [dmsFile]
            SET [dmsFile].[Path] = LOWER(TRIM('/' FROM CONCAT(parentDMSFolder.[Path], '/', dmsFile.[Name])))
            FROM 
                [DMS].[Files] dmsFile
                LEFT JOIN [DMS].[Folders] parentDMSFolder ON parentDMSFolder.Id=dmsFile.FolderId
            WHERE dmsFile.FolderId=@CreatedFolderId

            FETCH NEXT FROM FolderCursor INTO @OldFolderId, @OldSourcePath, @NewDestinationPath;
        END

        CLOSE FolderCursor;
        DEALLOCATE FolderCursor;
        
        ---By this point all the folders should exist
        ---We are now going to create all the missing files on the destination

        INSERT INTO [DMS].[Files] (
            Id,
            [CreatedBy],
            [CreatedOn],
            [Name],
            [Path],
            [FolderId],
            [MimeType],
            [Size]
        )
        SELECT
            NEWID() as [Id],
            @UserId as [CreatedBy],
            GETDATE() as [CreatedOn],
            dmsFile.[Name],
            LOWER(TRIM('/' FROM CONCAT(newDMSFolder.Path, '/', dmsFile.[Name]))) as [Path],
            newDMSFolder.Id,
            dmsFile.[MimeType],
            dmsFile.[Size]
        FROM [DMS].[Files] dmsFile
        INNER JOIN [DMS].[Folders] dmsFolder ON dmsFolder.Id=dmsFile.FolderId AND dmsFolder.AppId=@AppId
        INNER JOIN @OldFolders oldFolders ON oldFolders.OldFolderId=dmsFolder.Id
        INNER JOIN [DMS].[Folders] newDMSFolder ON newDMSFolder.AppId=@AppId AND newDMSFolder.[Path]=oldFolders.NewFolderPath
        WHERE NOT EXISTS (
            SELECT TOP 1 newDMSFile.[Id]
            FROM [DMS].[Files] newDMSFile
            WHERE newDMSFile.FolderId=newDMSFolder.Id AND newDMSFile.[Path]=TRIM('/' FROM CONCAT(newDMSFolder.[Path], '/', newDMSFile.[Name]))        
        )

        ---We need to merge the two sets files

        INSERT INTO [DMS].[FileContents] (
            [Id],
            [FileId],
            [CreatedBy],
            [CreatedOn],
            [Version],
            [Size],
            [RawData]
        )
        ---Select all files that are in the old folders and then find their newest file id and version
        SELECT
            NEWID() as [Id],
            c.[Id] as [FileId],
            @UserId as [CreatedBy],
            GETDATE() as [CreatedOn],
            (COALESCE(c.[Version],0) + 1) as [Version],
            COALESCE(d.[Size], '0 B') as [Size],
            COALESCE(d.[RawData], 0x) as [RawData]
        FROM [DMS].[Files] dmsFile
        INNER JOIN [DMS].[Folders] dmsFolder ON dmsFolder.Id=dmsFile.FolderId AND dmsFolder.AppId=@AppId
        INNER JOIN @OldFolders oldFolders ON oldFolders.OldFolderId=dmsFolder.Id
        CROSS APPLY (
            SELECT TOP 1 newDMSFile.[Id], [newDMSFileContents].[Version]
            FROM [DMS].[Files] newDMSFile
            INNER JOIN [DMS].[Folders] newDMSFolder ON newDMSFolder.Id=newDMSFile.FolderId
            LEFT JOIN [DMS].[FileContents] newDMSFileContents ON newDMSFileContents.FileId=newDMSFile.Id
            WHERE newDMSFolder.AppId=@AppId AND newDMSFolder.[Path]=oldFolders.[NewFolderPath] AND newDMSFile.[Path]=TRIM('/' FROM CONCAT(oldFolders.NewFolderPath, '/', newDMSFile.[Name]))
        ) c
        CROSS APPLY (
            SELECT TOP 1 existingFileContents.[Size], existingFileContents.[RawData] 
            FROM [DMS].[Files] existingFile
            INNER JOIN [DMS].[Folders] existingFolder ON existingFolder.[Id] = existingFile.[FolderId]
            INNER JOIN [DMS].[FileContents] existingFileContents ON existingFileContents.[FileId] = existingFile.[Id] 
            WHERE existingFile.[Path]=dmsFile.[Path] AND existingFolder.[AppId]=@AppId
            ORDER BY existingFileContents.[Version] DESC
        ) d

        DELETE fc FROM [DMS].[FileContents] fc
        INNER JOIN [DMS].[Files] fi ON fi.Id=fc.FileId
        INNER JOIN [DMS].[Folders] fo ON fo.Id=fi.FolderId
        WHERE fo.Id IN (SELECT OldFolderId FROM @OldFolders)

        DELETE fi FROM [DMS].[Files] fi
        INNER JOIN [DMS].[Folders] fo ON fo.Id=fi.FolderId
        WHERE fo.Id IN (SELECT OldFolderId FROM @OldFolders)

        DELETE fo FROM [DMS].[Folders] fo
        WHERE fo.Id IN (SELECT OldFolderId FROM @OldFolders)

    END

END

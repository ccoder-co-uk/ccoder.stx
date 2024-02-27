
CREATE FUNCTION [DMS].[GetPathSegments]
(
    @FolderPath as nvarchar(max)
)
RETURNS 
@Results TABLE 
(
    [Path] nvarchar(max),
	[Name] nvarchar(max)
)
BEGIN

    DECLARE @ReturnTable as TABLE ([Path] nvarchar(max));
    DECLARE @CurrentPath NVARCHAR(MAX)
    DECLARE @Delimiter CHAR(1) = '/'
    DECLARE @Position INT
	DECLARE @Name as nvarchar(max) = '';

    -- Initialize the current path
    SET @CurrentPath = ''

    -- Loop through each part of the folder path
    WHILE LEN(@FolderPath) > 0
    BEGIN
        -- Find the position of the delimiter
        SET @Position = CHARINDEX(@Delimiter, @FolderPath)

        -- If the delimiter is found, extract the next part of the path
        IF @Position > 0
        BEGIN
			SET @Name = SUBSTRING(@FolderPath, 1, @Position);
            SET @CurrentPath = @CurrentPath + SUBSTRING(@FolderPath, 1, @Position)
            SET @FolderPath = SUBSTRING(@FolderPath, @Position + 1, LEN(@FolderPath) - @Position)
        END
        ELSE
        BEGIN
            -- If no delimiter is found, it means this is the last part of the path
			SET @Name = @FolderPath
            SET @CurrentPath = @CurrentPath + @FolderPath
            SET @FolderPath = ''
        END

        INSERT INTO @Results ([Path], [Name])
        SELECT RTRIM(@CurrentPath, '/'), RTRIM(@Name, '/');
    END

    RETURN;
END
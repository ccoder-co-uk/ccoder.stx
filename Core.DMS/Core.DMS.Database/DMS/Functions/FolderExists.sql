-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [DMS].[FolderExists]
(
	@UserId as nvarchar(100),
	@AppId as int,
	@Path as nvarchar(max)
)
RETURNS tinyint
AS
BEGIN
	DECLARE @Exists as tinyint = (
        SELECT CASE WHEN EXISTS (
            SELECT *
            FROM [DMS].[Folders] dmsFolder
            WHERE dmsFolder.AppId = @AppId AND dmsFolder.Path=@Path
        ) THEN 1
        ELSE 0
	    END
    );

	RETURN @Exists
END
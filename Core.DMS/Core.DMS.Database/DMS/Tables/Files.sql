CREATE TABLE [DMS].[Files] (
    [Id]          UNIQUEIDENTIFIER   NOT NULL,
    [FolderId]    UNIQUEIDENTIFIER   NOT NULL,
    [Name]        NVARCHAR (MAX)     NOT NULL,
    [Path]        NVARCHAR (MAX)     NOT NULL,
    [MimeType]    NVARCHAR (MAX)     NOT NULL,
    [CreatedBy]   NVARCHAR (MAX)     NULL,
    [CreatedOn]   DATETIMEOFFSET (7) DEFAULT ('0001-01-01T00:00:00.000+00:00') NOT NULL,
    [Description] NVARCHAR (MAX)     NULL,
    [Size]        NVARCHAR (10)      NULL,
    CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Files_Folders_FolderId] FOREIGN KEY ([FolderId]) REFERENCES [DMS].[Folders] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Files_FolderId]
    ON [DMS].[Files]([FolderId] ASC);


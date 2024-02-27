CREATE TABLE [DMS].[FileContents] (
    [Id]          UNIQUEIDENTIFIER   NOT NULL,
    [FileId]      UNIQUEIDENTIFIER   NOT NULL,
    [Version]     INT                NOT NULL,
    [RawData]     VARBINARY (MAX)    NULL,
    [CreatedBy]   NVARCHAR (MAX)     NULL,
    [CreatedOn]   DATETIMEOFFSET (7) DEFAULT ('0001-01-01T00:00:00.000+00:00') NOT NULL,
    [Description] NVARCHAR (MAX)     NULL,
    [Size]        NVARCHAR (10)      NULL,
    CONSTRAINT [PK_FileContents] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_FileContents_Files_FileId] FOREIGN KEY ([FileId]) REFERENCES [DMS].[Files] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FileContents_FileId]
    ON [DMS].[FileContents]([FileId] ASC);


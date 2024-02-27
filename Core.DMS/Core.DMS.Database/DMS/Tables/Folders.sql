CREATE TABLE [DMS].[Folders] (
    [Id]       UNIQUEIDENTIFIER NOT NULL,
    [AppId]    INT              NOT NULL,
    [ParentId] UNIQUEIDENTIFIER NULL,
    [Name]     NVARCHAR (MAX)   NULL,
    [Path]     NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_Folders] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Folders_Apps_AppId] FOREIGN KEY ([AppId]) REFERENCES [CMS].[Apps] ([Id]),
    CONSTRAINT [FK_Folders_Folders_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [DMS].[Folders] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Folders_AppId]
    ON [DMS].[Folders]([AppId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Folders_ParentId]
    ON [DMS].[Folders]([ParentId] ASC);


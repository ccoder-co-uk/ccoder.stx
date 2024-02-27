CREATE TABLE [Security].[FolderRoles] (
    [FolderId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId]   UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_FolderRoles] PRIMARY KEY CLUSTERED ([FolderId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_FolderRoles_Folders_FolderId] FOREIGN KEY ([FolderId]) REFERENCES [DMS].[Folders] ([Id]),
    CONSTRAINT [FK_FolderRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Security].[Roles] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FolderRoles_RoleId]
    ON [Security].[FolderRoles]([RoleId] ASC);


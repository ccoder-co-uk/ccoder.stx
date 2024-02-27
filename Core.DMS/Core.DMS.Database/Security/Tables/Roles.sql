CREATE TABLE [Security].[Roles] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [AppId]       INT              NOT NULL,
    [Name]        NVARCHAR (MAX)   NOT NULL,
    [Privs]       NVARCHAR (MAX)   NULL,
    [Description] NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Roles_Apps_AppId] FOREIGN KEY ([AppId]) REFERENCES [CMS].[Apps] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Roles_AppId]
    ON [Security].[Roles]([AppId] ASC);


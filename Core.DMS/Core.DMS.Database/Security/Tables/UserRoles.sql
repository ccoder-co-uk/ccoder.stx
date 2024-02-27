CREATE TABLE [Security].[UserRoles] (
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    [UserId] NVARCHAR (450)   NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([RoleId] ASC, [UserId] ASC),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Security].[Roles] ([Id]),
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Security].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserRoles_UserId]
    ON [Security].[UserRoles]([UserId] ASC);


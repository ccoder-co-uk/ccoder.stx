CREATE TABLE [CMS].[Apps] (
    [Id]     INT            IDENTITY (1, 1) NOT NULL,
    [Domain] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Apps] PRIMARY KEY CLUSTERED ([Id] ASC)
);


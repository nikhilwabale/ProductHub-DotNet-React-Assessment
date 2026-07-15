CREATE TABLE [dbo].[Product]([Id] INT IDENTITY PRIMARY KEY,[ProductName] NVARCHAR(255) NOT NULL,[Description] NVARCHAR(1000) NULL,[CreatedBy] NVARCHAR(100) NOT NULL,[CreatedOn] DATETIME2 NOT NULL,[ModifiedBy] NVARCHAR(100) NULL,[ModifiedOn] DATETIME2 NULL);
CREATE INDEX IX_Product_ProductName ON [dbo].[Product]([ProductName]);
CREATE TABLE [dbo].[Item]([Id] INT IDENTITY PRIMARY KEY,[ProductId] INT NOT NULL,[Quantity] INT NOT NULL,CONSTRAINT FK_Item_Product FOREIGN KEY([ProductId]) REFERENCES [dbo].[Product]([Id]) ON DELETE CASCADE);
CREATE INDEX IX_Item_ProductId ON [dbo].[Item]([ProductId]);

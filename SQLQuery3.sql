USE Database1;
GO

-- 1. Добавляем колонку статуса в таблицу заказов (если её еще нет)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'Status')
BEGIN
    ALTER TABLE Orders ADD Status NVARCHAR(50) DEFAULT N'В процессе приготовления' NOT NULL;
END
GO

-- 2. Добавляем колонку роли для пользователей (если её еще нет)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'Role')
BEGIN
    ALTER TABLE Users ADD Role NVARCHAR(50) DEFAULT N'Client' NOT NULL;
END
GO

-- 3. Добавляем колонку Password (если её еще нет)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'Password')
BEGIN
    ALTER TABLE Users ADD Password NVARCHAR(255) NULL;
END
GO

-- 4. Делаем колонку PasswordHash необязательной (NULL), чтобы избежать ошибок при регистрации
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Users ALTER COLUMN PasswordHash NVARCHAR(255) NULL;
END
GO

-- 5. Делаем поле FullName необязательным (NULL), чтобы регистрация новых пользователей работала без ошибок
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'FullName')
BEGIN
    ALTER TABLE Users ALTER COLUMN FullName NVARCHAR(100) NULL;
END
GO

-- 6. Создаем таблицу для учета смен (если она еще не создана)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Shifts' and xtype='U')
BEGIN
    CREATE TABLE Shifts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AdminEmail NVARCHAR(100) NOT NULL,
        OpenedAt DATETIME NOT NULL,
        ClosedAt DATETIME NULL,
        Status NVARCHAR(20) DEFAULT N'Open' NOT NULL
    );
END
GO

-- 7. Назначаем ваш аккаунт администратором и задаем пароль
UPDATE Users 
SET Role = 'Admin', Password = '123456' 
WHERE Email = 'adminPIZZA@gmail.com';
GO
-- Crear esquema si no existe (evitando DROP innecesario)
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo');
END
GO

-- Tabla TiposHabitacion (creada primero por dependencias)
CREATE TABLE dbo.TiposHabitacion (
    IdTipoHabitacion  INT IDENTITY(1,1) NOT NULL,
    NombreTipo        VARCHAR(50)  COLLATE Modern_Spanish_CI_AS NOT NULL,
    Descripcion       VARCHAR(255) COLLATE Modern_Spanish_CI_AS NULL,
    CapacidadMaxima   INT DEFAULT 1 NOT NULL,
    PrecioBase        DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_TiposHabitacion PRIMARY KEY (IdTipoHabitacion)
);
GO

-- Tabla Usuarios (dependencia básica)
CREATE TABLE dbo.Usuarios (
    IdUsuario       INT IDENTITY(1,1) NOT NULL,
    NombreUsuario   VARCHAR(50)  COLLATE Modern_Spanish_CI_AS NOT NULL,
    Contraseña      VARCHAR(256) COLLATE Modern_Spanish_CI_AS NOT NULL,  -- Modificado a 256
    Salt            VARCHAR(128) COLLATE Modern_Spanish_CI_AS NOT NULL DEFAULT '',  -- Nuevo campo
    RolUsuario      VARCHAR(20)  COLLATE Modern_Spanish_CI_AS NOT NULL,
    FechaCreacion   DATETIME DEFAULT GETDATE() NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_NombreUsuario UNIQUE (NombreUsuario),
    CONSTRAINT CK_RolUsuario CHECK (RolUsuario IN ('admin', 'cliente'))
);
GO

-- Tabla Habitaciones (depende de TiposHabitacion)
CREATE TABLE dbo.Habitaciones (
    IdHabitacion            INT IDENTITY(1,1) NOT NULL,
    NumeroHabitacion        VARCHAR(10) COLLATE Modern_Spanish_CI_AS NOT NULL,
    IdTipoHabitacion        INT NOT NULL,
    Estado                  VARCHAR(20) COLLATE Modern_Spanish_CI_AS DEFAULT 'disponible' NOT NULL,
    Piso                    INT NOT NULL,
    Detalles                VARCHAR(255) COLLATE Modern_Spanish_CI_AS NULL,
    FechaCreacion           DATETIME DEFAULT GETDATE() NULL,
    FechaUltimaActualizacion DATETIME DEFAULT GETDATE() NULL,
    CONSTRAINT PK_Habitaciones PRIMARY KEY (IdHabitacion),
    CONSTRAINT UQ_NumeroHabitacion UNIQUE (NumeroHabitacion),
    CONSTRAINT FK_Habitaciones_TiposHabitacion 
        FOREIGN KEY (IdTipoHabitacion) REFERENCES dbo.TiposHabitacion(IdTipoHabitacion),
    CONSTRAINT CK_EstadoHabitacion 
        CHECK (Estado IN ('disponible', 'ocupada', 'mantenimiento', 'reservada', 'limpieza'))
);
GO

-- Tabla Reservas (depende de Usuarios y Habitaciones)
CREATE TABLE dbo.Reservas (
    IdReserva        INT IDENTITY(1,1) NOT NULL,
    IdUsuario        INT NOT NULL,
    IdHabitacion     INT NOT NULL,
    FechaInicio      DATETIME NOT NULL,
    FechaFin         DATETIME NOT NULL,
    EstadoReserva    VARCHAR(20) COLLATE Modern_Spanish_CI_AS DEFAULT 'pendiente' NOT NULL,
    TotalPago        DECIMAL(10,2) NOT NULL,
    FechaCreacion    DATETIME DEFAULT GETDATE() NULL,
    CONSTRAINT PK_Reservas PRIMARY KEY (IdReserva),
    CONSTRAINT FK_Reservas_Habitaciones 
        FOREIGN KEY (IdHabitacion) REFERENCES dbo.Habitaciones(IdHabitacion),
    CONSTRAINT FK_Reservas_Usuarios 
        FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuarios(IdUsuario),
    CONSTRAINT CK_EstadoReserva 
        CHECK (EstadoReserva IN ('pendiente', 'confirmada', 'cancelada', 'completada', 'no-show'))
);
GO

-- Datos iniciales (ordenados por dependencias)
-- Insertar 10 tipos de habitaciones
INSERT INTO dbo.TiposHabitacion (NombreTipo, Descripcion, CapacidadMaxima, PrecioBase)
VALUES 
    (N'Single', N'Habitación individual estándar', 1, 500.00),
    (N'Doble', N'Habitación doble con dos camas', 2, 800.00),
    (N'Queen', N'Habitación con cama queen size', 2, 900.00),
    (N'King', N'Habitación con cama king size', 2, 1000.00),
    (N'Suite', N'Suite estándar con sala separada', 4, 1500.00),
    (N'Suite Ejecutiva', N'Suite con área de trabajo', 2, 1800.00),
    (N'Suite Presidencial', N'Suite de lujo con todos los servicios', 4, 3000.00),
    (N'Familiar', N'Habitación familiar grande', 6, 1200.00),
    (N'Adaptada', N'Habitación para personas con discapacidad', 2, 800.00),
    (N'Económica', N'Habitación básica económica', 1, 400.00);

INSERT INTO dbo.Usuarios (NombreUsuario, Contraseña, Salt, RolUsuario)
VALUES 
    (N'admin', N'oKdgzgSlg9RBPn97ENMVZNGrNgRS3IZjA32eO+ghcUU=', N'salt_admin_123', N'admin'),
    (N'cliente', N'xJoZnAXQ3eWJ7voTCAFly0iowblZYOPPDU2/ZVNding=', N'salt_cliente_456', N'cliente');

-- Insertar habitaciones para cada tipo
-- Tipo 1 (Single)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'101', 1, N'disponible', 1, N'Vista al jardín'),
    (N'102', 1, N'disponible', 1, N'Vista al jardín'),
    (N'103', 1, N'ocupada', 1, N'Vista a la calle'),
    (N'201', 1, N'mantenimiento', 2, N'Recién renovada');

-- Tipo 2 (Doble)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'104', 2, N'disponible', 1, N'Con balcón'),
    (N'105', 2, N'disponible', 1, N'Con balcón'),
    (N'202', 2, N'ocupada', 2, N'Vista al mar'),
    (N'203', 2, N'reservada', 2, N'Vista al mar');

-- Tipo 3 (Queen)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'301', 3, N'disponible', 3, N'Jacuzzi incluido'),
    (N'302', 3, N'disponible', 3, N'Jacuzzi incluido'),
    (N'303', 3, N'ocupada', 3, N'Minibar gratuito');

-- Tipo 4 (King)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'401', 4, N'disponible', 4, N'Sala de estar'),
    (N'402', 4, N'reservada', 4, N'Terraza privada'),
    (N'403', 4, N'mantenimiento', 4, N'En renovación');

-- Tipo 5 (Suite)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'501', 5, N'disponible', 5, N'Dos baños completos'),
    (N'502', 5, N'ocupada', 5, N'Cocina pequeña'),
    (N'503', 5, N'disponible', 5, N'Vista panorámica');

-- Tipo 6 (Suite Ejecutiva)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'601', 6, N'disponible', 6, N'Oficina equipada'),
    (N'602', 6, N'reservada', 6, N'Acceso a lounge');

-- Tipo 7 (Suite Presidencial)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'701', 7, N'disponible', 7, N'300m² de lujo'),
    (N'702', 7, N'ocupada', 7, N'Con servicio de mayordomo');

-- Tipo 8 (Familiar)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'801', 8, N'disponible', 8, N'3 camas dobles'),
    (N'802', 8, N'disponible', 8, N'Área de juegos');

-- Tipo 9 (Adaptada)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'901', 9, N'disponible', 9, N'Accesibilidad total'),
    (N'902', 9, N'disponible', 9, N'Baño adaptado');

-- Tipo 10 (Económica)
INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'1001', 10, N'disponible', 10, N'Sin ventana'),
    (N'1002', 10, N'ocupada', 10, N'Baño compartido'),
    (N'1003', 10, N'disponible', 10, N'Espacio reducido');
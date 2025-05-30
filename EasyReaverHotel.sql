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
    Contraseña      VARCHAR(100) COLLATE Modern_Spanish_CI_AS NOT NULL,
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
INSERT INTO dbo.TiposHabitacion (NombreTipo, Descripcion, CapacidadMaxima, PrecioBase)
VALUES (N'Grande', N'Habitacion Grande', 6, 1000.00);

INSERT INTO dbo.Usuarios (NombreUsuario, Contraseña, RolUsuario)
VALUES 
    (N'admin', N'admin', N'admin'),
    (N'cliente', N'cliente', N'cliente');

INSERT INTO dbo.Habitaciones (NumeroHabitacion, IdTipoHabitacion, Estado, Piso, Detalles)
VALUES 
    (N'1', 1, N'mantenimiento', 5, N'Grande'),
    (N'45', 1, N'disponible', 15, N'nose');
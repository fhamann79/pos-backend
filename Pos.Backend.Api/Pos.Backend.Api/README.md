# POS Backend – .NET 8 Web API

Backend del sistema POS + Inventario + Facturación electrónica (Ecuador).

---

## Tecnologías
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Swagger

---

## Requisitos
- .NET SDK 8
- PostgreSQL
- Visual Studio 2022 o VS Code

---

## Base de datos
Crear una base de datos en PostgreSQL llamada:

posdb_dev

---

## Configuración
Editar el archivo:

appsettings.Development.json

Ejemplo de cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=posdb_dev;Username=postgres;Password=TU_PASSWORD"
  }
}
```

## Ejecutar el proyecto

Desde la carpeta del proyecto:

dotnet restore
dotnet run

## URLs


API: https://localhost:7096

Swagger: https://localhost:7096/swagger

## Login de prueba (temporal)

Usuario: admin
Password: 1234

Este login es solo para pruebas iniciales.
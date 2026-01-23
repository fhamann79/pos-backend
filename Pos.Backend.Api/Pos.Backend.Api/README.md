# POS Backend – .NET 8 Web API

Backend del sistema POS (multiempresa) con inventario, ventas y facturación electrónica (Ecuador).

---

## Tecnologías

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Swagger

---

## Requisitos

- .NET 8 SDK
- PostgreSQL
- Visual Studio 2022 o VS Code

---

## Configuración

Editar el archivo `appsettings.Development.json` para ajustar:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`

Ejemplo de cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=posdb_dev;Username=postgres;Password=TU_PASSWORD"
  }
}
```

---

## Levantar el proyecto localmente

Desde la carpeta del proyecto (`Pos.Backend.Api/Pos.Backend.Api`):

```bash
dotnet restore
```

Ejecutar migraciones (EF Core):

```bash
dotnet ef database update
```

Ejecutar la API:

```bash
dotnet run
```

---

## URLs (desarrollo)

Los perfiles locales definen los siguientes puertos:

- HTTP: `http://localhost:5125`
- HTTPS: `https://localhost:7096`
- Swagger: `https://localhost:7096/swagger`

---

## Probar endpoints (Auth)

- `POST /api/auth/login`
- `POST /api/auth/register`
- `GET /api/auth/me` (requiere JWT en `Authorization: Bearer <token>`)

---

## Notas

- La API es **stateless** y usa **JWT** para autenticación.
- La configuración local vive en `appsettings.Development.json`.
- Para cambiar la base de datos, actualiza la cadena de conexión en el archivo de configuración.

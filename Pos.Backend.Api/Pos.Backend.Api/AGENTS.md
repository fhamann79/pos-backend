# AGENTS.md

Este archivo define **reglas, estándares y flujos de trabajo** para que Codex (y cualquier colaborador) pueda trabajar correctamente en el proyecto POS.

---

## PARTE A — POS BACKEND (.NET)

### 1. Stack técnico

* Plataforma: .NET (especificar versión exacta en `global.json` si existe)
* Tipo: API REST
* Arquitectura: Clean Architecture / Capas (API, Application, Domain, Infrastructure)
* ORM: Entity Framework Core (si aplica)
* Base de datos: (especificar: SQL Server / PostgreSQL / MySQL)

### 2. Principios obligatorios

* La **API es stateless**
* Separación estricta de capas
* Controllers **no contienen lógica de negocio**
* Toda lógica vive en Application/Services
* Validaciones en DTOs + capa Application
* Manejo centralizado de errores

### 3. Estructura esperada

```
pos-backend/
 ├─ src/
 │   ├─ Api/
 │   │   ├─ Controllers/
 │   │   ├─ Middleware/
 │   │   └─ Program.cs
 │   ├─ Application/
 │   │   ├─ DTOs/
 │   │   ├─ Interfaces/
 │   │   ├─ Services/
 │   │   └─ Validators/
 │   ├─ Domain/
 │   │   ├─ Entities/
 │   │   └─ Enums/
 │   └─ Infrastructure/
 │       ├─ Persistence/
 │       ├─ Repositories/
 │       └─ Migrations/
 └─ tests/
```

### 4. Convenciones

* Controllers: `ProductosController`, `VentasController`
* Endpoints REST claros (GET, POST, PUT, DELETE)
* DTOs: `ProductoDto`, `CreateProductoDto`
* Responses consistentes:

```json
{ "success": true, "data": {}, "message": "" }
```

### 5. Manejo de errores

* No lanzar excepciones genéricas
* Usar middleware global
* HTTP status claros: 400, 401, 403, 404, 409, 500

### 6. Tests

* Tests obligatorios para:

  * Servicios
  * Validaciones
* Framework: xUnit / NUnit
* Un PR sin tests **no se acepta**

### 7. Comandos comunes

```bash
dotnet restore
dotnet build
dotnet test
dotnet ef migrations add Nombre
dotnet ef database update
```

---

## REGLAS PARA CODEX / IA

* Antes de escribir código: **explicar el enfoque**
* No romper código existente
* Mantener estilo y convenciones
* Agregar tests cuando aplique
* Explicar cada cambio importante
* Si hay duda: preguntar antes de asumir

---

## OBJETIVO DEL PROYECTO

Construir un **POS sólido, mantenible y escalable**, priorizando:

* Claridad
* Seguridad
* Facilidad de evolución
* Aprendizaje del desarrollador (Fernando)

Este archivo es obligatorio y debe respetarse en cada cambio.

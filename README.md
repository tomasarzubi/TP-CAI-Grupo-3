# ECommerce – TP CAI Grupo 3

Sistema de e-commerce armado como **5 microservicios** independientes en **.NET 10 (ASP.NET Core Minimal APIs)**. Users, Products, Cart y Orders tienen su propia base **SQLite** (acceso con Dapper); Notifications guarda en memoria. Todos usan **Serilog** para logging, **Swagger** para documentación y **health checks**.

La solución completa se abre con `ECommerce.sln`. Los documentos de prueba de cada API están en la carpeta [`docs/`](docs/).

---

## Tabla de contenidos

1. [Requisitos](#requisitos)
2. [Arquitectura](#arquitectura)
3. [Puertos](#puertos)
4. [Cómo levantar los servicios](#cómo-levantar-los-servicios)
5. [Descripción de cada servicio](#descripción-de-cada-servicio)
6. [Códigos de error](#códigos-de-error)
7. [Estructura del repositorio](#estructura-del-repositorio)

---

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version` debe mostrar 10.x)
- Git
- No hace falta instalar ninguna base de datos: SQLite se crea sola.

```bash
git clone https://github.com/tomasarzubi/TP-CAI-Grupo-3.git
cd TP-CAI-Grupo-3
dotnet restore ECommerce.sln
```

---

## Arquitectura

```
                     ┌──────────────────────────────┐
                     │ Cliente (Swagger / Postman)  │
                     └──────────────┬───────────────┘
                                    │ REST / JSON
     ┌───────────┬──────────────┬───┴────────┬────────────────┐
     ▼           ▼              ▼            ▼                ▼
┌─────────┐ ┌──────────┐  ┌──────────┐ ┌──────────┐  ┌───────────────┐
│ Users   │ │ Products │  │  Cart    │ │ Orders   │  │ Notifications │
│ :5102   │ │ :5112    │  │  :5231   │ │ :5104    │  │ :5132         │
│ users.db│ │products  │  │  cart.db │ │ orders.db│  │ (en memoria)  │
└─────────┘ │   .db    │  └────┬─────┘ └──┬────┬──┘  └───────┬───────┘
     ▲      └──────────┘       │          │    │             │
     │           ▲  ▲          │          │    │             │
     │           │  └──────────┘          │    │             │
     │           │   Cart ──► Products    │    │             │
     │           │                        │    │             │
     │           ├────────────────────────┘    │             │
     │           │   Orders ──► Products       │             │
     │           │                             │             │
     │           └─────────────────────────────┤             │
     │               Products ──► Orders       │             │
     │         (órdenes activas antes de borrar)             │
     │                                         │             │
     ├─────────────────────────────────────────┘             │
     │   Orders ──► Users                                    │
     └───────────────────────────────────────────────────────┘
         Notifications ──► Users
```

### Dependencias entre servicios (llamadas HTTP)

| Servicio origen  | Llama a        | Para qué                                                         |
| ---------------- | -------------- | ---------------------------------------------------------------- |
| `Cart.API`       | `Products.API` | Obtener el producto y validar stock al agregar/modificar ítems   |
| `Orders.API`     | `Products.API` | Obtener producto, precio unitario y validar stock                |
| `Orders.API`     | `Users.API`    | Verificar que el usuario existe (`GET /api/users/{id}`)         |
| `Products.API`   | `Orders.API`   | Verificar órdenes activas antes de eliminar un producto          |
| `Notifications.API` | `Users.API` | Verificar que el usuario existe antes de enviar una notificación |

> Hay una dependencia circular en runtime entre **Products** y **Orders** (Orders consulta Products y Products consulta Orders). Por eso, para probar el borrado de productos conviene tener **todos** los servicios levantados.

---

## Puertos

Puertos HTTP definidos en `Properties/launchSettings.json` (perfil `http`). **Son los que usan los servicios para hablarse entre sí**, así que no conviene cambiarlos.

| Servicio            | Puerto HTTP | Puerto HTTPS (perfil `https`) | Base de datos | Swagger                         |
| ------------------- | :---------: | :---------------------------: | ------------- | ------------------------------- |
| `Users.API`         | **5102**    | 7102                          | `users.db`    | http://localhost:5102/swagger   |
| `Orders.API`        | **5104**    | 7104                          | `orders.db`   | http://localhost:5104/swagger   |
| `Products.API`      | **5112**    | 7048                          | `products.db` | http://localhost:5112/swagger   |
| `Notifications.API` | **5132**    | 7132                          | — (en memoria)| http://localhost:5132/swagger   |
| `Cart.API`          | **5231**    | 7204                          | `cart.db`     | http://localhost:5231/swagger   |

Health checks (en todos los servicios): `/health`, `/health/ready` y `/health/live`. Ejemplo: `http://localhost:5102/health`. Panel visual de Health Checks UI (donde está habilitado, por ejemplo en `Cart.API` y `Users.API`): `/health-ui`.

> ⚠️ **Notas sobre HTTPS** (inconsistencias del repo actual):
> Las URLs entre servicios están configuradas en **HTTP**. **Recomendación: usar siempre el perfil `http`** (el que usan los comandos de abajo).

---

## Cómo levantar los servicios

Los servicios se ejecutan en **terminales separadas** (una por servicio). El orden recomendado es el siguiente, porque algunos dependen de otros:

1. `Users.API`
2. `Products.API`
3. `Orders.API`
4. `Cart.API`
5. `Notifications.API`

Desde la raíz del repositorio:

```bash
# Terminal 1 – Users (5102)
dotnet run --project src/Users.API --launch-profile http

# Terminal 2 – Products (5112)
dotnet run --project src/Products.API --launch-profile http

# Terminal 3 – Orders (5104)
dotnet run --project src/Orders.API --launch-profile http

# Terminal 4 – Cart (5231)
dotnet run --project src/Cart.API --launch-profile http

# Terminal 5 – Notifications (5132)
dotnet run --project src/Notifications.API --launch-profile http
```

Qué esperar al arrancar:

- Users, Products, Cart y Orders crean su archivo `.db` (SQLite) y sus tablas la primera vez que se ejecutan (`CREATE TABLE IF NOT EXISTS`), en el directorio desde el que se ejecutan. No hace falta correr migraciones. Notifications guarda todo en memoria.
- Swagger UI queda disponible en `http://localhost:<puerto>/swagger` cuando el entorno es `Development` (es el default con `dotnet run` y el perfil `launchSettings`).
- Si hay que forzarlo: `ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/<Servicio>`.

### Con Visual Studio

1. Abrir `ECommerce.sln`.
2. Clic derecho sobre la solución → **Configure Startup Projects…** → **Multiple startup projects**.
3. Poner las 5 APIs en **Start** y ejecutar con el perfil `http`.

### Verificar que todo está arriba

```bash
for p in 5102 5104 5112 5132 5231; do
  echo -n "localhost:$p -> "; curl -s -o /dev/null -w "%{http_code}\n" http://localhost:$p/health/live
done
```

Todos deberían responder `200`.

### Configuración de URLs entre servicios

| Servicio       | Cómo configura las URLs de otros servicios                                          |
| -------------- | ----------------------------------------------------------------------------------- |
| `Cart.API`     | `appsettings.json` → `Services:ProductsApi:BaseUrl` (default `http://localhost:5112`) |
| `Orders.API`   | Hardcodeadas en `Extensions/ServicesExtensions.cs` (`5112` Products, `5102` Users)  |
| `Products.API` | Hardcodeada en `Extensions/ServicesExtensions.cs` (`5104` Orders)                   |
| `Notifications.API` | Hardcodeada en `Extensions/ServicesExtensions.cs` (`5102` Users)               |

Si se cambia un puerto, hay que actualizar también estas URLs.

---

## Descripción de cada servicio

### Users.API – puerto 5102
Alta y autenticación de usuarios. Bloquea la cuenta tras demasiados intentos fallidos de login o por detección de fraude.

| Método | Ruta                      | Descripción            | Respuestas        |
| ------ | ------------------------- | ---------------------- | ----------------- |
| POST   | `/api/users/register`     | Registrar usuario      | 201, 400, 409, 500 |
| POST   | `/api/users/login`        | Autenticar usuario     | 200, 400, 401, 403, 500 |
| GET    | `/api/users/{id}`         | Buscar usuario por ID  | 200, 404, 500     |

### Products.API – puerto 5112
CRUD de productos (nombre, categoría, precio, stock). No permite dos productos con el mismo nombre en la misma categoría ni borrar productos con órdenes activas.

| Método | Ruta                  | Descripción                                    |
| ------ | --------------------- | ---------------------------------------------- |
| GET    | `/api/products`       | Listar productos (filtros `categoria`, `nombre`) |
| GET    | `/api/products/{id}`  | Obtener un producto                            |
| POST   | `/api/products`       | Crear producto                                 |
| PUT    | `/api/products/{id}`  | Actualizar producto                            |
| DELETE | `/api/products/{id}`  | Eliminar producto (consulta a `Orders.API`)    |

### Cart.API – puerto 5231
Carrito de compras por usuario. Valida contra `Products.API` que el producto exista y que haya stock suficiente.

| Método | Ruta                                       | Descripción                      |
| ------ | ------------------------------------------ | -------------------------------- |
| GET    | `/api/cart/{userId}`                       | Ver carrito del usuario          |
| POST   | `/api/cart/{userId}/items`                 | Agregar ítem al carrito          |
| PUT    | `/api/cart/{userId}/items/{productId}`     | Modificar la cantidad de un ítem |
| DELETE | `/api/cart/{userId}/items/{productId}`     | Quitar un ítem                   |
| DELETE | `/api/cart/{userId}`                       | Vaciar el carrito                |

### Orders.API – puerto 5104
Creación y seguimiento de órdenes. Al crear una orden valida el usuario (`Users.API`), los productos y el stock (`Products.API`), calcula el total y la deja en estado `Pendiente`.

| Método | Ruta                       | Descripción                                |
| ------ | -------------------------- | ------------------------------------------ |
| GET    | `/api/orders`              | Listar órdenes (filtro opcional `usuarioId`) |
| GET    | `/api/orders/{id}`         | Obtener una orden                          |
| POST   | `/api/orders`              | Crear orden                                |
| PUT    | `/api/orders/{id}/status`  | Cambiar el estado de la orden              |

Transiciones de estado permitidas (cualquier otra devuelve `ORD-006`):

```
Pendiente ──► Confirmada ──► Enviada ──► Entregada
    │             │
    └──► Cancelada ◄──┘
```

| Estado actual | Puede pasar a             |
| ------------- | ------------------------- |
| Pendiente     | Confirmada, Cancelada     |
| Confirmada    | Enviada, Cancelada        |
| Enviada       | Entregada                 |
| Entregada     | — (final)                 |
| Cancelada     | — (final)                 |

### Notifications.API – puerto 5132
Envío y consulta de notificaciones por usuario. Verifica contra `Users.API` que el usuario exista. Los tipos válidos son `Email`, `Push` y `SMS`. **No usa SQLite**: las notificaciones se guardan en una lista en memoria, por lo que se pierden al reiniciar el servicio.

| Método | Ruta                              | Descripción                              |
| ------ | --------------------------------- | ---------------------------------------- |
| POST   | `/api/notifications/send`         | Enviar una notificación a un usuario     |
| GET    | `/api/notifications/{userId}`     | Listar las notificaciones de un usuario  |

### Aspectos comunes a todos los servicios

- **Correlation ID** y **auditoría** de requests mediante middleware propio.
- **Logging estructurado** con Serilog (consola + archivo).
- **Manejo global de errores** con respuestas JSON uniformes (ver sección siguiente).
- **Health checks** y Health Checks UI.

---

## Códigos de error

Todos los errores se devuelven como JSON con esta forma:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
  "title": "Conflict",
  "status": 409,
  "detail": "No se puede completar la operación por un conflicto.",
  "instance": "/api/users/register",
  "errorCode": "USR-001",
  "errorMessage": "El email 'x@y.com' ya está registrado.",
  "correlationId": "0HN..."
}
```

Reglas generales de mapeo a HTTP: *no encontrado* → **404**, *datos inválidos* → **400**, *regla de negocio* → **409** por defecto (o el status indicado en cada caso, como 401/403/422) y *error interno* → **500**.

### Users.API (`USR-`)

| Código  | HTTP | Descripción                                          |
| ------- | :--: | ---------------------------------------------------- |
| USR-001 | 409  | El email ya está registrado                          |
| USR-002 | 400  | Los datos del usuario son inválidos                  |
| USR-003 | 401  | Credenciales incorrectas                             |
| USR-004 | 403  | Usuario bloqueado por demasiados intentos fallidos   |
| USR-005 | 403  | Usuario bloqueado por detección de fraude            |
| USR-006 | 500  | Error interno al procesar el usuario                 |
| USR-007 | 404  | Usuario no encontrado (definido, pero hoy `GET /api/users/{id}` responde 404 sin cuerpo y no lo usa) |

### Products.API (`PRD-`)

| Código  | HTTP | Descripción                                     |
| ------- | :--: | ----------------------------------------------- |
| PRD-001 | 404  | Producto no encontrado                          |
| PRD-002 | 400  | Datos inválidos                                 |
| PRD-003 | 409  | Nombre duplicado en la categoría                |
| PRD-004 | 409  | El producto tiene órdenes activas (no se puede eliminar) |
| PRD-005 | 500  | Error interno                                   |

### Cart.API (`CRT-`)

| Código  | HTTP | Descripción                                          |
| ------- | :--: | ---------------------------------------------------- |
| CRT-001 | 404  | Carrito no encontrado                                |
| CRT-002 | 404  | Producto no encontrado (o no está en el carrito)     |
| CRT-003 | 422  | Stock insuficiente                                   |
| CRT-004 | 400  | Cantidad / datos inválidos                           |
| CRT-005 | 500  | Error interno del carrito                            |

### Orders.API (`ORD-`)

| Código  | HTTP | Descripción                                   |
| ------- | :--: | --------------------------------------------- |
| ORD-001 | 404  | Orden no encontrada                           |
| ORD-002 | 400  | Datos inválidos (incluye estado inexistente)  |
| ORD-003 | 404  | Usuario no encontrado                         |
| ORD-004 | 404  | Producto no encontrado                        |
| ORD-005 | 422  | Stock insuficiente                            |
| ORD-006 | 409  | Transición de estado inválida                 |
| ORD-007 | 500  | Error interno                                 |

### Notifications.API (`NTF-`)

| Código  | HTTP | Descripción                              |
| ------- | :--: | ---------------------------------------- |
| NTF-001 | 404  | Usuario no encontrado                    |
| NTF-002 | 400  | Datos inválidos                          |
| NTF-003 | 404  | Sin notificaciones para el usuario       |
| NTF-004 | 500  | Error interno                            |

---

## Estructura del repositorio

```
TP-CAI-Grupo-3/
├── ECommerce.sln
├── docs/                        # Documentos de prueba de cada API (.docx)
│   ├── Documento de Pruebas Users.API.docx
│   ├── Documento de Pruebas Products.API.docx
│   ├── Documento de Pruebas Cart.API.docx
│   ├── Documento de Pruebas Orders.API.docx
│   └── Documento de PruebasNotifications.API.docx
└── src/
    ├── Users.API/               # :5102
    ├── Products.API/            # :5112
    ├── Cart.API/                # :5231
    ├── Orders.API/              # :5104
    └── Notifications.API/       # :5132
```

Cada proyecto sigue la misma estructura interna: `Extensions/` (servicios, middleware y endpoints), `Services/`, `Data/` (repositorio + inicializador de SQLite), `DTOs/`, `Models/`, `Exceptions/` (incluye `ErrorCodes.cs`), `ExceptionHandlers/`, `Middleware/` y `HealthChecks/`.

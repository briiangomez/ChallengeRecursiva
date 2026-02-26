# Horoscope Challenge API (.NET 8)

## Tecnologías y Características

- **.NET 8 Web API**
- **Entity Framework Core** (SQL Server)
- **JWT Authentication** (con claims personalizados)
- **Caché Híbrido**:
  - **L1 (MemoryCache)**: Cache en memoria para acceso ultrarrápido.
  - **L2 (Database)**: Tabla `HoroscopeCache` para persistencia entre reinicios.
- **HttpClientFactory**: Consumo eficiente de la API externa `newastro`.
- **BCrypt**: Hashing seguro de contraseñas.
- **Swagger**: Documentación interactiva habilitada con soporte para JWT Bearer.
- **Tests Unitarios**: xUnit + Moq + FluentAssertions.

## Requisitos Funcionales Implementados

- [x] Registro y Login de usuarios.
- [x] Perfil editable (respetando la restricción de username).
- [x] Cálculo automático de signo zodiacal y días hasta el próximo cumpleaños.
- [x] Consulta de horóscopo del día con lógica de caché y persistencia de historial.
- [x] Endpoint para consultar el signo más buscado en el historial.

## Estructura del Proyecto

- `src/HoroscopeChallenge.Api`: Proyecto principal de la API.
  - `Domain`: Entidades de negocio y helpers lógicos.
  - `Data`: DbContext y configuraciones de EF.
  - `Repositories`: Capa de acceso a datos.
  - `Services`: Lógica de negocio y coordinación de caché/APIs.
- `tests/HoroscopeChallenge.Tests`: Tests unitarios para lógica crítica (signo, cumpleaños y servicio de horóscopo).

## Configuración y Ejecución

1.  **Base de Datos**: Configurar la cadena de conexión en `appsettings.json`.
2.  **Migraciones**: La API ejecuta `db.Database.Migrate()` automáticamente al iniciar.
3.  **Ejecutar**:
    ```bash
    dotnet run --project src/HoroscopeChallenge.Api/HoroscopeChallenge.Api.csproj
    ```
4.  **Tests**:
    ```bash
    dotnet test
    ```

## Endpoints Principales

- `POST /api/auth/register`: Crea un usuario.
- `POST /api/auth/login`: Obtiene el token JWT.
- `GET /api/users/me`: Perfil del usuario autenticado.
- `GET /api/horoscope/today`: Horóscopo + Días al cumple.
- `GET /api/horoscope/most-queried`: Signo más consultado.

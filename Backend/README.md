# PlantCare Academy - Backend API en C#

## 🚀 Descripción General

Backend profesional desarrollado en **C# .NET 8** para la plataforma educativa PlantCare Academy. Proporciona una API REST completa con autenticación JWT, gestión de usuarios, cursos, módulos, quizzes y certificados.

## 📋 Requisitos

- **.NET 8.0 SDK** o superior
- **SQL Server** 2019 o superior (o usar LocalDB)
- **Visual Studio 2022** o **Visual Studio Code**

## 🛠️ Instalación

### 1. Clonar y navegar al proyecto
```bash
cd Backend
```

### 2. Restaurar paquetes NuGet
```bash
dotnet restore
```

### 3. Configurar base de datos
Editar `appsettings.json` si necesitas cambiar la cadena de conexión:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PlantCareAcademy;Trusted_Connection=true;TrustServerCertificate=true;"
```

### 4. Aplicar migraciones
```bash
dotnet ef database update
```

## ▶️ Ejecutar la aplicación

```bash
dotnet run
```

La API estará disponible en: `https://localhost:7000`

Swagger UI disponible en: `https://localhost:7000`

## 📚 Estructura del Proyecto

```
Backend/
├── Models/
│   ├── DomainModels.cs          # Entidades principales
│   └── DTOs.cs                   # Data Transfer Objects
├── Services/
│   ├── AuthAndUserServices.cs    # Autenticación y usuarios
│   └── EnrollmentAndQuizServices.cs  # Inscripciones, quizzes, certificados
├── Data/
│   └── PlantCareDbContext.cs     # Entity Framework DbContext
├── Controllers/
│   └── ApiControllers.cs         # Controladores REST
├── Program.cs                     # Configuración de aplicación
├── appsettings.json              # Configuración
└── PlantCareAPI.csproj           # Archivo de proyecto
```

## 🔐 Autenticación JWT

### Registrar Usuario
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "password": "SecurePassword123!"
}
```

**Respuesta:**
```json
{
  "success": true,
  "message": "Registro exitoso",
  "data": {
    "success": true,
    "message": "Usuario registrado exitosamente",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "name": "Juan Pérez",
      "email": "juan@example.com",
      "isActive": true
    }
  }
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "juan@example.com",
  "password": "SecurePassword123!"
}
```

## 📖 Endpoints Principales

### Cursos
- `GET /api/courses` - Obtener todos los cursos
- `GET /api/courses/{id}` - Obtener curso específico
- `GET /api/courses/{courseId}/modules` - Obtener módulos del curso

### Inscripciones
- `POST /api/enrollments/enroll` - Inscribir usuario a curso
- `GET /api/enrollments/user/{userId}` - Obtener inscripciones del usuario
- `PUT /api/enrollments/{enrollmentId}/progress` - Actualizar progreso

### Quizzes
- `GET /api/quizzes/{id}` - Obtener quiz
- `POST /api/quizzes/{quizId}/submit` - Enviar respuestas
- `GET /api/quizzes/user/{userId}/results` - Obtener resultados

### Certificados
- `GET /api/certificates/user/{userId}` - Obtener certificados del usuario
- `GET /api/certificates/verify/{certificateNumber}` - Verificar certificado

## 🗄️ Modelos de Datos

### User
- Autenticación con contraseñas hasheadas (BCrypt)
- Perfil de usuario con imagen
- Relación con inscripciones, resultados de quiz y certificados

### Course
- Título, descripción, categoría
- Duración y nivel de dificultad
- Múltiples módulos

### Module
- Contenido educativo
- Ordenamiento secuencial
- Múltiples quizzes por módulo

### Quiz
- Múltiples preguntas
- Sistema de calificación automático
- Requisito de puntuación mínima

### Certificate
- Número único de certificado
- Validez de 2 años

## 🔑 Configuración de Seguridad

### Cambiar Secret Key JWT
Editar en `appsettings.json`:
```json
"Jwt": {
  "SecretKey": "Tu_Nueva_Clave_Super_Secreta_De_32_Caracteres!"
}
```

**⚠️ Importante:** En producción, usar variables de entorno para almacenar secrets.

## 📊 Funcionalidades Principales

✅ **Autenticación JWT** con acceso seguro  
✅ **Gestión de Usuarios** completa  
✅ **Cursos y Módulos** organizados jerárquicamente  
✅ **Sistema de Quizzes** con evaluación automática  
✅ **Certificados** digitales con número único  
✅ **Seguimiento de Progreso** en tiempo real  
✅ **Balance de Roles** para usuarios y administradores  
✅ **CORS configurado** para desarrollo local  
✅ **Swagger/OpenAPI** para documentación interactiva  

## 🧪 Pruebas

### Health Check
```bash
curl https://localhost:7000/health
```

### Usar Swagger UI
1. Navegar a `https://localhost:7000`
2. Probar cada endpoint interactivamente
3. Usar el botón "Authorize" para agregar JWT token

## 📝 Logging

Logs configurados en `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

## 🤝 Integración Frontend

En el archivo `index.html`, actualizar las llamadas fetch para apuntar a:
```javascript
const API_BASE = 'https://localhost:7000/api';

// Ejemplo de login
fetch(`${API_BASE}/auth/login`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'usuario@example.com',
    password: 'password123'
  })
})
.then(res => res.json())
.then(data => {
  if (data.success) {
    localStorage.setItem('token', data.data.token);
  }
});
```

## 🚀 Deploy a Producción

1. Publicar la aplicación:
```bash
dotnet publish -c Release -o ./publish
```

2. Usar appsettings.Production.json con valores reales
3. Configurar variables de entorno para secrets
4. Usar HTTPS obligatoriamente
5. Configurar base de datos en el servidor

## 📞 Soporte

Para problemas o preguntas sobre la API, revisar la documentación de Swagger en la raíz de la aplicación.

---

**Desarrollado con ❤️ para PlantCare Academy**

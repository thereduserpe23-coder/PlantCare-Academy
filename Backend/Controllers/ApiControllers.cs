using Microsoft.AspNetCore.Mvc;
using PlantCareAPI.Models;
using PlantCareAPI.Models.DTOs;
using PlantCareAPI.Services;

namespace PlantCareAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthenticationService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(ApiResponse<AuthResponse>.Error("Email y contraseña son requeridos"));

        if (await _userService.UserExistsAsync(request.Email))
            return BadRequest(ApiResponse<AuthResponse>.Error("El email ya está registrado"));

        var passwordHash = _authService.HashPassword(request.Password);
        var user = await _userService.CreateUserAsync(request.Name, request.Email, passwordHash);

        var token = _authService.GenerateToken(user);
        var response = new AuthResponse
        {
            Success = true,
            Message = "Usuario registrado exitosamente",
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            }
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Registro exitoso"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(ApiResponse<AuthResponse>.Error("Credenciales inválidas"));

        var token = _authService.GenerateToken(user);
        var response = new AuthResponse
        {
            Success = true,
            Message = "Inicio de sesión exitoso",
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            }
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Inicio de sesión exitoso"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile/{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Error("Usuario no encontrado"));

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            ProfileImage = user.ProfileImage,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };

        return Ok(ApiResponse<UserDto>.Ok(userDto, "Perfil obtenido"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CourseDto>>>> GetAllCourses()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        var courseDtos = courses.Select(c => new CourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Category = c.Category,
            ThumbnailUrl = c.ThumbnailUrl,
            DurationMinutes = c.DurationMinutes,
            DifficultyLevel = c.DifficultyLevel,
            ModuleCount = c.Modules.Count
        }).ToList();

        return Ok(ApiResponse<List<CourseDto>>.Ok(courseDtos, "Cursos obtenidos"));
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> GetCourse(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null)
            return NotFound(ApiResponse<CourseDto>.Error("Curso no encontrado"));

        var courseDto = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Category = course.Category,
            ThumbnailUrl = course.ThumbnailUrl,
            DurationMinutes = course.DurationMinutes,
            DifficultyLevel = course.DifficultyLevel,
            ModuleCount = course.Modules.Count
        };

        return Ok(ApiResponse<CourseDto>.Ok(courseDto, "Curso obtenido"));
    }

    [HttpGet("{courseId}/modules")]
    public async Task<ActionResult<ApiResponse<List<ModuleDto>>>> GetCourseModules(int courseId)
    {
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null)
            return NotFound(ApiResponse<List<ModuleDto>>.Error("Curso no encontrado"));

        var moduleDtos = course.Modules.Select(m => new ModuleDto
        {
            Id = m.Id,
            CourseId = m.CourseId,
            Title = m.Title,
            Content = m.Content,
            OrderIndex = m.OrderIndex,
            DurationMinutes = m.DurationMinutes
        }).OrderBy(m => m.OrderIndex).ToList();

        return Ok(ApiResponse<List<ModuleDto>>.Ok(moduleDtos, "Módulos obtenidos"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizzesController(IQuizService quizService)
    {
        _quizService = quizService;
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> GetQuiz(int id)
    {
        var quiz = await _quizService.GetQuizByIdAsync(id);
        if (quiz == null)
            return NotFound(ApiResponse<QuizDto>.Error("Quiz no encontrado"));

        var quizDto = new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            RequiredScore = quiz.RequiredScore,
            Questions = quiz.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                OrderIndex = q.OrderIndex,
                Answers = q.Answers.Select(a => new AnswerDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    OrderIndex = a.OrderIndex
                }).OrderBy(a => a.OrderIndex).ToList()
            }).OrderBy(q => q.OrderIndex).ToList()
        };

        return Ok(ApiResponse<QuizDto>.Ok(quizDto, "Quiz obtenido"));
    }

    [HttpPost("{quizId}/submit")]
    public async Task<ActionResult<ApiResponse<QuizResultDto>>> SubmitQuiz(int quizId, [FromBody] SubmitQuizRequest request, [FromQuery] int userId)
    {
        var result = await _quizService.SubmitQuizAsync(userId, quizId, request.Answers);

        var resultDto = new QuizResultDto
        {
            Id = result.Id,
            Score = result.Score,
            Passed = result.Passed,
            CorrectAnswers = result.CorrectAnswers,
            TotalQuestions = result.TotalQuestions,
            CompletedAt = result.CompletedAt
        };

        return Ok(ApiResponse<QuizResultDto>.Ok(resultDto, result.Passed ? "¡Quiz completado exitosamente!" : "Quiz completado. Necesitas mejorar tu puntuación."));
    }

    [HttpGet("user/{userId}/results")]
    public async Task<ActionResult<ApiResponse<List<QuizResultDto>>>> GetUserResults(int userId)
    {
        var results = await _quizService.GetUserQuizResultsAsync(userId);

        var resultDtos = results.Select(r => new QuizResultDto
        {
            Id = r.Id,
            Score = r.Score,
            Passed = r.Passed,
            CorrectAnswers = r.CorrectAnswers,
            TotalQuestions = r.TotalQuestions,
            CompletedAt = r.CompletedAt
        }).ToList();

        return Ok(ApiResponse<List<QuizResultDto>>.Ok(resultDtos, "Resultados obtenidos"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICertificateService _certificateService;

    public EnrollmentsController(IEnrollmentService enrollmentService, ICertificateService certificateService)
    {
        _enrollmentService = enrollmentService;
        _certificateService = certificateService;
    }

    [HttpPost("enroll")]
    public async Task<ActionResult<ApiResponse<CourseEnrollmentDto>>> EnrollCourse([FromQuery] int userId, [FromQuery] int courseId)
    {
        var enrollment = await _enrollmentService.EnrollUserAsync(userId, courseId);

        var enrollmentDto = new CourseEnrollmentDto
        {
            Id = enrollment.Id,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course.Title,
            ProgressPercentage = enrollment.ProgressPercentage,
            EnrolledAt = enrollment.EnrolledAt,
            IsCompleted = enrollment.IsCompleted
        };

        return Ok(ApiResponse<CourseEnrollmentDto>.Ok(enrollmentDto, "Inscripción exitosa"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<CourseEnrollmentDto>>>> GetUserEnrollments(int userId)
    {
        var enrollments = await _enrollmentService.GetUserEnrollmentsAsync(userId);

        var enrollmentDtos = enrollments.Select(e => new CourseEnrollmentDto
        {
            Id = e.Id,
            CourseId = e.CourseId,
            CourseTitle = e.Course.Title,
            ProgressPercentage = e.ProgressPercentage,
            EnrolledAt = e.EnrolledAt,
            CompletedAt = e.CompletedAt,
            IsCompleted = e.IsCompleted
        }).ToList();

        return Ok(ApiResponse<List<CourseEnrollmentDto>>.Ok(enrollmentDtos, "Inscripciones obtenidas"));
    }

    [HttpPut("{enrollmentId}/progress")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateProgress(int enrollmentId, [FromQuery] int progress)
    {
        await _enrollmentService.UpdateProgressAsync(enrollmentId, progress);
        return Ok(ApiResponse<string>.Ok("", "Progreso actualizado"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<CertificateDto>>>> GetUserCertificates(int userId)
    {
        var certificates = await _certificateService.GetUserCertificatesAsync(userId);

        var certificateDtos = certificates.Select(c => new CertificateDto
        {
            Id = c.Id,
            CertificateNumber = c.CertificateNumber,
            CourseName = c.Course.Title,
            UserName = c.User.Name,
            IssuedAt = c.IssuedAt,
            ValidUntil = c.ValidUntil,
            CertificateUrl = c.CertificateUrl
        }).ToList();

        return Ok(ApiResponse<List<CertificateDto>>.Ok(certificateDtos, "Certificados obtenidos"));
    }

    [HttpGet("verify/{certificateNumber}")]
    public async Task<ActionResult<ApiResponse<CertificateDto>>> VerifyCertificate(string certificateNumber)
    {
        var certificate = await _certificateService.GetCertificateByNumberAsync(certificateNumber);
        if (certificate == null)
            return NotFound(ApiResponse<CertificateDto>.Error("Certificado no encontrado"));

        var certificateDto = new CertificateDto
        {
            Id = certificate.Id,
            CertificateNumber = certificate.CertificateNumber,
            CourseName = certificate.Course.Title,
            UserName = certificate.User.Name,
            IssuedAt = certificate.IssuedAt,
            ValidUntil = certificate.ValidUntil,
            CertificateUrl = certificate.CertificateUrl
        };

        return Ok(ApiResponse<CertificateDto>.Ok(certificateDto, "Certificado verificado"));
    }
}

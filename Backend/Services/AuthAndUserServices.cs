using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using PlantCareAPI.Models;

namespace PlantCareAPI.Services;

public interface IAuthenticationService
{
    string GenerateToken(User user);
    bool VerifyPassword(string password, string hash);
    string HashPassword(string password);
}

public class JwtAuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;

    public JwtAuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key no configurado")));
        
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string email);
    Task<User> CreateUserAsync(string name, string email, string passwordHash);
    Task UpdateUserAsync(User user);
}

public class UserService : IUserService
{
    private readonly Data.PlantCareDbContext _context;

    public UserService(Data.PlantCareDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(string name, string email, string passwordHash)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}

public interface ICourseService
{
    Task<List<Models.Course>> GetAllCoursesAsync();
    Task<Models.Course?> GetCourseByIdAsync(int id);
    Task<Models.Course> CreateCourseAsync(Models.Course course);
    Task UpdateCourseAsync(Models.Course course);
    Task DeleteCourseAsync(int id);
}

public class CourseService : ICourseService
{
    private readonly Data.PlantCareDbContext _context;

    public CourseService(Data.PlantCareDbContext context)
    {
        _context = context;
    }

    public async Task<List<Models.Course>> GetAllCoursesAsync()
    {
        return await _context.Courses
            .Where(c => c.IsPublished)
            .Include(c => c.Modules)
            .ToListAsync();
    }

    public async Task<Models.Course?> GetCourseByIdAsync(int id)
    {
        return await _context.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Quizzes)
                    .ThenInclude(q => q.Questions)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Models.Course> CreateCourseAsync(Models.Course course)
    {
        course.CreatedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task UpdateCourseAsync(Models.Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}

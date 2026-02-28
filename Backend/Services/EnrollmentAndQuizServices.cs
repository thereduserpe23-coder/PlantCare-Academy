using PlantCareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace PlantCareAPI.Services;

public interface IEnrollmentService
{
    Task<CourseEnrollment> EnrollUserAsync(int userId, int courseId);
    Task<List<CourseEnrollment>> GetUserEnrollmentsAsync(int userId);
    Task<CourseEnrollment?> GetEnrollmentAsync(int userId, int courseId);
    Task UpdateProgressAsync(int enrollmentId, int progress);
    Task CompleteEnrollmentAsync(int enrollmentId);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly Data.PlantCareDbContext _context;

    public EnrollmentService(Data.PlantCareDbContext context)
    {
        _context = context;
    }

    public async Task<CourseEnrollment> EnrollUserAsync(int userId, int courseId)
    {
        var existingEnrollment = await _context.CourseEnrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (existingEnrollment != null)
            return existingEnrollment;

        var enrollment = new CourseEnrollment
        {
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            ProgressPercentage = 0
        };

        _context.CourseEnrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<List<CourseEnrollment>> GetUserEnrollmentsAsync(int userId)
    {
        return await _context.CourseEnrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
            .ToListAsync();
    }

    public async Task<CourseEnrollment?> GetEnrollmentAsync(int userId, int courseId)
    {
        return await _context.CourseEnrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
    }

    public async Task UpdateProgressAsync(int enrollmentId, int progress)
    {
        var enrollment = await _context.CourseEnrollments.FindAsync(enrollmentId);
        if (enrollment != null)
        {
            enrollment.ProgressPercentage = Math.Min(progress, 100);
            _context.CourseEnrollments.Update(enrollment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CompleteEnrollmentAsync(int enrollmentId)
    {
        var enrollment = await _context.CourseEnrollments.FindAsync(enrollmentId);
        if (enrollment != null)
        {
            enrollment.IsCompleted = true;
            enrollment.CompletedAt = DateTime.UtcNow;
            enrollment.ProgressPercentage = 100;
            _context.CourseEnrollments.Update(enrollment);
            await _context.SaveChangesAsync();
        }
    }
}

public interface IQuizService
{
    Task<Quiz?> GetQuizByIdAsync(int id);
    Task<List<Quiz>> GetModuleQuizzesAsync(int moduleId);
    Task<QuizResult> SubmitQuizAsync(int userId, int quizId, Dictionary<int, int> answers);
    Task<List<QuizResult>> GetUserQuizResultsAsync(int userId);
}

public class QuizService : IQuizService
{
    private readonly Data.PlantCareDbContext _context;

    public QuizService(Data.PlantCareDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetQuizByIdAsync(int id)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Quiz>> GetModuleQuizzesAsync(int moduleId)
    {
        return await _context.Quizzes
            .Where(q => q.ModuleId == moduleId)
            .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
            .ToListAsync();
    }

    public async Task<QuizResult> SubmitQuizAsync(int userId, int quizId, Dictionary<int, int> answers)
    {
        var quiz = await GetQuizByIdAsync(quizId);
        if (quiz == null)
            throw new InvalidOperationException("Quiz no encontrado");

        int correctCount = 0;
        int totalQuestions = quiz.Questions.Count;

        foreach (var answer in answers)
        {
            var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.Key);
            if (question != null)
            {
                var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect && a.Id == answer.Value);
                if (correctAnswer != null)
                    correctCount++;
            }
        }

        int score = (int)((correctCount * 100) / totalQuestions);
        bool passed = score >= quiz.RequiredScore;

        var result = new QuizResult
        {
            UserId = userId,
            QuizId = quizId,
            Score = score,
            Passed = passed,
            CorrectAnswers = correctCount,
            TotalQuestions = totalQuestions,
            CompletedAt = DateTime.UtcNow
        };

        _context.QuizResults.Add(result);
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<List<QuizResult>> GetUserQuizResultsAsync(int userId)
    {
        return await _context.QuizResults
            .Where(qr => qr.UserId == userId)
            .Include(qr => qr.Quiz)
            .OrderByDescending(qr => qr.CompletedAt)
            .ToListAsync();
    }
}

public interface ICertificateService
{
    Task<Certificate> IssueCertificateAsync(int userId, int courseId);
    Task<List<Certificate>> GetUserCertificatesAsync(int userId);
    Task<Certificate?> GetCertificateByNumberAsync(string certificateNumber);
}

public class CertificateService : ICertificateService
{
    private readonly Data.PlantCareDbContext _context;

    public CertificateService(Data.PlantCareDbContext context)
    {
        _context = context;
    }

    public async Task<Certificate> IssueCertificateAsync(int userId, int courseId)
    {
        var existingCert = await _context.Certificates
            .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);

        if (existingCert != null)
            return existingCert;

        var certificate = new Certificate
        {
            UserId = userId,
            CourseId = courseId,
            CertificateNumber = GenerateCertificateNumber(),
            IssuedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddYears(2),
            CertificateUrl = $"/certificates/{Guid.NewGuid()}.pdf"
        };

        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();
        return certificate;
    }

    public async Task<List<Certificate>> GetUserCertificatesAsync(int userId)
    {
        return await _context.Certificates
            .Where(c => c.UserId == userId)
            .Include(c => c.Course)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync();
    }

    public async Task<Certificate?> GetCertificateByNumberAsync(string certificateNumber)
    {
        return await _context.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);
    }

    private string GenerateCertificateNumber()
    {
        return $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}

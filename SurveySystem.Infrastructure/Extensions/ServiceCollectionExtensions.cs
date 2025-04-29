using Microsoft.Extensions.DependencyInjection;
using SurveySystem.Core.Interfaces;
//using SurveySystem.Core.Services;
//using SurveySystem.Infrastructure.Auth;
using SurveySystem.Infrastructure.Repositories;
using SurveySystem.Infrastructure.Services;

namespace SurveySystem.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register DB Context
            //services.AddDbContext<AppDbContext>(options =>
            //    options.UseSqlServer(
            //        configuration.GetConnectionString("DefaultConnection"),
            //        sqlOptions => sqlOptions.EnableRetryOnFailure()
            //    )
            //);

            // Register repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ISurveyRepository, SurveyRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<ISurveyResponseRepository, SurveyResponseRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // Register services
            services.AddScoped<ISurveyService, SurveyService>();
            services.AddScoped<IAuditService, AuditService>();
            //services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            //services.AddScoped< IActiveDirectoryService , ActiveDirectoryService>();
            return services;
        }
    }
}
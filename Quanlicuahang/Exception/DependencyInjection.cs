using Microsoft.Extensions.DependencyInjection;
using Quanlicuahang.Repositories;
using Quanlicuahang.Services;

namespace Quanlicuahang.Exception
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            /* Category*/
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();

            /*USER*/
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<UserService>();

            /*AUTH*/
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<AuthService>();

            /*ACTION LOG*/
            services.AddScoped<IActionLogRepository, ActionLogRepository>();
            services.AddScoped<IActionLogService, ActionLogService>();

            return services;
        }
    }
}

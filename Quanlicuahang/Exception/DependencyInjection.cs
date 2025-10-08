using Microsoft.Extensions.DependencyInjection;
using Quanlicuahang.Repositories;
using Quanlicuahang.Services;

namespace Quanlicuahang.Exception
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Category
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();


            // Đăng ký Repositorys
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<UserService>();


            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<AuthService>();


            return services;
        }
    }
}

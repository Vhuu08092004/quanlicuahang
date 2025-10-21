using Quanlicuahang.Helpers;
using Quanlicuahang.Repositories;
using Quanlicuahang.Services;

namespace Quanlicuahang.Exception
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            /* Category */
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();

            /* Supplier */
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ISupplierService, SupplierService>();

            /* USER */
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();

            /* AUTH */
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<AuthService>();

            /* ACTION LOG */
            services.AddScoped<IActionLogRepository, ActionLogRepository>();
            services.AddScoped<IActionLogService, ActionLogService>();

            /* PRODUCT */
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            /* PRODUCT ATTRIBUTE */
            services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
            services.AddScoped<IProductAttributeService, ProductAttributeService>();

            /* PRODUCT ATTRIBUTE VALUE */
            services.AddScoped<IProductAttributeValueRepository, ProductAttributeValueRepository>();
            services.AddScoped<IProductAttributeValueService, ProductAttributeValueService>();

            /* HELPERS */
            services.AddScoped<ITokenHelper, TokenHelper>();
          
            /* PROMOTION */
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IPromotionService, PromotionService>();

            // Report
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IReportService, ReportService>();

            /* ORDER & PAYMENT */
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentService, PaymentService>();


            return services;
        }
    }
}

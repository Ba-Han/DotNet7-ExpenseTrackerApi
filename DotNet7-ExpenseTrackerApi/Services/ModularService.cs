﻿using DotNet7_ExpenseTrackerApi.Middleware;
using Microsoft.EntityFrameworkCore;

namespace DotNet7_ExpenseTrackerApi.Services;

public static class ModularService
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        services.AddCustomServices().AddDbContextService(builder);
        return services;
    }

    #region AddAuthorizationMiddleware
    public static IApplicationBuilder AddAuthorizationMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuthorizationMiddleware>();
        return app;
    }
    #endregion

    #region AddCustomServices
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddScoped<AdoDotNetService>();
        services.AddSingleton<AesService>();
        services.AddScoped<JwtService>();

        return services;
    }
    #endregion

    #region AddDbContextService
    public static IServiceCollection AddDbContextService(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
    #endregion
}

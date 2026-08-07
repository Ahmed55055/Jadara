using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reward_Flow_v2.Common;
using Reward_Flow_v2.Common.EmployeeLookup;
using Reward_Flow_v2.Common.ErrorHandling;
using Reward_Flow_v2.Common.Interceptors;
using Reward_Flow_v2.Common.Tokenization;
using Reward_Flow_v2.Common.UserIdRetrieval;
using Reward_Flow_v2.Employees;
using Reward_Flow_v2.Employees.BulkInsertEmployees;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Employees.Data.Database;
using Reward_Flow_v2.Employees.Shared;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward;
using Reward_Flow_v2.Rewards.SessionsReward.Common;
using Reward_Flow_v2.Rewards.SessionsReward.EndPoints;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;
using Reward_Flow_v2.User;
using Reward_Flow_v2.User.AuthService;
using Reward_Flow_v2.User.Data.Database;
using RewardFlow_API.Rewards.Common;
using RewardFlow_API.Rewards.Data;
using RewardFlow_API.Common.Interface;
using RewardFlow_API.Employees.Common;
using RewardFlow_API.Rewards.Courses;
using RewardFlow_API.User.AuthService;
using Scalar.AspNetCore;
using UserContext = RewardFlow_API.Common.Interface.UserContext;

namespace Reward_Flow_v2;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddExceptionHandling();
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        // Register services
        AppConfiguration.Initialize(builder.Configuration);
        builder.Services.AddScoped<IUserRetrievalService, UserRetrievalService>();
        builder.Services.AddScoped<ITokenizer, Tokenizer>();
        builder.Services.AddScoped<IEmployeeTokenService, EmployeeTokenService>();
        builder.Services.AddScoped<ISessionRewardCalculator, SessionsRewardCalculator>();
        builder.Services.AddScoped<ISessionRewardRules, SessionRewardRules>();
        builder.Services.AddScoped<IEmployeeLookupService,EmployeeLookupService>();
        builder.Services.AddScoped<ISessionRewardService,SessionRewardService>();
        builder.Services.AddScoped<ISnapshotService<Employee,EmployeeSnapshot>,EmployeeSnapshotService>();
        builder.Services.AddScoped<ISnapshotService<TermCourse,CourseSnapshot>,SubjectSnapshotService>();
        builder.Services.AddScoped<ScopedUserContext>();
        builder.Services.AddScoped<IUserContext,UserContext>();
        builder.Services.AddScoped<IBulkEmployeesImporter,BulkEmployeesImportJob>();
        builder.Services.AddScoped<ITokenBackgroundJob,TokenBackgroundJob>();
        builder.Services.AddScoped<IBulkInserter<EmployeeNameToken>, EmployeeTokenBulkInsert>();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<TokenService>();
        
        // Register Interceptors
        builder.Services
            .AddScoped<AuditSaveChangesInterceptor>()
            .AddScoped<TenantSaveChangesInterceptor>();
       
        // DbContexts
        builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddDbContext<RewardDbContext>( (sp,options) =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            
            var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
            var tenantInterceptor = sp.GetRequiredService<TenantSaveChangesInterceptor>();

            options.AddInterceptors(auditInterceptor, tenantInterceptor);
        });
        
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        builder.Services.AddDbContext<EmployeeDbContext>((sp,options) =>
        {
            options.UseSqlServer(connectionString);
            
            var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
            var tenantInterceptor = sp.GetRequiredService<TenantSaveChangesInterceptor>();

            options.AddInterceptors(auditInterceptor, tenantInterceptor);
        });
        
        // Hangfire
        builder.Services.AddHangfire(config => config
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddHangfireServer();
        //builder.Services.AddScoped<IBulkInsertBackgroundJob, BulkInsertBackgroundJob>();
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Token"]!)),
                };
            });

        
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            //app.EnsureDatabasesCreated();
        }

        // Redirect root to Scalar API documentation
        app.MapGet("/", () => Results.Redirect("/scalar/v1"));

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.MapUsers();
        app.MapEmployeeEndpoints();
        app.MapSessionRewardEndpoints();
        app.MapCourseEndpoints();
        
        app.Run();
    }
}


using CalendarAssistant.ActionFilters;
using CalendarAssistant.Middlewares;
using CalendarAssistant.Models;
using CalendarAssistant.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using OpenAI.Chat;
using System.Text;

namespace CalendarAssistant
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration["dbConnectionString"]!;

            // For Entity Framework
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            services.AddDbContext<CalendarAssistantContext>(options => options.UseSqlServer(connectionString));

            // For Identity
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // For Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })


            // Adding Jwt Bearer
            .AddJwtBearer(options =>
             {
                 options.SaveToken = true;
                 options.RequireHttpsMetadata = false;
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidAudience = configuration["JWT:ValidAudience"],
                     ValidIssuer = configuration["JWT:ValidIssuer"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!))
                 };
             });

          

            services.AddControllers();
            services.AddSwaggerGen();
            services.AddEndpointsApiExplorer();

            services.Configure<GoogleCalendarSettings>(configuration.GetSection(nameof(GoogleCalendarSettings)));
            services.AddSingleton<IGoogleCalendarSettings>(s => s.GetRequiredService<IOptions<GoogleCalendarSettings>>().Value);
            services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILlmService, LlmService>();

            services.AddScoped<IGoogleGmailService, GoogleGmailService>();
            services.AddScoped<IPythonExecutorService, PythonExecutorService>();
            services.AddScoped<IHttpService, HttpService>();

            services.AddScoped<IScheduleService, ScheduleService>();

            services.AddScoped<TokenValidationFilter>();

            //For Hangfire Tasks
            services.AddHangfire(config => config.UseMemoryStorage());
            services.AddHangfireServer();
            //For Background Tasks
            //services.AddHostedService<HostedService>();

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins("http://localhost:5173/");
                                  });
            });

        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // User Middleware
            app.UseMiddleware<UserMiddleware>();

            app.UseCors(x => x
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .SetIsOriginAllowed(origin => true) // allow any origin
                      .AllowCredentials()); // allow credentials

            // Add a default root endpoint
            app.MapGet("/", () => "Application is running");
            
            var loggerFactory = app.Services.GetService<ILoggerFactory>();
            loggerFactory.AddFile(_configuration["Logging:LogFilePath"]);
            app.MapControllers();

            app.UseHangfireDashboard();

        }
    }
}

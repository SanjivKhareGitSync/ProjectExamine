
namespace ProjectExamine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ============================================
            // DEPENDENCY INJECTION REGISTRATION
            // ============================================
            // This is where we register services with the DI container
            // When a controller requests IProductService in its constructor,
            // ASP.NET Core will automatically provide an instance based on
            // the registration below.
            //
            // LIFETIME OPTIONS:
            // - AddSingleton: One instance for the entire application lifetime
            // - AddScoped: One instance per HTTP request (most common for services)
            // - AddTransient: A new instance every time it's requested
            //
            // We use AddScoped for this example because:
            // - Each HTTP request can have its own service instance
            // - Good for services that access databases or state
            //
            // ============================================
            // THE MAGIC OF DEPENDENCY INJECTION
            // ============================================
            // Just change the SECOND type parameter to switch implementations!
            // 
            // TRY DIFFERENT IMPLEMENTATIONS:
            // 
            // Option 1: IN-MEMORY IMPLEMENTATION (Current - Fast, good for local testing)
            builder.Services.AddScoped<ProjectExamine.Controllers.IProductService, ProjectExamine.Controllers.ProductService>();

            // Option 2: DATABASE IMPLEMENTATION (Different behavior, logs show "Database:" operations)
            // builder.Services.AddScoped<ProjectExamine.Controllers.IProductService, ProjectExamine.Services.ProductServiceDatabase>();

            // Option 3: CACHED IMPLEMENTATION (With logging - wraps in-memory service with caching layer)
            // builder.Services.AddScoped<ProjectExamine.Controllers.ProductService>();
            // builder.Services.AddScoped<ProjectExamine.Controllers.IProductService, ProjectExamine.Services.ProductServiceCached>(
            //     provider => new ProjectExamine.Services.ProductServiceCached(
            //         provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ProjectExamine.Services.ProductServiceCached>>(),
            //         provider.GetRequiredService<ProjectExamine.Controllers.ProductService>()
            //     )
            // );

            // Option 4: MOCK IMPLEMENTATION (For testing without database)
            // builder.Services.AddScoped<ProjectExamine.Controllers.IProductService, ProjectExamine.Services.ProductServiceMock>();
            //
            // ============================================
            // KEY BENEFIT OF DEPENDENCY INJECTION:
            // ============================================
            // - Change ONE line in Program.cs
            // - ProductController doesn't need any changes!
            // - All the services injected through the constructor keep working
            // - Easy to test (inject mock)
            // - Easy to switch implementations (in-memory vs database)
            // - Easy to add cross-cutting concerns (caching, logging, etc.)
            //
            // WITHOUT DI:
            // You'd need to find and change EVERY place that instantiates the service.
            // High risk of bugs, harder to maintain, more code to modify.

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

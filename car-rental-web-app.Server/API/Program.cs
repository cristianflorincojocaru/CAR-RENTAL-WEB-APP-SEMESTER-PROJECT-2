using CarRental.API.Extensions;
using CarRental.API.Middleware;
using CarRental.Domain.Entities;
using CarRental.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ── Services ───────────────────────────────────────────────────────────────

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Return standardised validation error format
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(new
            {
                status = 400,
                title = "Validation Failed",
                errors
            });
        };
    });

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddCorsPolicy();

var app = builder.Build();

// ── Seed database ──────────────────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    await DbSeeder.SeedAsync(db, hasher);
}

// ── Middleware pipeline ────────────────────────────────────────────────────

app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WheelDeal Car Rental API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableFilter();
    });
}

app.UseHttpsRedirection();

app.UseCors(app.Environment.IsDevelopment() ? "AngularDev" : "Production");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})).AllowAnonymous();

app.Run();

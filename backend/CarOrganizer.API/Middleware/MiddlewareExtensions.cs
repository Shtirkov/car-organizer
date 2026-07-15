namespace CarOrganizer.API.Middleware;

/// <summary>
/// Groups the HTTP request pipeline in one place so Program.cs stays readable. Order matters:
/// authentication runs before authorization, and both run before the endpoints. Add future
/// middleware here (custom middleware classes live in this folder and are wired up below).
/// </summary>
public static class MiddlewareExtensions
{
    public static WebApplication UseApiMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Car Organizer API v1"));
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}

using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BlogPlatform API",
        Version = "v1",
        Description = "RESTful API for a simple blogging platform. Backend coding challenge.",
        Contact = new OpenApiContact
        {
            Name = "Hudson Steel",
            Url = new Uri("https://github.com/hudsonsteel")
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogPlatform API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "BlogPlatform.Api" }))
   .WithName("HealthCheck")
   .WithTags("Health");

app.Run();

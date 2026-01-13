using Microsoft.Azure.Cosmos;
using NhsTechTest.Application;
using NhsTechTest.Infrastructure;
using NhsTechTest.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NHS Fleet Solutions API",
        Version = "v1",
        Description = "API for managing patient information and retrieving patient summaries",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Northumbria Healthcare Team",
            Email = "support--test@northumbria-healthcare.nhs.uk"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();


if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
    var settings = scope.ServiceProvider.GetRequiredService<CosmosDbSettings>();
    var container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);

    await NhsTechTest.Infrastructure.Seeding.CosmosDbSeeder.SeedDataAsync(container);
}

app.Run();

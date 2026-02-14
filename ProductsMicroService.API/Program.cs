using BusinessLogicLayer;
using DataAccessLayer;
using ProductsMicroService.API.APiEndpoints;
using ProductsMicroService.API.Middleware;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccessLayer(builder.Configuration); //36
builder.Services.AddBusinessLogicLayer();

// Add Controllers
builder.Services.AddControllers();

//Configure JSON options for minimal APIs (camelCase naming and enum as numbers)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
//builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters
//.Add(new JsonStringEnumConverter()));

// Add Authentication and Authorization services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Allow requests from the specified origin (e.g., Angular development server)
        //.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();

// Add Swagger services for API documentation(29)
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandlingMiddleware();

// Routing
app.UseRouting();

app.UseSwagger(); //47
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductsMicroService.API v1")
); //47
app.UseCors();

// Auth
app.UseHttpsRedirection(); // 47
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapProductAPIEndpoints(); // 43

app.Run();

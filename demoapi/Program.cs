using demoapi.Data;
using demoapi.Services.Assesments;
using demoapi.Services.Assessments;
using demoapi.Services.Courses;
using demoapi.Services.Enrollments;
using demoapi.Services.Scorecards;
using demoapi.Services.Scores;
using demoapi.Services.Students;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// find the difference between AddScoped, AddTransient, and AddSingleton in ASP.NET Core dependency injection
//builder.Services.AddScoped<IStudentService, StudentServiceImpl>();
//builder.Services.AddTransient<IStudentService, StudentServiceImpl>();
//builder.Services.AddSingleton<IStudentService, StudentServiceImpl>();

//builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<IStudentService, StudentServiceImpl>();
builder.Services.AddScoped<ICourseService, CourseServiceImpl>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentServiceImpl>();
builder.Services.AddScoped<IAssesmentService, AssessmentServiceImpl>();
builder.Services.AddScoped<IScoreService, ScoreServiceImpl>();
builder.Services.AddScoped<IScorecardService, ScorecardServiceImpl>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

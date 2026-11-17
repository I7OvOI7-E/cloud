using Microsoft.EntityFrameworkCore;
using WebApplication6;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.OpenApi.Models;
using WebApplication6.Service;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi); 
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.MapType<JObject>(() => new OpenApiSchema { Type = "object" }));
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
    });

builder.Services.AddDbContext<MyDbContextFoodHero>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<ServiceSchoolStaff>();
builder.Services.AddScoped<ServiceRecipientStaff>();
builder.Services.AddScoped<ServiceVolunteer>();
builder.Services.AddScoped<ServiceAdmin>();
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
var app = builder.Build();


var imagePath = Path.Combine(builder.Environment.ContentRootPath, "Image");
Directory.CreateDirectory(imagePath);   // create if missing at startup

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagePath),
    RequestPath = "/Image"
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseCors("AllowAll");

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

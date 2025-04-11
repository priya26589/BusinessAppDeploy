using Business.Data;
using Business.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<GeocodingService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
            builder => builder.WithOrigins("https://sasmita2622606.github.io")
            .AllowAnyOrigin()//deployed url https://sasmita2622606.github.io, http://localhost:4200
                              .AllowAnyHeader()
                              .AllowAnyMethod());
});
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<SubAdminServices>();
//builder.Services.AddDbContext<BusinessContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<BusinessContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Configuration.AddJsonFile(@"C:\inetpub\wwwroot\businessapp\appsettings.json", optional: false, reloadOnChange: true);

var app = builder.Build();

// Serve static files from the 'uploads' directory
app.UseStaticFiles(new StaticFileOptions
{
   
});

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();

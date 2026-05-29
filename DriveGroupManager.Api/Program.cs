using DriveGroupManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<IDriveService, DriveService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Configure CORS for WebView2
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 在后台运行，不占用控制台
app.RunAsync();

// 保持应用程序运行
await Task.Delay(Timeout.Infinite);

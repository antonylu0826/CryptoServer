using CryptoServer.Services;
using CryptoServer.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加健康檢查
builder.Services.AddHealthChecks();

var key = builder.Configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not found in configuration");
builder.Services.AddScoped<IEncryptionService, EncryptionService>(provider => new EncryptionService(key));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 添加 IP 限制中間件
app.UseHttpsRedirection();
app.UseMiddleware<IpRestrictionMiddleware>();

app.UseAuthorization();

app.MapControllers();

// 添加健康檢查端點
app.MapHealthChecks("/health");

app.Run();

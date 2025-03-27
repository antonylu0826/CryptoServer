using CryptoServer.Services;
using CryptoServer.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

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

// 添加安全標頭中間件
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    await next().ConfigureAwait(false);
});

app.UseMiddleware<IpRestrictionMiddleware>();

app.UseAuthorization();

app.MapControllers();

// 添加全局錯誤處理中間件
app.UseExceptionHandler(errorApp =>
{
    var logUnhandledException = LoggerMessage.Define<Exception>(
        LogLevel.Error,
        new EventId(1, "UnhandledException"),
        "An unhandled exception occurred: {Exception}");

    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logUnhandledException(logger, error.Error, null);
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An internal server error occurred",
                requestId = context.TraceIdentifier
            }).ConfigureAwait(false);
        }
    });
});

// 添加健康檢查端點
app.MapHealthChecks("/health");

app.Run();

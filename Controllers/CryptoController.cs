using Microsoft.AspNetCore.Mvc;
using CryptoServer.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace CryptoServer.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class CryptoController : ControllerBase
{
    private static readonly Action<ILogger, string, Exception?> LogEncryptionArgumentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(LogEncryptionArgumentError)),
            "Encryption failed: {Message}");

    private static readonly Action<ILogger, Exception?> LogEncryptionCryptoError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2, nameof(LogEncryptionCryptoError)),
            "Encryption failed: Cryptographic error");

    private static readonly Action<ILogger, string, Exception?> LogDecryptionArgumentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3, nameof(LogDecryptionArgumentError)),
            "Decryption failed: {Message}");

    private static readonly Action<ILogger, Exception?> LogDecryptionCryptoError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(4, nameof(LogDecryptionCryptoError)),
            "Decryption failed: Cryptographic error");

    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<CryptoController> _logger;

    public CryptoController(IEncryptionService encryptionService, ILogger<CryptoController> logger)
    {
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("encrypt")]
    public IActionResult Encrypt([FromBody] TextRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Text))
            {
                return BadRequest(new { error = "Text cannot be empty" });
            }

            var encryptedText = _encryptionService.Encrypt(request.Text);
            return Ok(new { result = encryptedText });
        }
        catch (ArgumentException ex)
        {
            LogEncryptionArgumentError(_logger, ex.Message, ex);
            return BadRequest(new { error = ex.Message });
        }
        catch (CryptographicException ex)
        {
            LogEncryptionCryptoError(_logger, ex);
            return StatusCode(500, new { error = "Encryption failed" });
        }
    }

    [HttpPost("decrypt")]
    public IActionResult Decrypt([FromBody] TextRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Text))
            {
                return BadRequest(new { error = "Text cannot be empty" });
            }

            var decryptedText = _encryptionService.Decrypt(request.Text);
            return Ok(new { result = decryptedText });
        }
        catch (ArgumentException ex)
        {
            LogDecryptionArgumentError(_logger, ex.Message, ex);
            return BadRequest(new { error = ex.Message });
        }
        catch (CryptographicException ex)
        {
            LogDecryptionCryptoError(_logger, ex);
            return StatusCode(500, new { error = "Decryption failed" });
        }
    }
}

internal class TextRequest
{
    [Required(ErrorMessage = "Text is required")]
    [StringLength(10000, ErrorMessage = "Text length must be between 1 and 10000 characters")]
    [RegularExpression(@"^[\x20-\x7E\xA0-\xFF]+$", ErrorMessage = "Text contains invalid characters")]
    public string Text { get; set; } = string.Empty;
} 
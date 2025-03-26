using Microsoft.AspNetCore.Mvc;
using CryptoServer.Services;

namespace CryptoServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CryptoController : ControllerBase
{
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Encryption failed");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decryption failed");
            return StatusCode(500, new { error = "Decryption failed" });
        }
    }
}

public class TextRequest
{
    public string Text { get; set; } = string.Empty;
} 
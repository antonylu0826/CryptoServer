# CryptoServer

CryptoServer is a .NET 8-based encryption service API that provides text encryption and decryption functionality. The service uses the AES encryption algorithm and implements security features such as IP restrictions and environment variable configuration.

## Features

- Text encryption and decryption API
- AES encryption algorithm
- IP access restrictions
- Environment variable configuration
- Swagger UI documentation
- Docker containerization
- Docker Compose support

## System Requirements

- .NET 8 SDK
- Docker Desktop
- Docker Compose

## Installation

1. Start the service using Docker Compose:
```bash
docker-compose up -d
```

The service will run at `http://localhost:5237`.

## API Endpoints

### Encryption API
- **URL**: `/api/crypto/encrypt`
- **Method**: POST
- **Request Body**:
```json
{
    "text": "Text to encrypt"
}
```
- **Response**:
```json
{
    "result": "Encrypted Base64 string"
}
```

### Decryption API
- **URL**: `/api/crypto/decrypt`
- **Method**: POST
- **Request Body**:
```json
{
    "text": "Encrypted Base64 string"
}
```
- **Response**:
```json
{
    "result": "Decrypted original text"
}
```

## Environment Variables

The following environment variables can be configured in `docker-compose.yml`:

- `ASPNETCORE_URLS`: API service URL
- `Encryption__Key`: Encryption key
- `AllowedIps__*`: List of allowed IP addresses

## API Documentation

After starting the service, you can view the complete API documentation through Swagger UI:
```
http://localhost:5237/swagger
```

## Error Handling

The API returns standardized error responses:

```json
{
    "error": "Error message"
}
```

Common errors:
- 400 Bad Request: Invalid request format
- 401 Unauthorized: IP not authorized
- 500 Internal Server Error: Server internal error

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contribution

Welcome to submit issues and pull requests.

## Contact Information

GitHub: [antonylu0826](https://github.com/antonylu0826) 
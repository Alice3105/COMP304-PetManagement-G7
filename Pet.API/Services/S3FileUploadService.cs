using Amazon.S3;
using Amazon.S3.Model;
using Pet.API.Services.Interfaces;

namespace Pet.API.Services
{
    public class S3FileUploadService : IFileUploadService, IDisposable
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<S3FileUploadService> _logger;
        private readonly string _bucketName;
        private readonly string _defaultFolder;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB in bytes
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public S3FileUploadService(IConfiguration configuration, ILogger<S3FileUploadService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var accessKey = _configuration["AWS:AccessKey"];
            var secretKey = _configuration["AWS:SecretKey"];
            var region = _configuration["AWS:S3:Region"] ?? _configuration["AWS:Region"] ?? "us-east-1";

            // If credentials are provided in config, use them; otherwise use default credential chain (IAM roles, env vars, etc.)
            if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
            {
                _s3Client = new AmazonS3Client(
                    accessKey,
                    secretKey,
                    Amazon.RegionEndpoint.GetBySystemName(region)
                );
            }
            else
            {
                // Use default credential chain (IAM roles, environment variables, etc.)
                _s3Client = new AmazonS3Client(
                    Amazon.RegionEndpoint.GetBySystemName(region)
                );
            }

            _bucketName = _configuration["AWS:S3:BucketName"] ?? "petshelter-images";
            _defaultFolder = _configuration["AWS:S3:PetPhotosFolder"] ?? "pets/";
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string? folder = null)
        {
            // Validate file
            ValidateFile(fileStream, fileName);

            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            var targetFolder = folder ?? _defaultFolder;
            
            // If folder is specified, use the provided fileName (for structured paths like pets/{PetId}/{Name}-{increment}.jpg)
            // Otherwise, generate a unique filename for backward compatibility
            string finalFileName;
            if (folder != null)
            {
                // Use the provided fileName as-is (it should already include the extension)
                finalFileName = fileName;
            }
            else
            {
                // Generate unique file name for backward compatibility
                finalFileName = $"{Guid.NewGuid()}{fileExtension}";
            }
            
            var key = $"{targetFolder}{finalFileName}";

            try
            {
                fileStream.Position = 0;

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = fileStream,
                    ContentType = GetContentType(fileExtension)
                };

                var response = await _s3Client.PutObjectAsync(putRequest);

                var url = $"https://{_bucketName}.s3.amazonaws.com/{key}";

                _logger.LogInformation($"File uploaded successfully: {key}");

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upload file to S3: {fileName}");
                throw new Exception($"Failed to upload file: {ex.Message}", ex);
            }
        }

        private void ValidateFile(Stream fileStream, string fileName)
        {
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException(
                    $"Invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}");
            }

            if (fileStream.Length > MaxFileSize)
            {
                throw new ArgumentException(
                    $"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");
            }

            if (fileStream.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        public void Dispose()
        {
            _s3Client?.Dispose();
        }
    }
}


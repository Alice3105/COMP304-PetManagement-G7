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

            _s3Client = new AmazonS3Client(
                accessKey,
                secretKey,
                Amazon.RegionEndpoint.GetBySystemName(region)
            );

            _bucketName = _configuration["AWS:S3:BucketName"] ?? "petshelter-images";
            _defaultFolder = _configuration["AWS:S3:PetPhotosFolder"] ?? "pets/";
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string? folder = null)
        {
            // Validate file
            ValidateFile(fileStream, fileName);

            // Generate unique file name
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var targetFolder = folder ?? _defaultFolder;
            var key = $"{targetFolder}{uniqueFileName}";

            try
            {
                fileStream.Position = 0;

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = fileStream,
                    ContentType = GetContentType(fileExtension),
                    CannedACL = S3CannedACL.PublicRead // Make images publicly accessible
                };

                var response = await _s3Client.PutObjectAsync(putRequest);

                var region = _configuration["AWS:S3:Region"] ?? _configuration["AWS:Region"] ?? "us-east-1";
                var url = $"https://{_bucketName}.s3.{region}.amazonaws.com/{key}";

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


using Minio;
using Minio.DataModel.Args;

namespace ConsolidatedApi.Services
{
    public class MinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration;

        public MinioService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            var endpoint = _configuration["Minio:Endpoint"] ?? "localhost:9000";
            var accessKey = _configuration["Minio:AccessKey"] ?? "minioadmin";
            var secretKey = _configuration["Minio:SecretKey"] ?? "minioadmin";
            
            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .Build();
        }

        public async Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream, string contentType)
        {
            try
            {
                // Check if bucket exists, create if not
                var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
                if (!bucketExists)
                {
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                }

                // Upload file
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType));

                return $"{bucketName}/{objectName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file to MinIO: {ex.Message}", ex);
            }
        }

        public async Task<Stream> GetFileAsync(string bucketName, string objectName)
        {
            try
            {
                var stream = new MemoryStream();
                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream => stream.CopyTo(stream)));
                
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving file from MinIO: {ex.Message}", ex);
            }
        }

        public async Task DeleteFileAsync(string bucketName, string objectName)
        {
            try
            {
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file from MinIO: {ex.Message}", ex);
            }
        }
    }
}
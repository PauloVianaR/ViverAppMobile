using Amazon.S3;
using Amazon.S3.Model;

namespace ViverAppApi.Helpers
{
    public class B2StorageService
    {
        private readonly AmazonS3Client _s3Client;
        private const string _bucketName = "ViverAppBucket";

        public B2StorageService()
        {
            _s3Client = new AmazonS3Client(
                "005236b726b09ed0000000002",
                "K0057I2P9IyqowT2A8ziQPIhgFxDtYI",
                new AmazonS3Config
                {
                    ServiceURL = "https://s3.us-east-005.backblazeb2.com",
                    ForcePathStyle = true
                });
        }

        public async Task<string[]> UploadFileAsync(Stream stream, string fileName)
        {
            try
            {
                string finalFileName = fileName;

                string baseName = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);

                int counter = 1;
                while (await FileExistsAsync(finalFileName))
                {
                    finalFileName = $"{baseName}_{counter}{extension}";
                    counter++;
                }

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = finalFileName,
                    InputStream = stream,
                    ContentType = "application/octet-stream"
                };

                await _s3Client.PutObjectAsync(putRequest);

                string urlFileName = finalFileName.Replace(' ', '+').Trim();
                string url = $"https://f005.backblazeb2.com/file/{_bucketName}/{urlFileName}";

                return [url, finalFileName];
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao enviar o arquivo '{fileName}': {ex.Message}", ex);
            }
        }

        private async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(_bucketName, fileName);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;
                throw;
            }
        }


        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            using var response = await _s3Client.GetObjectAsync(_bucketName, fileName);
            using var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task DeleteFileAsync(string fileName)
        {
            await _s3Client.DeleteObjectAsync(_bucketName, fileName);
        }
    }
}

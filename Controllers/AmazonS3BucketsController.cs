using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;


namespace AmazonS3Buckets.Controllers
{
    [Route("api/s3/v1")]
    [ApiController]
    public class AmazonS3BucketsController : ControllerBase
    {
        private readonly ILogger<AmazonS3BucketsController> _logger;

        public AmazonS3BucketsController(ILogger<AmazonS3BucketsController> logger)
        {
            _logger = logger;

        }

        [HttpPost("createBucket")]
        public async Task<IActionResult> CreateBucket([FromQuery] string bucketNameCreate)
        {
            var s3Client = new AmazonS3Client();

            try
            {
                _logger.LogInformation($"Criando bucket {bucketNameCreate}...");

                var createBucketResponse = await s3Client.PutBucketAsync(bucketNameCreate);

                _logger.LogInformation($"Resposta: {createBucketResponse.HttpStatusCode}");

                return Ok($"Bucket {bucketNameCreate} criado com sucesso!");

            }
            catch (Exception ex)
            {
                return BadRequest($"Ocorreu um erro ao criar o bucket: {ex.Message}");
            }

        }


        [HttpGet("buckets")]
        public async Task<List<S3Bucket>> GetBuckets()
        {
            try
            {
                var s3Client = new AmazonS3Client();

                _logger.LogInformation("Listando buckets...");

                var listBucketResponse = await MyListBucketsAsync(s3Client);

                _logger.LogInformation($"Total de buckets: {listBucketResponse.Buckets.Count}");

                var listBuckets = new List<S3Bucket>();

                foreach (S3Bucket bucket in listBucketResponse.Buckets)
                    listBuckets.Add(bucket);


                return listBuckets;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Houve um erro ao criar o bucket: {ex.Message}");
                return new List<S3Bucket>();
            }
        }

        [HttpPost("createObjectBucket")]
        public async Task<IActionResult> CreateObjectBucket([FromQuery] string fileName, [FromQuery] string keyName, [FromQuery] string bucketName)
        {
            try
            {
                var s3Client = new AmazonS3Client();

                string extension = string.Empty;

                _logger.LogInformation("Verificando extensão do arquivo...");

                if (!fileName.EndsWith(".docx") && !fileName.EndsWith(".txt"))
                {
                    extension = ".docx";
                    fileName += extension;
                }

                _logger.LogInformation($"Criando o arquivo {fileName}");

                var fileObject = System.IO.File.Create(fileName);

                _logger.LogInformation("Montando o objeto de requisição do S3");

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = fileObject.Name
                };

                _logger.LogInformation("Fechando arquivo");

                fileObject.Close();

                _logger.LogInformation("Realizando o envio do arquivo para o bucket");

                var response = await s3Client.PutObjectAsync(request);
              

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao enviar objeto para bucket: {ex.Message}");
            }
        }

        [HttpGet("objectsBucket")]
        public async Task<List<S3Object>> GetObjectsBuckets([FromQuery] string bucketName)
        {
            try
            {
                var s3Client = new AmazonS3Client();

                _logger.LogInformation($"Obtendo objetos do bucket {bucketName}");

                var objects = await s3Client.ListObjectsAsync(bucketName);

                var objectsResponse = new List<S3Object>();

                _logger.LogInformation("Listando objetos e adicionando para a lista de objetos");

                foreach (var objectBucket in objects.S3Objects)
                    objectsResponse.Add(objectBucket);

                return objectsResponse;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Houve um erro ao retornar os objetos: {ex.Message}");
                return new List<S3Object>();
            }
        }

        [HttpDelete("deleteBucket")]
        public async Task<IActionResult> DeleteBucket([FromQuery] string bucketName)
        {
            try
            {
                var srClient = new AmazonS3Client();

                _logger.LogInformation($"Removendo bucket {bucketName}");

                var deleteBucket = await srClient.DeleteBucketAsync(bucketName);

                return Ok(deleteBucket);
            }
            catch (Exception ex)
            {
                return BadRequest($"Houve um erro ao excluir o bucket: {ex.Message}");
            }
        }


        private static async Task<ListBucketsResponse> MyListBucketsAsync(IAmazonS3 s3Client)
        {
            return await s3Client.ListBucketsAsync();
        }
    }
}

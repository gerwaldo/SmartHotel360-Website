using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using SmartHotel360.PublicWeb.Models.Settings;
using SmartHotel360.PublicWeb.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHotel360.PublicWeb.Controllers
{

    public class PetUploadRequest
    {
        public string Base64 { get; set; }
        public string Name { get; set; }
    }

    [Route("api/pets")]
    public class PetsApiController : Controller
    {
        private readonly LocalSettings _settings;
        private readonly string dbName = "pets";
        private readonly string colName = "checks";

        public PetsApiController(IOptions<LocalSettings> settings)
        {
            _settings = settings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> UploadPetImageAsync([FromBody] PetUploadRequest petRequest)
        {

            if (string.IsNullOrEmpty(petRequest?.Base64))
            {
                return BadRequest();
            }

            var tokens = petRequest.Base64.Split(',');
            var ctype = tokens[0].Replace("data:", "");
            var base64 = tokens[1];
            var content = Convert.FromBase64String(base64);

            // Upload photo to storage...
            var blobUri = await UploadPetToStorage(content);

            // Then create a Document in CosmosDb to notify our Function
            var identifier = await UploadDocument(blobUri, petRequest.Name ?? "Bob");

            return Ok(identifier);
        }

        private async Task<Guid> UploadDocument(Uri uri, string petName)
        {
            var cosmosClient = new CosmosClient(_settings.PetsConfig.CosmosUri, _settings.PetsConfig.CosmosKey);
            var identifier = Guid.NewGuid();

            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
            var container = await database.Database.CreateContainerIfNotExistsAsync(colName, "/id");

            await container.Container.CreateItemAsync(
                new PetDocument
                {
                    Id = identifier,
                    IsApproved = null,
                    PetName = petName,
                    MediaUrl = uri.ToString(),
                    Created = DateTime.UtcNow
                });

            return identifier;
        }

        private async Task<Uri> UploadPetToStorage(byte[] content)
        {
            // Create connection string from storage name and key
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={_settings.PetsConfig.BlobName};AccountKey={_settings.PetsConfig.BlobKey};EndpointSuffix=core.windows.net";
            var uploader = new PhotoUploader(connectionString);
            var blob = await uploader.UploadPetPhoto(content);
            return blob.Uri;
        }

        [HttpGet]
        public async Task<IActionResult> GetUploadState(Guid identifier)
        {
            var cosmosClient = new CosmosClient(_settings.PetsConfig.CosmosUri, _settings.PetsConfig.CosmosKey);
            var container = cosmosClient.GetContainer(dbName, colName);

            try
            {
                var response = await container.ReadItemAsync<PetDocument>(identifier.ToString(), new PartitionKey(identifier.ToString()));
                var doc = response.Resource;

                return Ok(new
                {
                    Approved = doc?.IsApproved ?? false,
                    Message = doc?.Message ?? ""
                });
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Ok(new
                {
                    Approved = false,
                    Message = "Document not found or still processing"
                });
            }
        }
    }
}

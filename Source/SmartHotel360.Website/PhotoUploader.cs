using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SmartHotel360.PublicWeb
{
    public class PhotoUploader
    {
        private readonly BlobServiceClient _blobServiceClient;

        public PhotoUploader(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<BlobClient> UploadPetPhoto(byte[] content)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("pets");
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobName = Guid.NewGuid().ToString();
            var blobClient = containerClient.GetBlobClient(blobName);
            
            using var stream = new MemoryStream(content);
            await blobClient.UploadAsync(stream, overwrite: true);
            
            return blobClient;
        }
    }
}
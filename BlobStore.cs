using Azure;
using Azure.Storage.Blobs;
using Microsoft.Bot.Schema;
using Microsoft.TeamsFx.Conversation;
using Newtonsoft.Json;
using System.Text;

namespace TeamsBotStoreExample
{
    public class BlobStore : IConversationReferenceStore
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStore(string storeConnectionString, string storeContainerName)
        {
            _containerClient = new BlobContainerClient(storeConnectionString, storeContainerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<ConversationReference> Get(string key)
        {
            var blobName = GetBlobName(key);

            try
            {
                var response = await _containerClient.GetBlobClient(blobName).DownloadAsync();
                using var streamReader = new StreamReader(response.Value.Content);
                var content = await streamReader.ReadToEndAsync();

                return JsonConvert.DeserializeObject<ConversationReference>(content);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == 404)
                    return null;

                throw;
            }
        }

        public async Task<bool> Add(string key, ConversationReference reference, ConversationReferenceStoreAddOptions options, CancellationToken cancellationToken = default)
        {
            var blobName = GetBlobName(key);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if ((options.Overwrite ?? false) || await Get(key) == null)
            {
                var content = JsonConvert.SerializeObject(reference);
                await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)), true, cancellationToken);

                return true;
            }

            return false;
        }

        public async Task<PagedData<ConversationReference>> List(int? pageSize = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var conversationReferences = new List<ConversationReference>();
            var pagedBlobs = _containerClient.GetBlobsAsync(cancellationToken: cancellationToken).AsPages(continuationToken, pageSizeHint: pageSize);

            await foreach (var pagedBlob in pagedBlobs)
            {
                foreach (var blobItem in pagedBlob.Values)
                {
                    try
                    {
                        var response = await _containerClient.GetBlobClient(blobItem.Name).DownloadAsync(cancellationToken);
                        using var streamReader = new StreamReader(response.Value.Content);
                        var content = await streamReader.ReadToEndAsync();

                        conversationReferences.Add(JsonConvert.DeserializeObject<ConversationReference>(content));
                    }
                    catch (RequestFailedException ex)
                    {
                        if (ex.Status != 404)
                            throw;
                    }
                    catch (JsonException) { }
                }
            }

            return new PagedData<ConversationReference>
            {
                Data = conversationReferences.ToArray(),
                ContinuationToken = null
            };
        }

        public async Task<bool> Remove(string key, ConversationReference reference, CancellationToken cancellationToken = default)
        {
            var blobName = GetBlobName(key);

            try
            {
                await _containerClient.GetBlobClient(blobName).DeleteAsync(cancellationToken: cancellationToken);

                return true;
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status != 404)
                    throw;

                return false;
            }
        }

        private static string GetBlobName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var invalidChars = new string(Path.GetInvalidFileNameChars());
            var sanitizedBlobName = string.Join("_", key.Split(invalidChars.ToCharArray()));

            return sanitizedBlobName;
        }
    }
}

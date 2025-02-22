// Program.cs
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "YourAzureBlobConnectionString";
        string containerName = "my-container";
        string filePath = "path_to_large_file.ext";
        string blobName = "uploaded_file.ext";

        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await UploadLargeFileAsync(containerClient, filePath, blobName);
    }

    static IEnumerable<byte[]> ReadFileInChunks(string filePath, int chunkSize)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[chunkSize];
            int bytesRead;
            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                yield return buffer.Take(bytesRead).ToArray();
            }
        }
    }

    static async Task UploadLargeFileAsync(BlobContainerClient containerClient, string filePath, string blobName)
    {
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        BlockBlobClient blockBlobClient = blobClient.GetBlockBlobClient();

        int chunkSize = 8 * 1024 * 1024; // 8MB
        var blockIds = new List<string>();

        var chunks = ReadFileInChunks(filePath, chunkSize).Select((chunk, index) => new { chunk, index });

        await Parallel.ForEachAsync(chunks, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (item, token) =>
        {
            string blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(item.index.ToString("D6")));
            using (var ms = new MemoryStream(item.chunk))
            {
                await StageBlockWithRetryAsync(blockBlobClient, blockId, ms);
                Console.WriteLine($"Uploaded chunk {item.index + 1}");
            }
            lock (blockIds)
            {
                blockIds.Add(blockId);
            }
        });

        await blockBlobClient.CommitBlockListAsync(blockIds);
        Console.WriteLine("Upload complete! 🎉");
    }

    static async Task StageBlockWithRetryAsync(BlockBlobClient blockBlobClient, string blockId, MemoryStream ms, int retries = 3)
    {
        for (int attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                ms.Position = 0; // Reset stream for retry
                await blockBlobClient.StageBlockAsync(blockId, ms);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading block {blockId}: {ex.Message}");
                if (attempt == retries - 1) throw;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
            }
        }
    }
}


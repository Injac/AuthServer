using AuthService.Logging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Storage
{
    /// <summary>
    /// Basic storage management functions.
    /// (c) 2013 ExGrip LLC & Ilija Injac
    /// All rights reserved.
    /// </summary>
    class AzureBlobStorageManager:IStorageManager
    {
        //Used to move files or directories
        private static List<string> files = new List<string>();
        private static List<string> newFiles = new List<string>();
       

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageManager"/> class.
        /// </summary>
        public AzureBlobStorageManager()
        {
           
        }


        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns></returns>
        public async Task<bool> CreateDirectory(CloudBlobContainer container, string directoryName)
        {
            MemoryStream stream = new MemoryStream(File.ReadAllBytes(@"dummy\dummy.txt"));
            var fileName = String.Format("{0}/{1}", directoryName, "dummy.txt");
            var exists = BlobExistsOnCloud(container, fileName);

            if (exists)
            {
                return false;
            }

            var blob = container.GetBlockBlobReference(fileName);
            await blob.UploadFromStreamAsync(stream);
            return true;
        }

        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="blob">The BLOB.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        public async Task<bool> CopyFile(CloudBlobContainer container, Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob blob, string newFileName)
        {
            await blob.FetchAttributesAsync();
            Uri uri = new Uri(blob.Name);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            
            var newFile = uri.OriginalString.Replace(filename, newFileName);
            var newBlob = blob.Container.GetBlockBlobReference(newFile);
            var exists = BlobExistsOnCloud(container, newFile);

            if (exists)
            {
                //Inform the user that the blob exists
                return false;
            }

            await newBlob.StartCopyFromBlobAsync(blob);

            return true;
        }

        /// <summary>
        /// Renames the file.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="blob">The BLOB.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        public async Task<bool> RenameFile(CloudBlobContainer container, CloudBlockBlob blob, string newFileName)
        {
            await blob.FetchAttributesAsync();
            string filename = System.IO.Path.GetFileName(blob.Name);
            var newFile = blob.Uri.OriginalString.Replace(filename, newFileName);
            var newBlob = blob.Container.GetBlockBlobReference(newFile);
            var exists = BlobExistsOnCloud(container, newFile);

            if (exists)
            {
                //Inform the user that the blob exists
                return false;
            }

            await newBlob.StartCopyFromBlobAsync(blob);
            
            await blob.DeleteAsync();
            return true;
        }

        /// <summary>
        /// Renames the directory.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        public async Task<bool> RenameDirectory(CloudBlobContainer container, string directoryName, string newName)
        {
            var fileName = String.Format("{0}/{1}", directoryName, "dummy.txt");
            var exists = BlobExistsOnCloud(container, fileName);

            if (exists)
            {
                var dirRef = container.GetDirectoryReference(directoryName);

                //Get all file references
                foreach (var blobItem in container.ListBlobs(directoryName + "/", true))
                {
                    var blob = blobItem as CloudBlockBlob;
                    var currentPath = System.IO.Path.GetDirectoryName(blobItem.Uri.AbsoluteUri);
                    var currentDirectoryName = currentPath.Substring(currentPath.LastIndexOf("\\") + 1);
                    var newDirectoryName = blobItem.Uri.AbsoluteUri.Replace(currentDirectoryName, newName);
                    newFiles.Add(newDirectoryName);
                    files.Add(blobItem.Uri.AbsoluteUri);

                    if (BlobExistsOnCloud(container, blob.Uri.AbsoluteUri))
                    {
                        var newBlob = container.GetBlockBlobReference(newDirectoryName);
                        await newBlob.StartCopyFromBlobAsync(blob);

                        if (BlobExistsOnCloud(container, newBlob.Uri.AbsoluteUri))
                        {
                            await blob.DeleteAsync();
                        }
                    }
                }

                files.Clear();
                newFiles.Clear();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves the directory.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="newDirectoryName">New name of the directory.</param>
        /// <returns></returns>
        public async Task<bool> MoveDirectory(CloudBlobContainer container, string directoryName, string newDirectoryName)
        {
            var fileName = String.Format("{0}/{1}", directoryName, "dummy.txt");
            var exists = BlobExistsOnCloud(container, fileName);

            if (exists)
            {
                var dirRef = container.GetDirectoryReference(directoryName);

                //Get all file references
                foreach (var blobItem in container.ListBlobs(directoryName + "/", true))
                {
                    var blob = blobItem as CloudBlockBlob;
                    var fname = Path.GetFileName(blob.Uri.LocalPath);
                    var newBlobPath = newDirectoryName + "/" + fname;
                    newFiles.Add(newBlobPath);
                    files.Add(blobItem.Uri.AbsoluteUri);

                    if (BlobExistsOnCloud(container, blob.Uri.AbsoluteUri))
                    {
                        var newBlob = container.GetBlockBlobReference(newBlobPath);
                        await newBlob.StartCopyFromBlobAsync(blob);

                        if (BlobExistsOnCloud(container, newBlob.Uri.AbsoluteUri))
                        {
                            await blob.DeleteAsync();
                        }
                    }
                }

                newFiles.Clear();
                files.Clear();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="file">The file.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public async Task<bool> UploadFile(CloudBlobContainer container, System.IO.MemoryStream file, string directory, string contentType, string filename)
        {
            // Create or overwrite the "myblob" blob with contents from a local file.
            using (file)
            {
                var blockBlob = container.GetBlockBlobReference(directory + filename);

                if (!BlobExistsOnCloud(container, blockBlob.Uri.AbsoluteUri))
                {
                    blockBlob.Properties.ContentType = contentType;
                    await blockBlob.UploadFromStreamAsync(file);
                    return true;
                }

                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public async Task<byte[]> DownloadFile(CloudBlobContainer container, string fileName)
        {
            var blob = container.GetBlockBlobReference(fileName);
            blob.FetchAttributes();

            if (BlobExistsOnCloud(container, blob.Uri.AbsoluteUri))
            {
                byte[] fileContent = new byte[blob.Properties.Length];
                await blob.DownloadToByteArrayAsync(fileContent, 0);

                if (fileContent.Length > 0)
                {
                    return fileContent;
                }
            }

            return null;
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="blob">The BLOB.</param>
        /// <returns></returns>
        public async Task DeleteFile(CloudBlockBlob blob)
        {
            if (blob.Exists())
            {
                await blob.DeleteAsync();
            }
        }

        /// <summary>
        /// BLOBs the exists on cloud.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool BlobExistsOnCloud(CloudBlobContainer client, string key)
        {
            return client.GetBlockBlobReference(key)
                    .Exists();
        }
    }
}

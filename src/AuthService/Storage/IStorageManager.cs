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
    /// Defines the storage management 
    /// functions.
    /// </summary>
    public interface IStorageManager
    {
        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns></returns>
        Task<bool> CreateDirectory(CloudBlobContainer container, string directoryName);

        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="blob">The BLOB.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        Task<bool> CopyFile(CloudBlobContainer container, CloudBlockBlob blob, string newFileName);

        /// <summary>
        /// Renames the file.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="blob">The BLOB.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        Task<bool> RenameFile(CloudBlobContainer container, CloudBlockBlob blob,string newFileName);

        /// <summary>
        /// Renames the directory.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        Task<bool> RenameDirectory(CloudBlobContainer container, string directoryName, string newName);

        /// <summary>
        /// Moves the directory.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="newDirectoryName">New name of the directory.</param>
        /// <returns></returns>
        Task<bool> MoveDirectory(CloudBlobContainer container, string directoryName, string newDirectoryName);

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="file">The file.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        Task<bool> UploadFile(CloudBlobContainer container, MemoryStream file, string directory, string contentType, string filename);

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        Task<byte[]> DownloadFile(CloudBlobContainer container, string fileName);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="blob">The BLOB.</param>
        /// <returns></returns>
        Task DeleteFile(CloudBlockBlob blob);

        /// <summary>
        /// BLOBs the exists on cloud.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        bool BlobExistsOnCloud(CloudBlobContainer client, string key);




    }
}

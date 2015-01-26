using AuthService.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;
using AuthService.Model;
using Microsoft.WindowsAzure;
using System.IO;
using AuthService.Logging;

namespace AuthService.Controller
{
    /// <summary>
    /// The storage client.
    /// </summary>
    public class StorageController : ApiController
    {
        private CloudStorageAccount _account;
        private CloudBlobContainer _userStorage;
        private CloudBlobClient _client;
        private List<DirectoryEntry> directory = new List<DirectoryEntry>();
        private string _userName;
        private string _containerName;
        private AzureBlobStorageManager _storageManager;
        private StorageExPorter _exporter;
        private Logger _logger;

        /// <summary>
        /// Initializes the storage.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        private async Task<string> InitStorage(string userName)
        {
                     

            await Task.Run(()=> {

            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Username cannot be null or empty.", "username");
            }

            _userName = userName.ToLowerInvariant();
            _containerName = _userName;
            _account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlobStorage"));

            //2nd create the client
            _client = _account.CreateCloudBlobClient();
            //3rd create the container
            _userStorage = _client.GetContainerReference(_userName);
            _userStorage.CreateIfNotExists();
            //4th set the permissions on the container
            _userStorage.SetPermissions(new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Off

            });

            //Create instance of Azure BlobStorageManager
            _storageManager = new Storage.AzureBlobStorageManager();

            _exporter = new StorageExPorter(userName);
            });


            await LogMessage(String.Format("Storage init: {0}", userName));
           

            return _userName;
           
        }


        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private async Task LogMessage(string message)
        {
            var logName = CloudConfigurationManager.GetSetting("LogName");
            _logger = new Logger(logName);
            await _logger.StoreNewLogMessage(new Message(message, logName));
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="filenName">Name of the filen.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile(MemoryStream fileStream,string directory,string contentType,string filenName, string userName)
        {


            await InitStorage(userName);

            var guard = new DirectoryQuota(this._userName, fileStream.Length);

            var enoughQuota = await guard.CheckUserQuota();

            if (enoughQuota != -1)
            {

                var uploadComplete = await _storageManager.UploadFile(this._userStorage, fileStream, directory, contentType, filenName);
                                
                var logName = CloudConfigurationManager.GetSetting("LogName");
                await LogMessage(String.Format("File Upload: {0} FileName: {1}", userName,filenName));

                return Request.CreateResponse<bool>(uploadComplete);
            }
            else
            {
                throw new HttpResponseException(new HttpResponseMessage() { Content=new StringContent("Not enuough storage space left"),ReasonPhrase="quota exceeded"});
            }
        }


        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> CreateDirectory(string directoryName,string userName)
        {

            await InitStorage(userName);
               
            var created = await _storageManager.CreateDirectory(this._userStorage, directoryName);

            await LogMessage(String.Format("File Upload: {0} FileName: {1}", userName, directoryName));


            return Request.CreateResponse<bool>(created);
        }

        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> CopyFile(string blobName,string newFileName,string userName)
        {

           await InitStorage(userName); 

           var blob = await _userStorage.GetBlobReferenceFromServerAsync(blobName);

           var ok = _storageManager.CopyFile(this._userStorage, (CloudBlockBlob) blob, newFileName);

           await LogMessage(String.Format("Copy File: {0} Orignial File: {1} New File {2}", userName, blobName,newFileName));


           return Request.CreateResponse<Task<bool>>(ok);
        }

        /// <summary>
        /// Renames the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> RenameFile(string fileName, string newFileName, string userName)
        {

            await InitStorage(userName);

            var blob = await _userStorage.GetBlobReferenceFromServerAsync(fileName);
            var ok = await _storageManager.RenameFile(this._userStorage, (CloudBlockBlob)blob, newFileName);

            await LogMessage(String.Format("Rename File: {0} Orignial File: {1} New File {2}", userName, fileName, newFileName));


            return Request.CreateResponse<bool>(ok);
        }

        /// <summary>
        /// Renames the directory.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> RenameDirectory(string directoryName, string newName,string userName)
        {
            await InitStorage(userName);

            var ok = await _storageManager.RenameDirectory(this._userStorage, directoryName, newName);

            await LogMessage(String.Format("Rename Directory: {0} Orignial Directory: {1} New Directory {2}", userName, directoryName, newName));

            return Request.CreateResponse<bool>(ok);
        }

        /// <summary>
        /// Moves the directory.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="newDirectoryName">New name of the directory.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> MoveDirectory(string directoryName, string newDirectoryName,string userName)
        {
            await InitStorage(userName);
            var ok = await _storageManager.MoveDirectory(this._userStorage, directoryName, newDirectoryName);

            await LogMessage(String.Format("Move Directory: {0} Orignial Directory: {1} New Directory {2}", userName, directoryName, newDirectoryName));

            return Request.CreateResponse<bool>(ok);
        }


        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> DownloadFile(string fileName,string userName)
        {
            await InitStorage(userName);
            var data = await _storageManager.DownloadFile(this._userStorage, fileName);
            await LogMessage(String.Format("Download File: {0} File Name: {1}", userName, fileName));
            return Request.CreateResponse<byte[]>(data);
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> DeleteFile(string fileName, string userName)
        {
            await InitStorage(userName);
            var blob = await _userStorage.GetBlobReferenceFromServerAsync(fileName);
            await _storageManager.DeleteFile((CloudBlockBlob)blob);
            await LogMessage(String.Format("Delete File: {0} Username:{1}",fileName,userName));
            return Request.CreateResponse<string>(fileName);
        }

        /// <summary>
        /// BLOBs the exists on cloud.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> BlobExistsOnCloud(string key,string userName)
        {

            await InitStorage(userName);

          var value =  await Task.Run(() =>
            {
                var ok = _storageManager.BlobExistsOnCloud(this._userStorage, key);
                return ok;
            });

          return Request.CreateResponse<bool>(value);
        }

        /// <summary>
        /// Jsons the directory data.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> JsonDirectoryData(string userName)
        {

            await InitStorage(userName);

            await LogMessage(String.Format("Export JSON, username: {0} ",userName));

            var json = await _exporter.Storage2JSON();

            return Request.CreateResponse<string>(json);
        }
    }
}

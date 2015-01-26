using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AuthService.Storage
{
    /// <summary>
    /// Class to check user directory 
    /// quota.
    /// </summary>
    class DirectoryQuota:IDirectoryQuota
    {
        private string _dbConnectionString;
        private CloudStorageAccount _account;
        private CloudBlobContainer _userStorage;
        private CloudBlobClient _client;
        private string _userName;
        private long _currentUserQuota;
        private long _currentFileSize;
    

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryQuota"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <exception cref="System.ArgumentException">Username cannot be null or empty.;username</exception>
        public DirectoryQuota(string userName, long currentFileSize)
        {

            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Username cannot be null or empty.", "username");
            }

            this._userName = userName;
            this._dbConnectionString = CloudConfigurationManager.GetSetting("Database");
            this._account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlobStorage"));
            this._client = _account.CreateCloudBlobClient();
            this._userStorage = _client.GetContainerReference(this._userName);
            this._currentUserQuota = 0;
            this._currentFileSize = currentFileSize;
            

        }


        /// <summary>
        /// Gets the current user quota.
        /// </summary>
        /// <returns></returns>
        private async Task GetCurrentUserQuota()
        {

            var blobList = this._userStorage.ListBlobs(null, true);

            foreach(var entry in blobList)
            {
                var blob = (CloudBlockBlob)entry;

                await blob.FetchAttributesAsync();

                this._currentUserQuota += blob.Properties.Length;
            }

            
        }


        /// <summary>
        /// Checks the user quota.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="currentDirectorySize">Size of the current directory.</param>
        /// <param name="currentFileSize">Size of the current file.</param>
        /// <returns></returns>
        public async Task<long> CheckUserQuota()
        {

            long quota = 0;
            object queryValue = 0;

            await this.GetCurrentUserQuota();

            using(var connection = new SqlConnection(this._dbConnectionString))
            {

                string queryTemplate = "SELECT StorageQuota " +
                                " FROM Subscription AS s " +
                                " INNER JOIN UserSubscriptions AS us " +
                                " ON s.id = us.subscriptionid " +
                                " INNER JOIN UserProfile ON " +
                                " userprofile.userid = us.userid " +
                                " where userprofile.username = '@username' ";


                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = queryTemplate;
                cmd.Parameters.Add(new SqlParameter("@username", this._userName));

                await connection.OpenAsync();
               
                if(connection.State == System.Data.ConnectionState.Open)
                {
                    queryValue  = await cmd.ExecuteScalarAsync();
                }

                quota = (long)queryValue;

                connection.Close();

                if(quota > (this._currentUserQuota+this._currentFileSize))
                {
                    return quota - this._currentUserQuota;
                }

                if (quota < (this._currentUserQuota + this._currentFileSize))
                {
                    return -1;
                }
               

            }

            return 0;
        }
    }
}

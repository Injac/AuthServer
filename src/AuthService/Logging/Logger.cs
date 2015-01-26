using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Logging
{
    /// <summary>
    /// Log a message to table storage.
    /// </summary>
    public class Logger
    {

        private const string MessagePartionKey = "LogEntry";
        private const string DateFormat = "yyyyMMdd ; HH:mm:ss:fffffff";
        private const string RowKeyFormat = "{0} - {1}";
        private string _logName;
        private CloudStorageAccount _account;


        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="logName">Name of the log.</param>
        /// <exception cref="System.ArgumentException">Logname cannot be null.</exception>
        public Logger(string logName)
        {
            if (String.IsNullOrEmpty(logName))
            {
                throw new ArgumentException("Logname cannot be null.", logName);
            }

            _logName = logName;
            _account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlobStorage"));
        }


        /// <summary>
        /// Stores the new log message.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <returns></returns>
        public async Task StoreNewLogMessage(Message logMessage)
        {

            CloudTableClient tableClient = _account.CreateCloudTableClient();
            var table = await tableClient.GetTableReference(this._logName).CreateIfNotExistsAsync();
            TableServiceContext tableContext = tableClient.GetTableServiceContext();
            tableContext.AddObject(this._logName, logMessage);
            await tableContext.SaveChangesWithRetriesAsync();

        }
    }
}

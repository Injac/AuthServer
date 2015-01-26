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
    /// Used to log everything that
    /// is happening with the file 
    /// service.
    /// </summary>
    public class Message:TableServiceEntity
    {
        private const string MessagePartionKey = "LogEntry";
        private const string DateFormat = "yyyyMMdd ; HH:mm:ss:fffffff";
        private const string RowKeyFormat = "{0} - {1}";
        private string _logName;
      
        public string LogMessage { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="logName">Name of the log.</param>
        /// <exception cref="System.ArgumentException">
        /// Logname cannot be null.;logName
        /// or
        /// LogMessage cannot be null.;logMesage
        /// </exception>
        public Message(string logMessage, string logName)
        {
            if (String.IsNullOrEmpty(logName))
            {
                throw new ArgumentException("Logname cannot be null.", "logName");
            }

            if (String.IsNullOrEmpty(logMessage))
            {
                throw new ArgumentException("LogMessage cannot be null.", "logMesage");
            }

            _logName = logName;
            PartitionKey = MessagePartionKey;
            string date = DateTime.Now.ToUniversalTime().ToString(DateFormat);
            RowKey = string.Format(RowKeyFormat, date, Guid.NewGuid().ToString());
            LogMessage = logMessage;
        }

    }

   
}

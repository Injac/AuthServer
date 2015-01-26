using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Model
{
    /// <summary>
    /// Representing a directory entry
    /// that is a file, containing the
    /// whole blob path.
    /// </summary>
   public class DirectoryEntry
    {

        /// <summary>
        /// Gets or sets the c file.
        /// </summary>
        /// <value>
        /// The c file.
        /// </value>
        public CloudFile CFile { get; set; }
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }
    }

   /// <summary>
   /// A cloud directory, that is
   /// build on extracting the blob path.
   /// </summary>
   public class CloudDirectory
   {
       /// <summary>
       /// Gets or sets the name.
       /// </summary>
       /// <value>
       /// The name.
       /// </value>
       public string Name { get; set; }

       /// <summary>
       /// Gets or sets a value indicating whether [is root].
       /// </summary>
       /// <value>
       ///   <c>true</c> if [is root]; otherwise, <c>false</c>.
       /// </value>
       public bool isRoot { get; set; }

       /// <summary>
       /// Gets or sets the parent.
       /// </summary>
       /// <value>
       /// The parent.
       /// </value>
       public string Parent { get; set; }

       /// <summary>
       /// Gets or sets the sub directories.
       /// </summary>
       /// <value>
       /// The sub directories.
       /// </value>
       public List<CloudDirectory> SubDirectories { get; set; }

       /// <summary>
       /// Gets or sets the cloud files.
       /// </summary>
       /// <value>
       /// The cloud files.
       /// </value>
       public List<CloudFile> CloudFiles { get; set; }


       /// <summary>
       /// Gets or sets the directory size in bytes.
       /// </summary>
       /// <value>
       /// The directory size in bytes.
       /// </value>
       public long DirectorySizeInBytes { get; set; }

       /// <summary>
       /// Gets or sets the directory size human readable.
       /// </summary>
       /// <value>
       /// The directory size human readable.
       /// </value>
       public string DirectorySizeHumanReadable { get; set; }

       /// <summary>
       /// Initializes a new instance of the <see cref="CloudDirectory"/> class.
       /// </summary>
       public CloudDirectory()
       {
           this.CloudFiles = new List<CloudFile>();
           this.SubDirectories = new List<CloudDirectory>();
       }
   }


   /// <summary>
   /// This is the user directory that
   /// will hold the username and will restrict
   /// the user to only browsing this part of the
   /// directory.
   /// </summary>
   [Newtonsoft.Json.JsonObject(Title = "UserDirectory")]
   public class UserDirectory
   {
       /// <summary>
       /// Gets or sets the name of the user.
       /// </summary>
       /// <value>
       /// The name of the user.
       /// </value>
       public string UserName { get; set; }

       /// <summary>
       /// Gets or sets the content of the storage.
       /// </summary>
       /// <value>
       /// The content of the storage.
       /// </value>
       public CloudDirectory StorageContent { get; set; }
   }

   /// <summary>
   /// Representing a file on Azure.
   /// </summary>
   public class CloudFile
   {
       /// <summary>
       /// Gets or sets the name of the file.
       /// </summary>
       /// <value>
       /// The name of the file.
       /// </value>
       public string FileName { get; set; }

       /// <summary>
       /// Gets or sets the size of the file.
       /// </summary>
       /// <value>
       /// The size of the file.
       /// </value>
       public long FileSize { get; set; }

       /// <summary>
       /// Gets or sets the type of the content.
       /// </summary>
       /// <value>
       /// The type of the content.
       /// </value>
       public string ContentType { get; set; }
   }


   public class Wrapper
   {
       public CloudDirectory UserDirectory { get; set; }
       public string Username { get; set; }
   }
}

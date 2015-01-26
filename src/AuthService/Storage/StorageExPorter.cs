using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Model;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;

namespace AuthService.Storage
{
    /// <summary>
    /// Exports the current storage directories
    /// and files into several formats like JSON.
    /// </summary>
    public class StorageExPorter
    {
        private CloudStorageAccount _account;
        private CloudBlobContainer _userStorage;
        private CloudBlobClient _client;
        private List<DirectoryEntry> directory = new List<DirectoryEntry>();
        private string _userName;
        private string _containerName;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageExPorter"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <exception cref="System.ArgumentException">Username cannot be null or empty.;username</exception>
        public StorageExPorter(string userName)
        {
            if(String.IsNullOrEmpty(userName))
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

        }


        /// <summary>
        /// Storage2s the json.
        /// </summary>
        /// <returns></returns>
        public async Task<string> Storage2JSON()
        {
               
            long dirSize = 0;
            //This is how it works with a container
            foreach (var blobItem in _client.ListBlobs(String.Format("{0}/",_userName), true))
            {
                var reference = blobItem.Container.GetBlockBlobReference(blobItem.Uri.AbsoluteUri);
             
                await reference.FetchAttributesAsync();
                dirSize += reference.Properties.Length;
             
                var realPath = reference.Uri.LocalPath;
                var temp = realPath.Substring(1);
                var temp2 = realPath.Substring(temp.IndexOf('/') + 1);

                directory.Add(new DirectoryEntry()
                {
                    CFile = new CloudFile()
                    {
                        ContentType = reference.Properties.ContentType,
                        FileSize = reference.Properties.Length
                    }
                    ,
                    Path = temp2
                });
              
            }
         
            CloudDirectory root = new CloudDirectory();
            root.isRoot = true;
            CloudDirectory lastDir = null;
            root.Name = _userStorage.Name;
            string lastPath = "";
            
            foreach (var fileitem in directory)
            {

                string fileName = "";
                var items = fileitem.Path.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                if (items.Length == 2)
                {
                    //File is on root level
                    var itemsRoot = items.ToList().GetRange(1, items.Length - 1);
                    fileitem.CFile.FileName = itemsRoot[0];
                    root.CloudFiles.Add(fileitem.CFile);
                    continue;
                }

                var itemsWithoutRootAndFile = items.ToList().GetRange(1, items.Length - 2);
                
                var pathstring = String.Join("/", itemsWithoutRootAndFile);

                fileName = items.Last();

                if (!lastPath.Equals(pathstring))
                {
                    lastPath = pathstring;
                    
                    bool addedToRoot = false;

                    foreach (var dir in itemsWithoutRootAndFile)
                    {

                        if (!addedToRoot)
                        {
                            lastDir = new CloudDirectory() { Name = dir };
                            var inRootDir = root.SubDirectories.Where(rs => rs.Name.Equals(dir)).FirstOrDefault();
                            if (inRootDir == null)
                            {
                                lastDir.Parent = root.Name;
                                root.SubDirectories.Add(lastDir);
                            }
                            else
                            {
                                lastDir = root.SubDirectories.Where(s => s.Name.Equals(dir)).FirstOrDefault();
                            }
                            addedToRoot = true;
                        }
                        else
                        {

                            var nextDir = new CloudDirectory() { Name = dir };
                            var inSubDir = lastDir.SubDirectories.Where(ls => ls.Name.Equals(dir)).FirstOrDefault();
                            if (inSubDir == null)
                            {
                                nextDir.Parent = lastDir.Name;
                                lastDir.SubDirectories.Add(nextDir);
                            }
                            else
                            {
                                nextDir = lastDir.SubDirectories.Where(sub => sub.Name.Equals(dir)).FirstOrDefault();
                            }

                           
                            lastDir = nextDir;

                        }
                    }
                }

                fileitem.CFile.FileName = fileName;
                lastDir.CloudFiles.Add(fileitem.CFile);
                lastPath = pathstring;
            }

            UserDirectory userdir = new UserDirectory();
            userdir.UserName = _userName;
            userdir.StorageContent = root;


            root.DirectorySizeInBytes = dirSize;
            root.DirectorySizeHumanReadable = FileHelper.StrFormatByteSize(dirSize);

           

            Wrapper wp = new Wrapper();

            wp.UserDirectory = root;
            wp.Username = _userName;
            
            var ser = JsonConvert.SerializeObject(wp);

            this.directory.Clear();
            
            return ser;
        }

    }
}

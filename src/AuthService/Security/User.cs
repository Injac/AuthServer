using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Security
{
    [DataContract]
    public class User
    {
        
        /// <summary>
        /// The ip address of the 
        /// current user logged in.
        /// </summary>
        private string _ip;
        
        /// <summary>
        /// The _status
        /// </summary>
        private string _status;
               
        /// <summary>
        /// The _user name
        /// </summary>
        private string _userName;

        /// <summary>
        /// The _token
        /// </summary>
        private string _token;

        /// <summary>
        /// The secret
        /// </summary>
        private string _secret;


        /// <summary>
        /// The _sec soup
        /// </summary>
        private string _secSoup;

        /// <summary>
        /// Gets or sets the sec soup.
        /// </summary>
        /// <value>
        /// The sec soup.
        /// </value>
        [DataMember]
        public string SecSoup
        {
            get { return _secSoup; }
            set { _secSoup = value; }
        }


        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        [DataMember]
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
            }
        }
        /// <summary>
        /// The ip address of the
        /// currently logged in client.
        /// </summary>
        [DataMember]
        public string Ip
        {
            get { return this._ip; }
            set { this._ip = value; }
        }

        /// <summary>
        /// The secret.
        /// </summary>
        [DataMember]
        public string Secret
        {
            get { return this._secret; }
            set { this._secret = value; }
        }
        

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [DataMember]
        public string Token
        {
            get
            {
                return _token;
            }
            set
            {
                _token = value;
            }
        }
    }
}

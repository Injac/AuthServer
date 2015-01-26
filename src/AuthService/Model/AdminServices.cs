using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Model
{
    /// <summary>
    /// The services that can be called only with
    /// administrative permissions.
    /// </summary>
    internal class AdminServices
    {

        /// <summary>
        /// Gets or sets the admin service identifiers.
        /// </summary>
        /// <value>
        /// The admin service identifiers.
        /// </value>
        public List<string> AdminServiceIdentifiers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminServices"/> class.
        /// </summary>
        public AdminServices()
        {
            this.AdminServiceIdentifiers = new List<string>();
            this.InitAdminServices();
        }

        /// <summary>
        /// Initializes the admin services.
        /// </summary>
        private void InitAdminServices()
        {
            this.AdminServiceIdentifiers.Add("AddSystemAppUser");
            this.AdminServiceIdentifiers.Add("DeleteSystemAppUser");
            this.AdminServiceIdentifiers.Add("DeleteSystemApp");
            this.AdminServiceIdentifiers.Add("CreateSystemApp");
            this.AdminServiceIdentifiers.Add("ListAllSystemApps");
            this.AdminServiceIdentifiers.Add("ListSystemApp");
          
        }

    }
}

# AuthServer

This solution is part of my last startup-project to market Windows and Windows Phone apps.

It represents the Authentication-Server that was used as a kind of Guardian between the services and the callers that needed authentication to access the API's.

The service was planned as a scalable service from the beginning. I have chosen an hosted Web API that runs within an Azure Worker Role. This will give you the most flexible approach possible today to scale the server based on your needs.

Available Features:

* Can Issue API keys and tokens
* Access to AUTH-Server and API based on Token, Secret and Domain or IP
* Seperates system API from user API
* Request Throttling based on WebApiThrottle
* The system can issue OTP's (One Time Codes)

## How to make it work

Basic settings have to be made within the settings dialog (Properties) of the AuthService role located within the ExGripAuthServer project.

![config](https://cloud.githubusercontent.com/assets/1821384/5904796/8304d8d8-a58b-11e4-9afc-abee8ccef008.png)

All of these values are simply hardcoded values and need to be present:


1. **Blob Storage:** Add you storage connection string here.
2. **Database:** Add a valid Azure database connection string here.
3. **LogName:** This will be the name of the log. Log entries are written to table storage, hence the storage account
4. **ManagementSecret:** String that defines the base managment secret. Use a password-genrator to generate this secret.
5. **MGMToken:** The managment token. Generate this using a password generator as well.
6. **MGMSecret:** This is the secondary secret. Again, use a password manager to generate a solid secret.
7. **AzureCacheKey:** Azure Cache was used to store banned IP's. I would not recommend to use that further. Check the AzureCacheHelper directory within the solution and replace this with what you need.

## Database

Within the **Model** folder you will find 2 Entity Framework models (Model First):

* **UserApps.edmx** - this is the model you need. Generate your database from that one. This is the database you will need as mentioned in section "How to make it work"
* **SystemModel.edmx** - Connection to the user-backend

### Quick overview of the tables used

The database-model is fairly simple. As you can see, there are no hard-coded relations between the tables within the model. Everything is done purely using EF.

![dbmodel](https://cloud.githubusercontent.com/assets/1821384/5906488/485a42e2-a598-11e4-9b05-454266ed5f9f.png)

* Table **app**: Table for the user-based apps
* Table **appuser**: Those are *external* users. Those users have access to your API.
* Table **user**: This is the main user table. 
* Table **OTPUser**: This table holds the OTP (One Time Token) data for each user (if used)
* Table **promotioncode**: This table holds the promotional-codes for each user
* Table **systemapp**: This are apps (API's) by system-users
* Table **systemappuser**: This are users who have access to system-apps (system API's)
* Table **managementkey**: Holds managment-keys. This table is not used

Yes, some tables don't belong there, but I have put them there for security reasons. I have installed different tables in different databases to devide them in general from each other.



## Important classes and files

It's important to know, which class is doing what to configure the server in more detail. This section describes the various classes and their responsibilities.

### Security Related

Let's start with the security related ones:

* **/Filters/ApiAuthFilter.cs** - Checks general for services that want to secure their API 
* **/Filters/IPAddressFilter.cs** - Allows access to this Auth-Server only from specific IP-Addresses
* **/Handler/AuthHandler.cs** -  This is where user/system API keys and secrets are checked
* **/Model/AuthorizedIpRepository.cs** - Put the authorized system-ip's in this repo
* **/Model/AdminServices.cs** - Controller actions that can only be called with system API access
* **/Security/Security.cs** - Secure Hash creation and password validation
* **/Security/User.cs** - User related security data
* **/Security/UserAppAuthenticationManager.cs** - Token creation, validation, encryption, de-cryption, based on SSL-Certificates (use the existing certificate & private key only for testing! Generate your own!)

### Helpers

* **/Helpers/DateHelper.cs** - Maps from TZDB (aka IANA, Olson, or zoneinfo) to Windows based time-zone id's
* **/Helpers/WordGenerator.cs** - Generates simple random words of a specific lenght

### Logging

* **/Logging/Logger.cs** - Logger implementation that logs to Azure Tables
* **/Logging/Message.cs** - TableServiceEntity that represents a log-entry

## Storage

The solution contains a complete system to manage storage files like directories using JSON:

* **/Storage/AzureBlobStorageManager.cs** - Manages all the CRUD related task to manage Azure Storage Blobs
* **/Storage/DirectoryQuota.cs** - Allows you to set a maximum of X MB, GB or whatever size you use
* **/Storage/FileHelper.cs** - Allows you to get the file-size in bytes as string using P/Invoke
* **/Storage/StorageExporter.cs** - Returns the directory-like structure (JSON) for the blobs per user 

## Controllers

* **/Controllers/AppManagementController.cs** - Provides the API to manage user/system apps and system-users and API users
* **/Controllers/OtpController.cs** - Issues and de-activates OTP's (One Time Codes)
* **/Controllers/PromotionCodesController.cs** - Promotional Codes managment
* **/Controller/StorageController.cs** - Controller that manages the storage part mentioned in section "Storage"


(still working on the documentation)




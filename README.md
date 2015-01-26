# ExGrip-AuthServer

This project is part of my last startup-solution to market Windows and Windows Phone apps.

It represents the Authentication-Server that was used as a kind of Guardian between the services and the callers that needed authentication to access the API's.

The service was planned as a scalable service from the beginning. I have chosen an hosted Web API that runs within an Azure Worker Role. This will give you the most flexible approach possible today to scale the server based on your needs.

Available Features:

* Can Issue API keys and tokens
* Access to AUTH-Server and API based on Token, Secret and Domain or IP
* Seperates system API from user API
* Request Throttling based on WebApiThrottle

## How to make it work

Basic settings have to be made within the settings dialog (Properties) of the AuthService role located within the ExGripAuthServer project.





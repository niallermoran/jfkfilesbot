# jfkfilesbot
A BOT built for the Dublin MTC using the Azure BOT framework that demonstrates cognitive services and accompanies the JFK Files demo

#Setup

## Create a LUIS Application

1. Go to luis.ai and click "import new app" from your dashboard. Importa the file FK Files BOT.json
2. Click the Train button on the top right hand corner.
3. Go to the publish tab and publish the app.
4. From the publish screen copy the key string, this is the subscription_key needed later
5. From the settings screen for the LUIS app copy the application ID for later

## Prepare the BOT Application

1. Open the Visual Studio solution file jfkfilesbot.sln.
2. Create a new file called web.secret.config in the same folder as web.config
3. Create a new file called IntentDialog.secrets.cs in the same folder as IntentDialog.cs
4. Add the following code to web.secrets.config
```c#
<?xml version="1.0"?>
  <appSettings>
    <!-- update these with your Microsoft App Id and your Microsoft App Password from Azure bot settings-->
    <add key="AuthenticationConnectionName" value=""/>
    <add key="BingSearchKey" value=""/>
    <add key="MicrosoftAppId" value=""/>
    <add key="MicrosoftAppPassword" value=""/>
    <add key="AzureWebJobsStorage" value=""/>
    <add key="AzureWebJobsDashboard" value=""/>
  </appSettings>
```

# jfkfilesbot
A BOT built for the Dublin MTC using the Azure BOT framework that demonstrates cognitive services and accompanies the JFK Files demo

# Setup

## Create a LUIS Application

1. Go to luis.ai and click "import new app" from your dashboard. Import the file 'JFK Files BOT.json'
2. Click the Train button on the top right hand corner.
3. Go to the publish tab and publish the app.
4. From the publish screen copy the key string, this is the subscription_key needed later
5. From the settings screen for the LUIS app copy the application ID for later

## Prepare the BOT Application

1. Open the Visual Studio solution file jfkfilesbot.sln.
2. Create a new file called web.secret.config in the same folder as web.config
3. Create a new file called IntentDialog.secrets.cs in the same folder as IntentDialog.cs
4. Add the following code to web.secrets.config replacing the settings as described
```xml
<?xml version="1.0"?>
  <appSettings>
    <!-- update these with your Microsoft App Id and your Microsoft App Password from Azure bot settings-->
    <add key="AuthenticationConnectionName" value="[not used for this release]"/>
    <add key="BingSearchKey" value="[create a bing search resource in Azure and get the key from the keys section, make sure the pricing tier include web search]"/>
    <add key="MicrosoftAppId" value="[create a web bot in Azure and from settings select the App ID]"/>
    <add key="MicrosoftAppPassword" value="[in Azure settings for the BOT resource beside the App ID, from last setting, click the manage link and generate a new password]"/>
    <add key="AzureWebJobsStorage" value="[for the storage account in Azure created with the BOT enter the connection string from the access keys section]"/>
    <add key="AzureWebJobsDashboard" value="[same as above]"/>
	<add key="VisionAPIKey" value="[create a vision API service in Azure and grab the key]"/>
    <add key="VisionAPIRegion" value="[the region associated with your Vision service e.g. northeurope]"/>
  </appSettings>
```
5. Open the file IntentDialog.secrets.cs and use the following code, replacing the IDs as appropriate
```c#
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;


namespace jfkfiles.bot
{
    /// <summary>
    /// The top-level natural language dialog for sample.
    /// </summary>
    [LuisModel("[replace with application ID recorded above]", "[replace with the subscription iID recorded above]")]
    internal sealed partial class IntentDialog : LuisDialog<object>
    {
      
    }
}
```
6. Build and run the application in Visual Studio. Take not of the URL that launches, e.g. http://localhost:xxxx/

## Test the BOT
1. Download the [BOT emulator](https://docs.microsoft.com/en-gb/azure/bot-service/bot-service-debug-emulator?view=azure-bot-service-3.0)
2. Run the BOT emulator and create a new BOT configuration
3. Enter a name and set the URL to http://localhost:xxxx/api/messages, using the correct port number from step 1
4. Leave APP ID and Password blank 

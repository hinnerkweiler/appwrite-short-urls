using System.Xml.XPath;

namespace DotNetRuntime;

using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using Appwrite.Extensions;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Collections;
using System.Net;

public class Handler 
{
    private string projectId = Environment.GetEnvironmentVariable("APPWRITE_FUNCTION_PROJECT_ID");
    private string apiKey = Environment.GetEnvironmentVariable("APPWRITE_API_KEY");
    private string collectionId = Environment.GetEnvironmentVariable("APPWRITE_COLLECTION_ID");
    private string databaseId = Environment.GetEnvironmentVariable("APPWRITE_DATABASE_ID");

    // This is your Appwrite function
    // It is executed each time we get a request
    public async Task<RuntimeOutput> Main(DotNetRuntime.RuntimeContext Context) 
    {
        // You can log messages to the console
        // if environment variable LOG_REQUESTS is set to true logs the request details
        if (Environment.GetEnvironmentVariable("LOG_REQUESTS") == "true") {
            Context.Log("---> at " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));  // Current time in UTC
            Context.Log(Context.Req.BodyRaw);                                                   // Raw request body, contains request data
          }

        switch (Context.Req.Method) {   
            case "GET":
                return Context.Res.Redirect(await Get(Context),302);
            default:
                return Context.Res.Json(new Dictionary<string, object>()
                    {
                        { "message", "This is not the endpoint you were looking for!" },
                        { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") }
                    });
        }
    }
    private async Task<string> Get(DotNetRuntime.RuntimeContext Context)
    {
        Uri result = new Uri(Environment.GetEnvironmentVariable("APPWRITE_DEFAULTURL"));

        // get the url from from url parameter "url"
        string slug = Context.Req.Query["slug"].ToString();
        Context.Log("slug: " + slug);
        Context.Log("projectId: " + projectId);
        Context.Log("endpoint: " + Environment.GetEnvironmentVariable("APPWRITE_ENDPOINT"));

        // query the appwrite collection for the url and return the attribute "destination" if found
        // if not found return the default url
        try
        {
            Client client = new Client();
            client
                .SetEndpoint(Environment.GetEnvironmentVariable("APPWRITE_ENDPOINT"))
                .SetProject(projectId)
                .SetKey(apiKey)
            ;

            var UrlList = new List<Models.UrlDocument>();

            Databases databases = new Databases(client);
            Context.Log("database connected");
            var documentList = await databases.ListDocuments(collectionId: collectionId, databaseId: databaseId, queries: new List<string> { $"slug={slug}"});
            //get the first element from documentList and return it as string
            if (documentList.Documents.Count > 0)
            {
                 Context.Log("destination 1 of " + documentList.Documents.Count );
                var firstDocument = documentList.Documents.First();
                return firstDocument.Data.GetValueOrDefault("destination").ToString();
                
            }
            else
            {
                return Environment.GetEnvironmentVariable("APPWRITE_DEFAULTURL");
            }
        }
        catch (Exception e)
        {
            Context.Log(e.Message);
        }

        return Environment.GetEnvironmentVariable("APPWRITE_DEFAULTURL");
    }
}
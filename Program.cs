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
using System.ComponentModel;

public class Handler 
{
    private string projectId = Environment.GetEnvironmentVariable("APPWRITE_FUNCTION_PROJECT_ID");
    private string endpoint = Environment.GetEnvironmentVariable("APPWRITE_ENDPOINT");
    private string apiKey = Environment.GetEnvironmentVariable("APPWRITE_API_KEY");
    private string collectionId = Environment.GetEnvironmentVariable("APPWRITE_COLLECTION_ID");
    private string databaseId = Environment.GetEnvironmentVariable("APPWRITE_DATABASE_ID");
    private bool log = Environment.GetEnvironmentVariable("LOG_REQUESTS") == "true" ? true : false;
    private string defaultUrl = Environment.GetEnvironmentVariable("APPWRITE_DEFAULTURL");

    private int resultstatus = 200;

    // This is your Appwrite function
    // It is executed each time we get a request
    public async Task<RuntimeOutput> Main(DotNetRuntime.RuntimeContext Context) 
    {
        // You can log messages to the console
        // if environment variable LOG_REQUESTS is set to true logs the request details
        if (log) {
            Context.Log("---> at " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));  // Current time in UTC
            Context.Log(Context.Req.BodyRaw);                                                   // Raw request body, contains request data
          }

        switch (Context.Req.Method) {   
            case "GET":
                return Context.Res.Redirect(await Get(Context),resultstatus);
            case "POST":
                return Context.Res.Send(await Post(Context),resultstatus);
            default:
                return Context.Res.Json(new Dictionary<string, object>()
                    {
                        { "message", "This is not the endpoint you were looking for!" },
                        { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") }
                    });
        }
    }

    private async Task<string> Post(DotNetRuntime.RuntimeContext context)
    {
        // take the "slug" and "destination" url from the request body and store it in the appwrite collection specified by the environment variables
        // if the slug already exists return an error
        // if the slug does not exist create a new document with the slug and destination url
        // return the new document as json
        try
        {
            Client client = new Client();
            client
                .SetEndpoint(endpoint)
                .SetProject(projectId)
                .SetKey(apiKey)
                .SetSelfSigned(true)
            ;

            // extract the slug and destination from the request body
            var body = context.Req.BodyRaw;
            JObject bodyjson = JObject.Parse(body);
            if (log) context.Log("body: " + bodyjson.ToString());
            string slug = bodyjson["slug"].ToString();
            string destination = bodyjson["destination"].ToString();

            if (log) 
            {
                context.Log("create slug: " + slug);
                context.Log("for destination: " + destination);
            }
            
            Databases databases = new Databases(client);
            if (log) context.Log("database connected");
            

            // check if the slug already exists
            var documentList = await databases.ListDocuments(collectionId: collectionId, databaseId: databaseId, queries: new List<string> {Query.Equal("slug", slug)} );
            if (log) context.Log("documents: " + documentList.Documents.Count);
            if (documentList.Documents.Count > 0)
            {
                // if the slug already exists return an error
                resultstatus = 400;
                return "slug already exists";
            }
            else
            {
                // if the slug does not exist create a new document with the slug and destination url
                var document = await databases.CreateDocument(collectionId: collectionId, databaseId: databaseId, documentId: slug, data: bodyjson);
                if (log) context.Log("document created");
                if (log) context.Log("document id: " + document.Id);
                if (log) context.Log("document data: " + document.Data);
                if (document.Id == null)
                {
                    resultstatus = 500;
                    return "error creating document";
                }
                resultstatus = 201;
                return document.Data.ToString();
            }
        }
        catch (Exception e)
        {
            resultstatus = 500;
            context.Log(e.Message);
        }
        return "error";
    }
    private async Task<string> Get(DotNetRuntime.RuntimeContext Context)
    {
        Uri result = new Uri(defaultUrl);

        // get the url from from url parameter "url"
        string slug = Context.Req.Query["s"].ToString() ?? "hwde";
        Context.Log("slug: " + slug);
        Context.Log("projectId: " + projectId);
        Context.Log("endpoint: " + endpoint);

        // query the appwrite collection for the url and return the attribute "destination" if found
        // if not found return the default url
        try
        {
            Client client = new Client();
            client
                .SetEndpoint(endpoint)
                .SetProject(projectId)
                .SetKey(apiKey)
                .SetSelfSigned(true)
            ;

            var UrlList = new List<Models.UrlDocument>();

            Databases databases = new Databases(client);
            if (log) Context.Log("database connected");
            var documentList = await databases.ListDocuments(
                collectionId: collectionId, 
                databaseId: databaseId,
                queries: new List<string> {Query.Equal("slug", slug)}
            );
            if (log) Context.Log("documents: " + documentList.Documents.Count);


            //get the first element from documentList and return it as string queries: new List<string> { $"slug={slug}"}
            if (documentList.Documents.Count > 0)
            {
                if (log) Context.Log("destination 1 of " + documentList.Documents.Count );
                var firstDocument = documentList.Documents.First();
                return firstDocument.Data.GetValueOrDefault("destination").ToString();
            }
            else
            {
                return defaultUrl;
            }
        }
        catch (Exception e)
        {
            resultstatus = 500;
            Context.Log(e.Message);
        }

        return defaultUrl;
    }
}
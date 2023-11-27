using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Syncfusion.EJ2.PdfViewer;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "MyPolicy";
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    // Use the default property (Pascal) casing
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                        builder => {
                            builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                        });
});

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
builder.Services.AddResponseCompression();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.UseResponseCompression();
app.MapControllers();
app.MapGet("/PdfViewer", () =>
{
    return new string[] { "value1", "value2" };

});

IMemoryCache _cache;
IWebHostEnvironment _hostingEnvironment;

var scope = app.Services.CreateScope();
_cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
_hostingEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();


app.MapPost("pdfviewer/Load", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    MemoryStream stream = new MemoryStream();
    object jsonResult = new object();
    if (jsonObject != null && jsonObject.ContainsKey("document"))
    {

        if (bool.Parse(jsonObject["isFileName"]))
        {
            string documentPath = GetDocumentPath(jsonObject["document"]);
            if (!string.IsNullOrEmpty(documentPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                stream = new MemoryStream(bytes);
            }
            else
            {
                string fileName = jsonObject["document"].Split("://")[0];
                if (fileName == "http" || fileName == "https")
                {
                    WebClient webclient = new WebClient();
                    byte[] pdfDoc = webclient.DownloadData(jsonObject["document"]);
                    stream = new MemoryStream(pdfDoc);
                }
                else
                {
                    return Results.Ok(jsonObject["document"] + " is not found");
                }
            }
        }
        else
        {
            byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
            stream = new MemoryStream(bytes);
        }
    }
    jsonResult = pdfviewer.Load(stream, jsonObject);
    return Results.Ok(JsonConvert.SerializeObject(jsonResult));
});

app.MapPost("pdfviewer/RenderPdfPages", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    //Initialize the PDF Viewer object with memory cache object
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    object jsonResult = pdfviewer.GetPage(jsonObject);
    return Results.Ok(JsonConvert.SerializeObject(jsonResult));
});

app.MapPost("pdfviewer/Bookmarks", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    //Initialize the PDF Viewer object with memory cache object
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    var jsonResult = pdfviewer.GetBookmarks(jsonObject);
    return Results.Ok(JsonConvert.SerializeObject(jsonResult));
});
app.MapPost("pdfviewer/RenderThumbnailImages", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    //Initialize the PDF Viewer object with memory cache object
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    object result = pdfviewer.GetThumbnailImages(jsonObject);
    return Results.Ok(JsonConvert.SerializeObject(result));
});
app.MapPost("pdfviewer/RenderAnnotationComments", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    //Initialize the PDF Viewer object with memory cache object
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
    return Results.Ok(JsonConvert.SerializeObject(jsonResult));
});
app.MapPost("pdfviewer/ExportAnnotations", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    string jsonResult = pdfviewer.ExportAnnotation(jsonObject);
    return jsonResult;
});
app.MapPost("pdfviewer/ImportAnnotations", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    string jsonResult = string.Empty;
    object JsonResult;
    if (jsonObject != null && jsonObject.ContainsKey("fileName"))
    {
        string documentPath = GetDocumentPath(jsonObject["fileName"]);
        if (!string.IsNullOrEmpty(documentPath))
        {
            jsonResult = System.IO.File.ReadAllText(documentPath);
            string[] searchStrings = { "textMarkupAnnotation", "measureShapeAnnotation", "freeTextAnnotation", "stampAnnotations", "signatureInkAnnotation", "stickyNotesAnnotation", "signatureAnnotation", "AnnotationType" };
            bool isnewJsonFile = !searchStrings.Any(jsonResult.Contains);
            if (isnewJsonFile)
            {
                byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                jsonObject["importedData"] = Convert.ToBase64String(bytes);
				JsonResult = pdfviewer.ImportAnnotation(jsonObject);
            	return Results.Content(JsonConvert.SerializeObject(JsonResult));
            }
        }
        else
        {
            return Results.Ok(jsonObject["document"] + " is not found");
        }
    }
    else
    {
        string extension = Path.GetExtension(jsonObject["importedData"]);
        if (extension != ".xfdf")
        {
            JsonResult = pdfviewer.ImportAnnotation(jsonObject);
            return Results.Content(JsonConvert.SerializeObject(JsonResult));
        }
        else
        {
            string documentPath = GetDocumentPath(jsonObject["importedData"]);
            if (!string.IsNullOrEmpty(documentPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                jsonObject["importedData"] = Convert.ToBase64String(bytes);
                JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                return Results.Ok(JsonConvert.SerializeObject(JsonResult));
            }
            else
            {
                return Results.Ok(jsonObject["document"] + " is not found");
            }
        }
    }
    return Results.Ok(jsonResult);
});

app.MapPost("pdfviewer/ExportFormFields", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    string jsonResult = pdfviewer.ExportFormFields(jsonObject);
    return jsonResult;
});
app.MapPost("pdfviewer/ImportFormFields", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    object jsonResult = pdfviewer.ImportFormFields(jsonObject);
    return Results.Content(JsonConvert.SerializeObject(jsonResult));
});
app.MapPost("pdfviewer/Unload", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    //Initialize the PDF Viewer object with memory cache object
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    pdfviewer.ClearCache(jsonObject);
    string jsonResult = "Document cache is cleared";
    return jsonResult;
});
app.MapPost("pdfviewer/Download", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    //Initialize the PDF Viewer object with memory cache object
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
    return documentBase;
});
app.MapPost("pdfviewer/PrintImages", (Dictionary<string, object> args) =>
{
    Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
    //Initialize the PDF Viewer object with memory cache object
    PdfRenderer pdfviewer = new PdfRenderer(_cache);
    object pageImage = pdfviewer.GetPrintImage(jsonObject);
    return Results.Content(JsonConvert.SerializeObject(pageImage));
});
string GetDocumentPath(string document)
{
    string documentPath = string.Empty;
    if (!System.IO.File.Exists(document))
    {
        var path = _hostingEnvironment.ContentRootPath;
        if (System.IO.File.Exists(path + "/Data/" + document))
            documentPath = path + "/Data/" + document;
    }
    else
    {
        documentPath = document;
    }
    Console.WriteLine(documentPath);
    return documentPath;
}
app.Run();
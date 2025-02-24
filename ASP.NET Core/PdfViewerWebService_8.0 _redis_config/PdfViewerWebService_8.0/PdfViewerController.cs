using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using Syncfusion.EJ2.PdfViewer;
using System.Text.Json;
using System.Drawing;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Redaction;

namespace ej2_pdfviewer_web_service.Controllers
{
    [Route("api/[controller]")]
    public class PdfViewerController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private IMemoryCache _mCache;
        private IDistributedCache _dCache;
        private IConfiguration _configuration;
        private int _slidingTime = 0;
        string path;
        public PdfViewerController(IMemoryCache memoryCache, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IDistributedCache cache, IConfiguration configuration)
        {
            _mCache = memoryCache;
            _dCache = cache;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _slidingTime = int.Parse(_configuration["DOCUMENT_SLIDING_EXPIRATION_TIME"]);
            path = _configuration["DOCUMENT_PATH"];
            //check the document path environment variable value and assign default data folder
            //if it is null.
            path = string.IsNullOrEmpty(path) ? Path.Combine(_hostingEnvironment.ContentRootPath, "Data") : Path.Combine(_hostingEnvironment.ContentRootPath, path);
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("Load")]
        public IActionResult Load([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            MemoryStream stream = new MemoryStream();
            object jsonResult = new object();
            if (jsonObject != null && jsonObject.ContainsKey("document"))
            {
                string documentName = jsonObject["document"];
                if (bool.Parse(jsonObject["isFileName"]))
                {
                    string documentPath = GetDocumentPath(documentName);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        stream = new MemoryStream(bytes);
                    }
                    else
                    {
                        bool result = Uri.TryCreate(documentName, UriKind.Absolute, out Uri uriResult)
      && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (result)
                        {
                            stream = GetDocumentFromURL(documentName).Result;
                            if (stream != null)
                                stream.Position = 0;
                            else
                                return this.Content(documentName + " is not a PDF document");
                        }
                        else
                            return this.Content(documentName + " is not found");
                    }
                }
                else
                {
                    byte[] bytes = Convert.FromBase64String(documentName);
                    stream = new MemoryStream(bytes);
                }
            }
            jsonResult = pdfviewer.Load(stream, jsonObject);
            return Content(JsonSerializer.Serialize(jsonResult));
        }

        async Task<MemoryStream> GetDocumentFromURL(string url)
        {
            var client = new HttpClient(); ;
            var response = await client.GetAsync(url);
            var rawStream = await response.Content.ReadAsStreamAsync();
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentType.MediaType == "application/pdf")
            {
                MemoryStream docStream = new MemoryStream();
                rawStream.CopyTo(docStream);
                return docStream;
            }
            else { return null; }
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("Bookmarks")]
        public IActionResult Bookmarks([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            object jsonResult = pdfviewer.GetBookmarks(jsonObject);
            return Content(JsonSerializer.Serialize(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("RenderPdfPages")]
        public IActionResult RenderPdfPages([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            object jsonResult = pdfviewer.GetPage(jsonObject);
            return Content(JsonSerializer.Serialize(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("RenderPdfTexts")]
        //Post action for processing the PDF texts  
        public IActionResult RenderPdfTexts([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            object jsonResult = pdfviewer.GetDocumentText(jsonObject);
            return Content(JsonSerializer.Serialize(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("RenderAnnotationComments")]
        public IActionResult RenderAnnotationComments([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
            return Content(JsonSerializer.Serialize(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("Unload")]
        public IActionResult Unload([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            pdfviewer.ClearCache(jsonObject);
            return this.Content("Document cache is cleared");
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("RenderThumbnailImages")]
        public IActionResult RenderThumbnailImages([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            object result = pdfviewer.GetThumbnailImages(jsonObject);
            return Content(JsonSerializer.Serialize(result));
        }

        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("Download")]
        public IActionResult Download([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            return Content(documentBase);
        }

        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("Redaction")]
        public IActionResult Redaction([FromBody] Dictionary<string, string> jsonObject)
        {
            string RedactionText = "Redacted";
            var finalbase64 = string.Empty;
            if (jsonObject != null && jsonObject.ContainsKey("base64String"))
            {
                string base64 = jsonObject["base64String"];
                string base64String = base64.Split(new string[] { "data:application/pdf;base64," }, StringSplitOptions.None)[1];
                if (base64String != null || base64String != string.Empty)
                {
                    byte[] byteArray = Convert.FromBase64String(base64String);
                    Console.WriteLine("redaction");
                    PdfLoadedDocument loadedDocument = new PdfLoadedDocument(byteArray);
                    foreach (PdfLoadedPage loadedPage in loadedDocument.Pages)
                    {
                        List<PdfLoadedAnnotation> removeItems = new List<PdfLoadedAnnotation>();
                        foreach (PdfLoadedAnnotation annotation in loadedPage.Annotations)
                        {
                            if (annotation is PdfLoadedRectangleAnnotation)
                            {
                                if (annotation.Author == "Redaction")
                                {
                                    // Add the annotation to the removeItems list
                                    removeItems.Add(annotation);
                                    // Create a new redaction with the annotation bounds and color
                                    PdfRedaction redaction = new PdfRedaction(annotation.Bounds, annotation.Color);
                                    // Add the redaction to the page
                                    loadedPage.AddRedaction(redaction);
                                    annotation.Flatten = true;
                                }
                                if (annotation.Author == "Text")
                                {
                                    // Add the annotation to the removeItems list
                                    removeItems.Add(annotation);
                                    // Create a new redaction with the annotation bounds and color
                                    PdfRedaction redaction = new PdfRedaction(annotation.Bounds);
                                    //Set the font family and font size
                                    PdfStandardFont font = new PdfStandardFont(PdfFontFamily.Courier, 8);
                                    //Create the appearance like repeated text in the redaction area 
                                    CreateRedactionAppearance(redaction.Appearance.Graphics, PdfTextAlignment.Left, true, new SizeF(annotation.Bounds.Width, annotation.Bounds.Height), RedactionText, font, PdfBrushes.Red);
                                    // Add the redaction to the page
                                    loadedPage.AddRedaction(redaction);
                                    annotation.Flatten = true;
                                }
                                //Apply the pattern for the Redaction
                                if (annotation.Author == "Pattern")
                                {
                                    // Add the annotation to the removeItems list
                                    removeItems.Add(annotation);
                                    // Create a new redaction with the annotation bounds and color
                                    PdfRedaction redaction = new PdfRedaction(annotation.Bounds);
                                    Syncfusion.Drawing.RectangleF rect = new Syncfusion.Drawing.RectangleF(0, 0, 8, 8);
                                    PdfTilingBrush tillingBrush = new PdfTilingBrush(rect);
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.Gray, new Syncfusion.Drawing.RectangleF(0, 0, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new Syncfusion.Drawing.RectangleF(2, 0, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(4, 0, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new Syncfusion.Drawing.RectangleF(6, 0, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new Syncfusion.Drawing.RectangleF(0, 2, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(2, 2, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(4, 2, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(6, 2, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(0, 4, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new Syncfusion.Drawing.RectangleF(2, 4, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(4, 4, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new Syncfusion.Drawing.RectangleF(6, 4, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(0, 6, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new Syncfusion.Drawing.RectangleF(2, 6, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(4, 6, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new Syncfusion.Drawing.RectangleF(6, 6, 2, 2));
                                    rect = new Syncfusion.Drawing.RectangleF(0, 0, 16, 14);
                                    PdfTilingBrush tillingBrushNew = new PdfTilingBrush(rect);
                                    tillingBrushNew.Graphics.DrawRectangle(tillingBrush, rect);
                                    //Set the pattern for the redaction area
                                    redaction.Appearance.Graphics.DrawRectangle(tillingBrushNew, new Syncfusion.Drawing.RectangleF(0, 0, annotation.Bounds.Width, annotation.Bounds.Height));
                                    // Add the redaction to the page
                                    loadedPage.AddRedaction(redaction);
                                    annotation.Flatten = true;
                                }
                            }
                            else if (annotation is PdfLoadedRubberStampAnnotation)
                            {
                                if (annotation.Author == "Image")
                                {
                                    Stream[] images = PdfLoadedRubberStampAnnotationExtension.GetImages(annotation as PdfLoadedRubberStampAnnotation);
                                    // Create a new redaction with the annotation bounds and color
                                    PdfRedaction redaction = new PdfRedaction(annotation.Bounds);
                                    images[0].Position = 0;
                                    PdfImage image = new PdfBitmap(images[0]);
                                    //Apply the image to redaction area
                                    redaction.Appearance.Graphics.DrawImage(image, new Syncfusion.Drawing.RectangleF(0, 0, annotation.Bounds.Width, annotation.Bounds.Height));
                                    // Add the redaction to the page
                                    loadedPage.AddRedaction(redaction);
                                    annotation.Flatten = true;
                                }
                            }
                        }
                        foreach (PdfLoadedAnnotation annotation1 in removeItems)
                        {
                            loadedPage.Annotations.Remove(annotation1);
                        }
                    }
                    loadedDocument.Redact();
                    MemoryStream stream = new MemoryStream();
                    loadedDocument.Save(stream);
                    stream.Position = 0;
                    loadedDocument.Close(true);
                    byteArray = stream.ToArray();
                    finalbase64 = "data:application/pdf;base64," + Convert.ToBase64String(byteArray);
                    stream.Dispose();
                    return Content(finalbase64);
                }
            }

            return Content("data:application/pdf;base64," + "");
        }

        //The Method used for apply the text in the full area of redaction rectangle
        private static void CreateRedactionAppearance(PdfGraphics graphics, PdfTextAlignment alignment, bool repeat, SizeF size, string overlayText, PdfFont font, PdfBrush textcolor)
        {
            float col = 0, row;
            if (font == null) font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
            int textAlignment = Convert.ToInt32(alignment);
            float y = 0, x = 0, diff = 0;
            Syncfusion.Drawing.RectangleF rect;
            Syncfusion.Drawing.SizeF textsize = font.MeasureString(overlayText);

            if (repeat)
            {
                col = size.Width / textsize.Width;
                row = (float)Math.Floor(size.Height / font.Size);
                diff = Math.Abs(size.Width - (float)(Math.Floor(col) * textsize.Width));
                if (textAlignment == 1)
                    x = diff / 2;
                if (textAlignment == 2)
                    x = diff;
                for (int i = 1; i < col; i++)
                {
                    for (int j = 0; j < row; j++)
                    {
                        rect = new Syncfusion.Drawing.RectangleF(x, y, 0, 0);
                        graphics.DrawString(overlayText, font, textcolor, rect);
                        y = y + font.Size;
                    }
                    x = x + textsize.Width;
                    y = 0;
                }
            }
            else
            {
                diff = Math.Abs(size.Width - textsize.Width);
                if (textAlignment == 1)
                {
                    x = diff / 2;
                }
                if (textAlignment == 2)
                {
                    x = diff;
                }
                rect = new Syncfusion.Drawing.RectangleF(x, 0, 0, 0);
                graphics.DrawString(overlayText, font, textcolor, rect);
            }
        }

        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("SaveUrl")]
        public async Task<IActionResult> SaveUrl([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            try
            {
                HttpClient client = new HttpClient();
                Dictionary<string, string> RequestDetails = new Dictionary<string, string>();
                RequestDetails.Add("Url", jsonObject["RequestedUrl"]);
                RequestDetails.Add("base64", jsonObject["base64Data"]);
                string jsonString = JsonSerializer.Serialize(RequestDetails);
                StringContent Data = new StringContent(jsonString);
                string UriAddress = jsonObject["UriAddress"];
                var result = await client.PutAsync(UriAddress, Data);
                if (result.StatusCode.ToString() == "OK")
                {
                    return Content("Document saved successfully!");
                }
                else
                {
                    return Content("Failed to save the document!");
                }
            }
            catch (Exception exception)
            {
                return Content(exception.Message);
            }
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("PrintImages")]
        public IActionResult PrintImages([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            object pageImage = pdfviewer.GetPrintImage(jsonObject);
            return Content(JsonSerializer.Serialize(pageImage));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("ExportAnnotations")]
        public IActionResult ExportAnnotations([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            string jsonResult = pdfviewer.ExportAnnotation(jsonObject);
            return Content(jsonResult);
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("ImportAnnotations")]
        public IActionResult ImportAnnotations([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            string jsonResult = string.Empty;
            object JsonResult;
            if (jsonObject != null && jsonObject.ContainsKey("fileName"))
            {
                string documentPath = GetDocumentPath(jsonObject["fileName"]);
                if (!string.IsNullOrEmpty(documentPath))
                {
                    jsonResult = System.IO.File.ReadAllText(documentPath);
                }
                else
                {
                    return this.Content(jsonObject["document"] + " is not found");
                }
            }
            else
            {
                string extension = Path.GetExtension(jsonObject["importedData"]);
                if (extension != ".xfdf")
                {
                    JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                    return Content(JsonSerializer.Serialize(JsonResult));
                }
                else
                {
                    string documentPath = GetDocumentPath(jsonObject["importedData"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        jsonObject["importedData"] = Convert.ToBase64String(bytes);
                        JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                        return Content(JsonSerializer.Serialize(JsonResult));
                    }
                    else
                    {
                        return this.Content(jsonObject["document"] + " is not found");
                    }
                }

            }
            return Content(jsonResult);
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("ExportFormFields")]
        public IActionResult ExportFormFields([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            string jsonResult = pdfviewer.ExportFormFields(jsonObject);
            return Content(jsonResult);
        }
        [AcceptVerbs("Post")]
        [HttpPost]
        [EnableCors("AllowAllOrigins")]
        [Route("ImportFormFields")]
        public IActionResult ImportFormFields([FromBody] Dictionary<string, object> args)
        {
            Dictionary<string, string> jsonObject = args.ToDictionary(k => k.Key, k => k.Value?.ToString());
            PdfRenderer pdfviewer;
            if (Startup.isRedisCacheEnable)
                pdfviewer = new PdfRenderer(_mCache, _dCache, _slidingTime);
            else
                pdfviewer = new PdfRenderer(_mCache, _slidingTime);
            object jsonResult = pdfviewer.ImportFormFields(jsonObject);
            return Content(JsonSerializer.Serialize(jsonResult));
        }


        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        private string GetDocumentPath(string document)
        {
            if (!System.IO.File.Exists(document))
            {
                string documentPath = Path.Combine(path, document);
                if (System.IO.File.Exists(documentPath))
                    return documentPath;
                else
                    return string.Empty;
            }
            else
                return document;
        }
    }
}

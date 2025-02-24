using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Syncfusion.EJ2.PdfViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace PdfViewerService2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PdfViewerController : ControllerBase
    {
        private IHostingEnvironment _hostingEnvironment;
        //Initialize the memory cache object   
        public IMemoryCache _cache;
        public PdfViewerController(IHostingEnvironment hostingEnvironment, IMemoryCache cache)
        {
            _hostingEnvironment = hostingEnvironment;
            _cache = cache;
            Console.WriteLine("PdfViewerController initialized");
        }

        [HttpPost("Load")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/Load")]
        //Post action for Loading the PDF documents   
        public IActionResult Load([FromBody] Dictionary<string, string> jsonObject)
        {
            Console.WriteLine("Load called");
            //Initialize the PDF viewer object with memory cache object
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
                        string fileName = jsonObject["document"].Split(new string[] { "://" }, StringSplitOptions.None)[0];

                        if (fileName == "http" || fileName == "https")
                        {
                            WebClient WebClient = new WebClient();
                            byte[] pdfDoc = WebClient.DownloadData(jsonObject["document"]);
                            stream = new MemoryStream(pdfDoc);
                        }

                        else
                        {
                            return this.Content(jsonObject["document"] + " is not found");
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
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

         [AcceptVerbs("Post")]
        [HttpPost("ValidatePassword")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/ValidatePassword")]
        public IActionResult ValidatePassword([FromBody] Dictionary<string, string> jsonObject)
        {
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
                        string fileName = jsonObject["document"].Split(new string[] { "://" }, StringSplitOptions.None)[0];

                        if (fileName == "http" || fileName == "https")
                        {
                            WebClient WebClient = new WebClient();
                            byte[] pdfDoc = WebClient.DownloadData(jsonObject["document"]);
                            stream = new MemoryStream(pdfDoc);
                        }

                        else
                        {
                            return this.Content(jsonObject["document"] + " is not found");
                        }
                    }
                }
                else
                {
                    byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
                    stream = new MemoryStream(bytes);
                }
            }
            string password = null;
            if (jsonObject.ContainsKey("password"))
            {
                password = jsonObject["password"];
            }
            var result = pdfviewer.Load(stream, password);

            return Content(JsonConvert.SerializeObject(result));
        }
        
        [AcceptVerbs("Post")]
        [HttpPost("Bookmarks")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/Bookmarks")]
        //Post action for processing the bookmarks from the PDF documents
        public IActionResult Bookmarks([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonResult = pdfviewer.GetBookmarks(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost("RenderPdfPages")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/RenderPdfPages")]
        //Post action for processing the PDF documents  
        public IActionResult RenderPdfPages([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.GetPage(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost("RenderPdfTexts")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/RenderPdfTexts")]
        //Post action for processing the PDF texts  
        public IActionResult RenderPdfTexts([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.GetDocumentText(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost("RenderThumbnailImages")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/RenderThumbnailImages")]
        //Post action for rendering the ThumbnailImages
        public IActionResult RenderThumbnailImages([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object result = pdfviewer.GetThumbnailImages(jsonObject);
            return Content(JsonConvert.SerializeObject(result));
        }
        [AcceptVerbs("Post")]
        [HttpPost("RenderAnnotationComments")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/RenderAnnotationComments")]
        //Post action for rendering the annotations
        public IActionResult RenderAnnotationComments([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }
        [AcceptVerbs("Post")]
        [HttpPost("ExportAnnotations")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/ExportAnnotations")]
        //Post action to export annotations
        public IActionResult ExportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string jsonResult = pdfviewer.ExportAnnotation(jsonObject);
            return Content(jsonResult);
        }
        [AcceptVerbs("Post")]
        [HttpPost("ImportAnnotations")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/ImportAnnotations")]
        //Post action to import annotations
        public IActionResult ImportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
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
                        jsonResult = JsonConvert.SerializeObject(JsonResult);
                    }
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
                    return Content(JsonConvert.SerializeObject(JsonResult));
                }
                else
                {
                    string documentPath = GetDocumentPath(jsonObject["importedData"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        jsonObject["importedData"] = Convert.ToBase64String(bytes);
                        JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                        return Content(JsonConvert.SerializeObject(JsonResult));
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
        [HttpPost("ExportFormFields")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/ExportFormFields")]
        public IActionResult ExportFormFields([FromBody] Dictionary<string, string> jsonObject)

        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string jsonResult = pdfviewer.ExportFormFields(jsonObject);
            return Content(jsonResult);
        }

        [AcceptVerbs("Post")]
        [HttpPost("ImportFormFields")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/ImportFormFields")]
        public IActionResult ImportFormFields([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            jsonObject["data"] = GetDocumentPath(jsonObject["data"]);
            object jsonResult = pdfviewer.ImportFormFields(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost("Unload")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/Unload")]
        //Post action for unloading and disposing the PDF document resources  
        public IActionResult Unload([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            pdfviewer.ClearCache(jsonObject);
            return this.Content("Document cache is cleared");
        }


        [HttpPost("Download")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/Download")]
        //Post action for downloading the PDF documents
        public IActionResult Download([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            return Content(documentBase);
        }

        [HttpPost("PrintImages")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/PrintImages")]
        //Post action for printing the PDF documents
        public IActionResult PrintImages([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object pageImage = pdfviewer.GetPrintImage(jsonObject);
            return Content(JsonConvert.SerializeObject(pageImage));
        }

        [HttpPost("Download")]
        [Microsoft.AspNetCore.Cors.EnableCors("MyPolicy")]
        [Route("[controller]/Redaction")]
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
        
        //Gets the path of the PDF document
        private string GetDocumentPath(string document)
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
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
    }
}
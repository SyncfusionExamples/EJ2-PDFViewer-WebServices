using Newtonsoft.Json;
using Syncfusion.EJ2.PdfViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using Syncfusion.Pdf.Parsing;
using System.Security.Cryptography.X509Certificates;
using Syncfusion.Pdf.Security;
using Syncfusion.Pdf;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Presentation;
using Syncfusion.PresentationToPdfConverter;
using Syncfusion.XlsIO;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.OfficeChart;
using Syncfusion.OfficeChartToImageConverter;
using WFormatType = Syncfusion.DocIO.FormatType;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Redaction;
using System.Drawing;

namespace MVCwebservice.webapi
{
    public class PdfViewerController : ApiController
    {
        [System.Web.Mvc.HttpPost]
        public object Load(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
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
                        return (jsonObject["document"] + " is not found");
                    }
                }
                else
                {
                    byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
                    stream = new MemoryStream(bytes);
                }
            }
            jsonResult = pdfviewer.Load(stream, jsonObject);
            return (JsonConvert.SerializeObject(jsonResult));
        }

        [System.Web.Mvc.HttpPost]
        public object ValidatePassword(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
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
                        return (jsonObject["document"] + " is not found");
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

            return (JsonConvert.SerializeObject(result));
        }
        
        [System.Web.Mvc.HttpPost]
        public object Bookmarks(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            var jsonResult = pdfviewer.GetBookmarks(jsonObject);
            return (jsonResult);
        }

        [System.Web.Mvc.HttpPost]
        public object RenderPdfPages(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object jsonResult = pdfviewer.GetPage(jsonObject);
            return (JsonConvert.SerializeObject(jsonResult));
        }

        [System.Web.Mvc.HttpPost]
        public object RenderThumbnailImages(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object result = pdfviewer.GetThumbnailImages(jsonObject);
            return (JsonConvert.SerializeObject(result));
        }

        [System.Web.Mvc.HttpPost]
        public object RenderPdfTexts(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object result = pdfviewer.GetDocumentText(jsonObject);
            return (JsonConvert.SerializeObject(result));
        }

        [System.Web.Mvc.HttpPost]
        public object RenderAnnotationComments(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
            return (JsonConvert.SerializeObject(jsonResult));
        }

        [System.Web.Mvc.HttpPost]
        public object Unload(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            pdfviewer.ClearCache(jsonObject);
            return ("Document cache is cleared");
        }

        [System.Web.Mvc.HttpPost]
        public HttpResponseMessage Download(Dictionary<string, string> jsonObject)
        {

            PdfRenderer pdfviewer = new PdfRenderer();
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            return (GetPlainText(documentBase));
        }

        [System.Web.Mvc.HttpPost]
        public object PrintImages(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object pageImage = pdfviewer.GetPrintImage(jsonObject);
            return (pageImage);
        }

        [System.Web.Mvc.HttpPost]
        //Post action to export annotations
        [System.Web.Mvc.Route("{id}/ExportAnnotations")]
        public HttpResponseMessage ExportAnnotations(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            string jsonResult = pdfviewer.ExportAnnotation(jsonObject);
            return (GetPlainText(jsonResult));
        }

        [System.Web.Mvc.HttpPost]
        //Post action to import annotations
        public object ImportAnnotations(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
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
                    return (jsonObject["document"] + " is not found");
                }
            }
            else
            {
                string extension = Path.GetExtension(jsonObject["importedData"]);
                if (extension != ".xfdf")
                {
                    JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                    return (GetPlainText((JsonConvert.SerializeObject(JsonResult))));
                }
                else
                {
                    string documentPath = GetDocumentPath(jsonObject["importedData"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        jsonObject["importedData"] = Convert.ToBase64String(bytes);
                        JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                        return (GetPlainText((JsonConvert.SerializeObject(JsonResult))));
                    }
                    else
                    {
                        return (jsonObject["document"] + " is not found");
                    }
                }
            }
            return (GetPlainText((jsonResult)));
        }

        [System.Web.Mvc.HttpPost]
        public HttpResponseMessage ExportFormFields(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            string jsonResult = pdfviewer.ExportFormFields(jsonObject);
            //   return (JsonConvert.SerializeObject(jsonResult));
            return (GetPlainText(jsonResult));
        }

        [System.Web.Mvc.HttpPost]
        public object ImportFormFields(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object jsonResult = pdfviewer.ImportFormFields(jsonObject);
            return (JsonConvert.SerializeObject(jsonResult));
        }

        [System.Web.Mvc.HttpPost]
        public object LoadFile(Dictionary<string, string> jsonObject)
        {
            if (jsonObject.ContainsKey("data"))
            {
                string base64 = jsonObject["data"];
                //string fileName = args.FileData[0].Name; 
                string type = jsonObject["type"];
                string data = base64.Split(',')[1];
                byte[] bytes = Convert.FromBase64String(data);
                var outputStream = new MemoryStream();
                Syncfusion.Pdf.PdfDocument pdfDocument = new Syncfusion.Pdf.PdfDocument();
                using (Stream stream = new MemoryStream(bytes))
                {
                    switch (type)
                    {
                        case "docx":
                        case "dot":
                        case "doc":
                        case "dotx":
                        case "docm":
                        case "dotm":
                        case "rtf":
                            Syncfusion.DocIO.DLS.WordDocument doc = new Syncfusion.DocIO.DLS.WordDocument(stream, GetWFormatType(type));
                            //Initialization of DocIORenderer for Word to PDF conversion
                            DocToPDFConverter converter = new DocToPDFConverter();
                            //Converts Word document into PDF document
                            pdfDocument = converter.ConvertToPDF(doc);
                            doc.Close();
                            break;
                        case "pptx":
                        case "pptm":
                        case "potx":
                        case "potm":
                            //Loads or open an PowerPoint Presentation
                            IPresentation pptxDoc = Presentation.Open(stream);
                            pdfDocument = PresentationToPdfConverter.Convert(pptxDoc);
                            pptxDoc.Close();
                            break;
                        case "xlsx":
                        case "xls":
                            ExcelEngine excelEngine = new ExcelEngine();
                            //Loads or open an existing workbook through Open method of IWorkbooks
                            IWorkbook workbook = excelEngine.Excel.Workbooks.Open(stream);
                            //Initialize XlsIO renderer.
                            ExcelToPdfConverter Excelconverter = new ExcelToPdfConverter(workbook);
                            //Convert Excel document into PDF document
                            pdfDocument = Excelconverter.Convert();
                            workbook.Close();
                            break;
                        case "jpeg":
                        case "jpg":
                        case "png":
                        case "bmp":
                            //Add a page to the document
                            PdfPage page = pdfDocument.Pages.Add();
                            //Create PDF graphics for the page
                            PdfGraphics graphics = page.Graphics;
                            PdfBitmap image = new PdfBitmap(stream);
                            //Draw the image
                            graphics.DrawImage(image, 0, 0);
                            break;
                        case "pdf":
                            string pdfBase64String = Convert.ToBase64String(bytes);
                            return (GetPlainText("data:application/pdf;base64," + pdfBase64String));
                    }
                }
                pdfDocument.Save(outputStream);
                outputStream.Position = 0;
                byte[] byteArray = outputStream.ToArray();
                pdfDocument.Close();
                outputStream.Close();
                string base64String = Convert.ToBase64String(byteArray);
                return (GetPlainText("data:application/pdf;base64," + base64String));
            }
            return (GetPlainText("data:application/pdf;base64," + ""));
        }

        public static WFormatType GetWFormatType(string format)
        {
            if (string.IsNullOrEmpty(format))
                throw new NotSupportedException("This is not a valid Word documnet.");
            switch (format.ToLower())
            {
                case "dotx":
                    return WFormatType.Dotx;
                case "docx":
                    return WFormatType.Docx;
                case "docm":
                    return WFormatType.Docm;
                case "dotm":
                    return WFormatType.Dotm;
                case "dot":
                    return WFormatType.Dot;
                case "doc":
                    return WFormatType.Doc;
                case "rtf":
                    return WFormatType.Rtf;
                default:
                    throw new NotSupportedException("This is not a valid Word documnet.");
            }
        }
        [System.Web.Mvc.HttpPost]
        public object Redaction(Dictionary<string, string> jsonObject)
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
                                    RectangleF rect = new RectangleF(0, 0, 8, 8);
                                    PdfTilingBrush tillingBrush = new PdfTilingBrush(rect);
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.Gray, new RectangleF(0, 0, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new RectangleF(2, 0, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(4, 0, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new RectangleF(6, 0, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new RectangleF(0, 2, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(2, 2, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new RectangleF(4, 2, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(6, 2, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(0, 4, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new RectangleF(2, 4, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(4, 4, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.White, new RectangleF(6, 4, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new RectangleF(0, 6, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(2, 6, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.Black, new RectangleF(4, 6, 2, 2));
                                    tillingBrush.Graphics.DrawRectangle(PdfBrushes.DarkGray, new RectangleF(6, 6, 2, 2));
                                    rect = new RectangleF(0, 0, 16, 14);
                                    PdfTilingBrush tillingBrushNew = new PdfTilingBrush(rect);
                                    tillingBrushNew.Graphics.DrawRectangle(tillingBrush, rect);
                                    //Set the pattern for the redaction area
                                    redaction.Appearance.Graphics.DrawRectangle(tillingBrushNew, new RectangleF(0, 0, annotation.Bounds.Width, annotation.Bounds.Height));
                                    // Add the redaction to the page
                                    loadedPage.AddRedaction(redaction);
                                    annotation.Flatten = true;
                                }
                            }
                            else if (annotation is PdfLoadedRubberStampAnnotation)
                            {
                                if (annotation.Author == "Image")
                                {
                                    //Get the existing rubber stamp annotation.
                                    PdfLoadedRubberStampAnnotation rubberStampAnnotation = annotation as PdfLoadedRubberStampAnnotation;
                                    //Get the custom images used for the rubber stamp annotation.
                                    Image[] images = rubberStampAnnotation.GetImages();

                                    PdfRedaction redaction = new PdfRedaction(annotation.Bounds);
                                    //images[0].Position = 0;
                                    PdfImage image = new PdfBitmap(images[0]);
                                    //Apply the image to redaction area
                                    redaction.Appearance.Graphics.DrawImage(image, new RectangleF(0, 0, annotation.Bounds.Width, annotation.Bounds.Height));
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
                    return GetPlainText(finalbase64);
                }
            }

            return GetPlainText("data:application/pdf;base64," + "");
        }

        //The Method used for apply the text in the full area of redaction rectangle
        private static void CreateRedactionAppearance(PdfGraphics graphics, PdfTextAlignment alignment, bool repeat, SizeF size, string overlayText, PdfFont font, PdfBrush textcolor)
        {
            float col = 0, row;
            if (font == null) font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
            int textAlignment = Convert.ToInt32(alignment);
            float y = 0, x = 0, diff = 0;
            RectangleF rect;
            SizeF textsize = font.MeasureString(overlayText);
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
                        rect = new RectangleF(x, y, 0, 0);
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
                rect = new RectangleF(x, 0, 0, 0);
                graphics.DrawString(overlayText, font, textcolor, rect);
            }
        }

        private HttpResponseMessage GetPlainText(string pageImage)
        {
            var responseText = new HttpResponseMessage(HttpStatusCode.OK);
            responseText.Content = new StringContent(pageImage, System.Text.Encoding.UTF8, "text/plain");
            return responseText;
        }
        private string GetDocumentPath(string document)
        {
            string documentPath = string.Empty;
            if (!System.IO.File.Exists(document))
            {
                var path = HttpContext.Current.Request.PhysicalApplicationPath;
                if (System.IO.File.Exists(path + "Data\\" + document))
                    documentPath = path + "Data\\" + document;
            }
            else
            {
                documentPath = document;
            }
            return documentPath;
        }

        // GET api/values
        [System.Web.Mvc.HttpPost]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

    }
}

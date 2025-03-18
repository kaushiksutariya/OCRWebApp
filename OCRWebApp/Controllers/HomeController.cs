using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OCRWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController()
        {
            _httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Please select a valid image.";
                return View("Index");
            }

            string extractedText = await ExtractTextFromImage(file);
            ViewBag.ExtractedText = extractedText;
            return View("Index");
        }

        private async Task<string> ExtractTextFromImage(IFormFile file)
        {
            string apiKey = "helloworld"; // 🔹 Replace with your OCR.Space API Key
            string apiUrl = "https://api.ocr.space/parse/image";

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                var form = new MultipartFormDataContent();
                form.Add(new StringContent(apiKey), "apikey");
                form.Add(new StringContent("eng"), "language");
                form.Add(new ByteArrayContent(fileBytes), "image", file.FileName);

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, form);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                return ParseOCRResponse(jsonResponse);
            }
        }

        private string ParseOCRResponse(string jsonResponse)
        {
            var json = JObject.Parse(jsonResponse);
            var parsedResults = json["ParsedResults"];
            if (parsedResults != null && parsedResults.HasValues)
            {
                return parsedResults[0]["ParsedText"].ToString();
            }
            return "No text found";
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

using Master_pice.Helpers;
using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Master_pice.Controllers
{
    public class AiController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public AiController(IHttpClientFactory httpClientFactory, AppDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> AiWizard()
        {
            var questions = await GenerateAiQuestions();
            var session = new AiMultiStepViewModel
            {
                QuestionsRaw = questions,
                CurrentIndex = 0
            };

            HttpContext.Session.SetObjectAsJson("AiFlow", session);
            return View("AiWizard", session);
        }

        private async Task<List<(string Question, List<string> Choices, string Answer)>> GenerateAiQuestions()
        {
            var prompt = @"
أعطني 5 أسئلة متسلسلة لاختيار جهاز كمبيوتر مناسب، بصيغة JSON فقط كالتالي:
[
  { ""question"": ""ما استخدامك؟"", ""choices"": [""ألعاب"", ""برمجة"", ""تصفح"", ""تصميم""] },
  ...
]
بدون أي شرح خارجي.";

            var body = new
            {
                contents = new[] {
                    new {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key=AIzaSyAxg31MZH-Db6bfUdhTNb42EP7uulVqG78")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            var res = await client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    throw new Exception("⚠️ الرد لا يحتوي على candidates.");

                var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("⚠️ لا يوجد نص في الرد.");

                var start = text.IndexOf('[');
                var end = text.LastIndexOf(']');
                if (start == -1 || end == -1 || end <= start)
                    throw new Exception("⚠️ الرد لا يحتوي على JSON مصفوفة.");

                var jsonText = text.Substring(start, end - start + 1);
                var parsedList = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(jsonText);

                var questions = parsedList!.Select(q =>
                (
                    q["question"].GetString()!,
                    q["choices"].EnumerateArray().Select(x => x.GetString()!).ToList(),
                    ""
                )).ToList();

                return questions;
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ فشل تحليل أسئلة Gemini: " + ex.Message);
                throw new Exception("⚠️ فشل توليد الأسئلة من Gemini.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AiSubmitAll(List<string> Answers)
        {
            var session = HttpContext.Session.GetObjectFromJson<AiMultiStepViewModel>("AiFlow");
            if (session == null || session.QuestionsRaw.Count != Answers.Count)
                return BadRequest("حدث خطأ أثناء استلام البيانات.");

            var history = session.QuestionsRaw.Select((q, i) => (q.Question, Answer: Answers[i])).ToList();
            var recommendation = await SendAllAnswersToGemini(history);

            var vm = new RecommendationViewModel
            {
                RecommendedDevice = recommendation
            };

            HttpContext.Session.Remove("AiFlow");
            return View("AiFinal", vm);
        }

        private async Task<object> SendAllAnswersToGemini(List<(string Question, string Answer)> history)
        {
            var formatted = string.Join("\n", history.Select(h => $"السؤال: {h.Question}\nالجواب: {h.Answer}"));

            var prompt = $@"
أنت مساعد خبير في ترشيح أفضل جهاز كمبيوتر (لابتوب أو PC).
استنادًا إلى هذه الأسئلة والأجوبة، اختر الجهاز الأنسب من قائمة الأجهزة المتوفرة (لا تختر من خارجها):

{formatted}

أعطني النتيجة النهائية على شكل JSON فقط كالتالي:
{{
  ""type"": ""Laptop"" أو ""PC"",
  ""id"": 3
}}

بدون أي شرح أو تعليق خارج JSON.
";

            var body = new
            {
                contents = new[] {
                    new {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key=AIzaSyAxg31MZH-Db6bfUdhTNb42EP7uulVqG78")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            var res = await client.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            Console.WriteLine("AI Response:\n" + json);

            try
            {
                var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("⚠️ لم يتم استلام نتيجة من الذكاء الاصطناعي.");

                var start = text.IndexOf('{');
                var end = text.LastIndexOf('}');
                if (start == -1 || end == -1)
                    throw new Exception("⚠️ الرد لا يحتوي على JSON صالح.");

                var cleanJson = text.Substring(start, end - start + 1);
                using var parsed = JsonDocument.Parse(cleanJson);
                var root = parsed.RootElement;

                if (!root.TryGetProperty("type", out var typeProp) || !root.TryGetProperty("id", out var idProp))
                    throw new Exception("⚠️ الذكاء الاصطناعي لم يرجع النوع أو المعرف.");

                var type = typeProp.GetString()?.ToLower();
                if (idProp.ValueKind != JsonValueKind.Number)
                    throw new Exception("⚠️ المعرف الذي تم إرجاعه ليس رقمًا.");

                var id = idProp.GetInt32();

                return type switch
                {
                    "laptop" => _context.Laptops.FirstOrDefault(x => x.LaptopID == id)
                                 ?? throw new Exception("⚠️ لم يتم العثور على اللابتوب بالمعرف المحدد."),
                    "pc" => _context.PCs.FirstOrDefault(x => x.PCID == id)
                             ?? throw new Exception("⚠️ لم يتم العثور على الجهاز المكتبي بالمعرف المحدد."),
                    _ => throw new Exception("⚠️ النوع الذي أرجعه الذكاء الاصطناعي غير معروف.")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error while parsing AI response: " + ex.Message);
                throw new Exception("⚠️ لم يتمكن الذكاء الاصطناعي من ترشيح جهاز مناسب.");
            }
        }

        // النسخة القديمة (مرجعية)
        private object RecommendBasedOn(List<(string Question, string Answer)> history)
        {
            var allDevices = _context.Laptops.Cast<dynamic>().ToList();
            allDevices.AddRange(_context.PCs);

            var priceQ = history.FirstOrDefault(h => h.Question?.Contains("السعر") == true || h.Question?.Contains("ميزانيتك") == true);

            if (priceQ.Question != null && !string.IsNullOrEmpty(priceQ.Answer))
            {
                decimal max = priceQ.Answer.Contains("350") ? 350 :
                              priceQ.Answer.Contains("500") ? 500 :
                              priceQ.Answer.Contains("750") ? 750 : 10000;

                allDevices = allDevices.Where(d => d.Price <= max).ToList();
            }

            object best = null;
            int bestScore = -1;

            foreach (var device in allDevices)
            {
                int score = 0;
                foreach (var (_, answer) in history)
                {
                    if (answer.Contains("ألعاب") && device.GPU.ToLower().Contains("rtx")) score += 5;
                    if (answer.Contains("تصميم") && device.RAM.Contains("16")) score += 3;
                    if (answer.Contains("تصفح") && device.RAM.Contains("8")) score += 2;
                }

                if (score > bestScore)
                {
                    best = device;
                    bestScore = score;
                }
            }

            return best!;
        }
    }
}

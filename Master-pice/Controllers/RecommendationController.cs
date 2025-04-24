using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Master_pice.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public RecommendationController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(int userId)
        {
            var userAnswers = await _context.UserAnswers
                .Include(a => a.Question)
                .Where(a => a.UserID == userId)
                .ToListAsync();

            var laptops = await _context.Laptops.ToListAsync();
            var pcs = await _context.PCs.ToListAsync();

            var prompt = BuildPrompt(userAnswers, laptops, pcs);
            var reply = await CallDeepSeekAPI(prompt);
            var selectedProduct = FindProductById(reply, laptops, pcs);


            ViewBag.RawReply = reply;
            return View("RecommendedProduct", selectedProduct);
        }

        private string BuildPrompt(List<UserAnswer> answers, List<Laptop> laptops, List<PC> pcs)
        {
            var sb = new StringBuilder();

            sb.AppendLine("📌 User preferences:");
            foreach (var answer in answers)
            {
                sb.AppendLine($"- {answer.Question.QuestionText}: {answer.SelectedOption}");
            }

            sb.AppendLine("\n📦 Available products (DO NOT invent new ones):");

            foreach (var p in laptops)
            {
                sb.AppendLine($"ID: Laptop_{p.LaptopID}, Brand: {p.Brand}, Model: {p.Model}, Price: {p.Price}, CPU: {p.Processor}, RAM: {p.RAM}, GPU: {p.GPU}, Storage: {p.Storage}, Screen: {p.ScreenSize}\"");
            }

            foreach (var p in pcs)
            {
                sb.AppendLine($"ID: PC_{p.PCID}, Brand: {p.Brand}, Price: {p.Price}, CPU: {p.Processor}, RAM: {p.RAM}, GPU: {p.GPU}, Storage: {p.Storage}");
            }

            sb.AppendLine("\n🔒 INSTRUCTIONS:");
            sb.AppendLine("✅ Based ONLY on the user answers, pick EXACTLY ONE best-matching product from above.");
            sb.AppendLine("❌ Do NOT invent or combine products.");
            sb.AppendLine("❌ Do NOT recommend a product that exceeds the user's budget.");
            sb.AppendLine("🎯 RETURN ONLY the ID of the product like 'Laptop_32' or 'PC_5' with NO explanation.");

            return sb.ToString();
        }

        private async Task<string> CallDeepSeekAPI(string prompt)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sk-335b7c97800c479a9bb17ebc22179158");

            var requestData = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "You are an AI that selects the best product strictly based on user preferences and budget from a list." },
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.deepseek.com/v1/chat/completions", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine("🔎 DeepSeek Response:");
            Console.WriteLine(responseJson);

            var json = JsonDocument.Parse(responseJson);

            if (json.RootElement.TryGetProperty("choices", out JsonElement choices))
            {
                return choices[0].GetProperty("message").GetProperty("content").GetString();
            }

            return "❌ Error: Invalid DeepSeek API response\n\n" + responseJson;
        }

        private object FindProductById(string reply, List<Laptop> laptops, List<PC> pcs)
        {
            reply = reply.Trim().Replace("\"", "").Replace("\n", "");

            if (reply.StartsWith("Laptop_") && int.TryParse(reply.Replace("Laptop_", ""), out int lid))
                return laptops.FirstOrDefault(l => l.LaptopID == lid);

            if (reply.StartsWith("PC_") && int.TryParse(reply.Replace("PC_", ""), out int pid))
                return pcs.FirstOrDefault(p => p.PCID == pid);

            return null;
        }

        public async Task<IActionResult> Survey()
        {
            var questions = await _context.Questions.ToListAsync();
            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswers(List<UserAnswer> Answers, int UserID)
        {
            foreach (var answer in Answers)
            {
                answer.UserID = UserID;
                answer.Timestamp = DateTime.Now;
                _context.UserAnswers.Add(answer);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { userId = UserID });
        }
    }
}
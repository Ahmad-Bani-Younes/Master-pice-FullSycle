using Master_pice.Helpers;
using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace Master_pice.Controllers
{
    public class RecommendationController1 : Controller
    {
        private readonly AppDbContext _context;

        public RecommendationController1(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Step(int id = 1)
        {
            var fullOrder = new List<int> { 23, 14, 13, 12, 15, 16, 17, 18, 19, 20, 21 };
            var answers = HttpContext.Session.GetObjectFromJson<Dictionary<int, string>>("WizardAnswers") ?? new();
            var pendingOrder = fullOrder.Where(qId => !answers.ContainsKey(qId)).ToList();

            if (id > pendingOrder.Count)
                return RedirectToAction("ShowResult");

            var questionId = pendingOrder[id - 1];
            var q = _context.Questions.FirstOrDefault(x => x.QuestionID == questionId);
            if (q == null) return Content($" No question found with ID = {questionId} in database.");

            var options = new List<string>();

            if (questionId == 14 && answers.TryGetValue(23, out var type))
            {
                type = type.ToLower();
                options = type.Contains("mac") ? new() { "MacOS" } :
                          type.Contains("pc") ? new() { "Windows", "Linux" } : options;
            }
            else if (questionId == 13)
            {
                options = new() { "Less than 350", "350 - 500", "500 - 750", "750+" };
            }
            else
            {
                options = new[] { q.Option1, q.Option2, q.Option3, q.Option4 }
                    .Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
            }

            var vm = new StepQuestionViewModel
            {
                CurrentStep = id,
                QuestionID = q.QuestionID,
                QuestionText = q.QuestionText,
                Options = options,
                TotalSteps = pendingOrder.Count
            };

            return View("Step", vm);
        }

        [HttpPost]
        public IActionResult Step(StepQuestionViewModel model)
        {
            var answers = HttpContext.Session.GetObjectFromJson<Dictionary<int, string>>("WizardAnswers") ?? new();
            answers[model.QuestionID] = model.SelectedAnswer;
            HttpContext.Session.SetObjectAsJson("WizardAnswers", answers);

            var devices = _context.Laptops.Cast<dynamic>().ToList();
            devices.AddRange(_context.PCs);

            foreach (var (qid, val) in answers)
            {
                string ans = val.ToLower();
                devices = qid switch
                {
                    23 => ans.Contains("laptop") ? devices.Where(d => d is Laptop).ToList()
                         : ans.Contains("pc") ? devices.Where(d => d is PC).ToList() : devices,

                    14 => ans.Contains("mac") ? devices.Where(d => d.Processor.ToLower().Contains("m1")).ToList()
                         : ans.Contains("windows") ? devices.Where(d => d.Processor.ToLower().Contains("intel") || d.Processor.ToLower().Contains("amd")).ToList()
                         : ans.Contains("linux") ? devices.Where(d => d.Description.ToLower().Contains("linux")).ToList() : devices,

                    13 => ans.Contains("less") ? devices.Where(d => d.Price <= 350).ToList()
                         : ans.Contains("350") ? devices.Where(d => d.Price > 350 && d.Price <= 500).ToList()
                         : ans.Contains("500") ? devices.Where(d => d.Price > 500 && d.Price <= 750).ToList()
                         : ans.Contains("750") ? devices.Where(d => d.Price > 750).ToList() : devices,

                    _ => devices
                };
            }

            return RedirectToAction("Step", new { id = model.CurrentStep + 1 });
        }

        [HttpGet]
        public IActionResult ShowResult()
        {
            var answers = HttpContext.Session.GetObjectFromJson<Dictionary<int, string>>("WizardAnswers");
            if (answers == null || !answers.Any()) return RedirectToAction("Step");

            var allLaptops = _context.Laptops.ToList();
            var allPCs = _context.PCs.ToList();

            if (answers.TryGetValue(23, out var type))
            {
                type = type.ToLower();
                if (type.Contains("pc")) allLaptops.Clear();
                else if (type.Contains("laptop")) allPCs.Clear();
            }

            if (answers.TryGetValue(12, out var os))
            {
                os = os.ToLower();
                if (os.Contains("mac"))
                {
                    allLaptops = allLaptops.Where(l => l.Processor.ToLower().Contains("m1")).ToList();
                    allPCs.Clear();
                }
                else if (os.Contains("linux"))
                {
                    allLaptops = allLaptops.Where(l => l.Description.ToLower().Contains("linux")).ToList();
                    allPCs = allPCs.Where(p => p.Description.ToLower().Contains("linux")).ToList();
                }
                else if (os.Contains("windows"))
                {
                    allLaptops = allLaptops.Where(l => l.Processor.ToLower().Contains("intel") || l.Processor.ToLower().Contains("amd")).ToList();
                    allPCs = allPCs.Where(p => p.Processor.ToLower().Contains("intel") || p.Processor.ToLower().Contains("amd")).ToList();
                }
            }

            if (answers.TryGetValue(13, out var price))
            {
                int maxPrice = price.ToLower() switch
                {
                    var p when p.Contains("less") => 350,
                    var p when p.Contains("350") => 500,
                    var p when p.Contains("500") => 750,
                    _ => 9999
                };

                allLaptops = allLaptops.Where(l => l.Price <= maxPrice).ToList();
                allPCs = allPCs.Where(p => p.Price <= maxPrice).ToList();
            }

            if (type.Contains("pc") && !allPCs.Any())
                return Content("❌ No suitable PC found. Try changing your answers.");
            if (type.Contains("laptop") && !allLaptops.Any())
                return Content("❌ No suitable Laptop found. Try changing your answers.");

            int GetScore(dynamic d, Dictionary<int, string> a)
            {
                double score = 0;
                foreach (var (qid, ans) in a)
                {
                    string answer = ans.ToLower();
                    string desc = d.Description?.ToLower() ?? "";
                    switch (qid)
                    {
                        case 12:
                            if (answer.Contains("mac") && d.Processor.ToLower().Contains("m1")) score += 3;
                            else if (answer.Contains("windows") && d.Processor.ToLower().Contains("intel")) score += 2;
                            else if (answer.Contains("linux") && desc.Contains("linux")) score += 2;
                            break;
                        case 13:
                            if (answer.Contains("less") && d.Price <= 350) score += 3;
                            else if (answer.Contains("350") && d.Price > 350 && d.Price <= 500) score += 2.5;
                            else if (answer.Contains("500") && d.Price > 500 && d.Price <= 750) score += 2;
                            else if (answer.Contains("750") && d.Price > 750) score += 1;
                            break;
                        case 15:
                            if (answer.Contains("gaming") && d.GPU.ToLower().Contains("rtx")) score += 3;
                            else if (answer.Contains("multimedia") && d.GPU.ToLower().Contains("gtx")) score += 2;
                            break;
                        case 16:
                            if (d.RAM.Contains("32") && answer.Contains("32")) score += 3;
                            else if (d.RAM.Contains("16") && answer.Contains("16")) score += 2.5;
                            else if (d.RAM.Contains("8") && answer.Contains("8")) score += 1.5;
                            break;
                        case 17:
                            if (d.Storage.ToLower().Contains("1tb") && answer.Contains("1tb")) score += 2.5;
                            else if (d.Storage.ToLower().Contains("512") && answer.Contains("512")) score += 2;
                            else if (d.Storage.ToLower().Contains("256") && answer.Contains("256")) score += 1;
                            break;
                        case 18:
                            if (desc.Contains("touchscreen") && answer.Contains("touch")) score += 2;
                            if (desc.Contains("oled") && answer.Contains("oled")) score += 1.5;
                            break;
                        case 19:
                            if (desc.Contains("lightweight") && answer.Contains("lightweight")) score += 2;
                            if (desc.Contains("compact") && answer.Contains("portable")) score += 1.5;
                            break;
                        case 20:
                            if (desc.Contains("backlit") && answer.Contains("backlit")) score += 1;
                            break;
                        case 21:
                            if (desc.Contains("pen") && answer.Contains("pen")) score += 1.5;
                            break;
                    }
                }
                return (int)Math.Round(score * 10);
            }

            var bestDevice = allLaptops.Cast<dynamic>().Concat(allPCs).OrderByDescending(d => GetScore(d, answers)).FirstOrDefault();
            var vm = new RecommendationViewModel
            {
                RecommendedDevice = bestDevice,
                Questions = _context.Questions.Take(10).ToList()
            };

            HttpContext.Session.Remove("WizardAnswers");
            return View("RecommendationResult", vm);
        }
    }
}

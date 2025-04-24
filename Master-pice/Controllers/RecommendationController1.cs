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
            // الترتيب الأساسي
            var fullOrder = new List<int> { 23, 14, 13, 12, 15, 16, 17, 18, 19, 20, 21 };

            // تحميل الإجابات الحالية
            var answers = HttpContext.Session.GetObjectFromJson<Dictionary<int, string>>("WizardAnswers") ?? new();

            // استبعاد الأسئلة اللي تم الإجابة عليها فعليًا
            var pendingOrder = fullOrder.Where(qId => !answers.ContainsKey(qId)).ToList();

            // التحقق من نهاية الخطوات
            if (id > pendingOrder.Count)
                return RedirectToAction("ShowResult");

            // السؤال التالي
            var questionId = pendingOrder[id - 1];
            var q = _context.Questions.FirstOrDefault(x => x.QuestionID == questionId);

            if (q == null)
                return Content($"❌ No question found with ID = {questionId} in database.");

            var filteredOptions = new List<string>();

            // فلترة الخيارات بناءً على السؤال السابق
            if (questionId == 14 && answers.TryGetValue(23, out var type)) // نظام التشغيل بعد اختيار النوع
            {
                if (type.ToLower().Contains("mac"))
                    filteredOptions = new List<string> { "MacOS" };
                else if (type.ToLower().Contains("pc") || type.ToLower().Contains("windows"))
                    filteredOptions = new List<string> { "Windows", "Linux" };
            }
            else if (questionId == 13) // السعر بناء على الاجهزة الموجودة حاليا
            {
                var min = _context.Laptops.Min(l => l.Price);
                var max = _context.Laptops.Max(l => l.Price);

                filteredOptions = new List<string>
        {
            "Less than 350",
            "350 - 500",
            "500 - 750",
            "750+"
        };
            }
            else
            {
                filteredOptions = new List<string> { q.Option1, q.Option2, q.Option3, q.Option4 }
                    .Where(opt => !string.IsNullOrWhiteSpace(opt)).ToList();
            }

            var vm = new StepQuestionViewModel
            {
                CurrentStep = id,
                QuestionID = q.QuestionID,
                QuestionText = q.QuestionText,
                Options = filteredOptions,
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

            var devices = new List<dynamic>();
            devices.AddRange(_context.Laptops.ToList());
            devices.AddRange(_context.PCs.ToList());


            string a = model.SelectedAnswer.ToLower();

            // فلترة مباشرة حسب كل الإجابات حتى الآن
            foreach (var (qid, val) in answers)
            {
                string ans = val.ToLower();
                switch (qid)
                {
                    case 23: // نوع الجهاز
                        if (ans.Contains("laptop"))
                            devices = devices.Where(d => d is Laptop).ToList();
                        else if (ans.Contains("pc"))
                            devices = devices.Where(d => d is PC).ToList();
                        break;

                    case 14: // نظام التشغيل
                        if (ans.Contains("mac"))
                            devices = devices.Where(d => d.Processor.ToLower().Contains("m1") || d.Processor.ToLower().Contains("apple")).ToList();
                        else if (ans.Contains("windows"))
                            devices = devices.Where(d => d.Processor.ToLower().Contains("intel") || d.Processor.ToLower().Contains("amd")).ToList();
                        else if (ans.Contains("linux"))
                            devices = devices.Where(d => d.Description.ToLower().Contains("linux")).ToList();
                        break;

                    case 13: // السعر
                        if (ans.Contains("less"))
                            devices = devices.Where(d => d.Price <= 350).ToList();
                        else if (ans.Contains("350"))
                            devices = devices.Where(d => d.Price > 350 && d.Price <= 500).ToList();
                        else if (ans.Contains("500"))
                            devices = devices.Where(d => d.Price > 500 && d.Price <= 750).ToList();
                        else if (ans.Contains("750"))
                            devices = devices.Where(d => d.Price > 750).ToList();
                        break;
                }
            }

            // حفظ آخر نتيجة للأجهزة ممكن لو بدك تستخدمها لاحقًا (مش ضروري)
            // HttpContext.Session.SetObjectAsJson("FilteredDevices", devices);

            return RedirectToAction("Step", new { id = model.CurrentStep + 1 });
        }




      [HttpGet]
public IActionResult ShowResult()
{
    var answers = HttpContext.Session.GetObjectFromJson<Dictionary<int, string>>("WizardAnswers");
    if (answers == null || !answers.Any())
        return RedirectToAction("Step");

    var allLaptops = _context.Laptops.ToList();
    var allPCs = _context.PCs.ToList();

    if (answers.TryGetValue(23, out var typeAnswer))
    {
        if (typeAnswer.Contains("pc"))
            allLaptops.Clear();
        else if (typeAnswer.Contains("laptop"))
            allPCs.Clear();
    }

    // فلترة نظام التشغيل
    if (answers.TryGetValue(12, out var osAnswer))
    {
        string a = osAnswer.ToLower();
        if (a.Contains("mac"))
        {
            allLaptops = allLaptops.Where(l => l.Processor.ToLower().Contains("m1") || l.Processor.ToLower().Contains("apple")).ToList();
            allPCs.Clear();
        }
        else if (a.Contains("linux"))
        {
            allLaptops = allLaptops.Where(l => l.Description.ToLower().Contains("linux")).ToList();
            allPCs = allPCs.Where(p => p.Description.ToLower().Contains("linux")).ToList();
        }
        else if (a.Contains("windows"))
        {
            allLaptops = allLaptops.Where(l => l.Processor.ToLower().Contains("intel") || l.Processor.ToLower().Contains("amd")).ToList();
            allPCs = allPCs.Where(p => p.Processor.ToLower().Contains("intel") || p.Processor.ToLower().Contains("amd")).ToList();
        }
    }

    // فلترة السعر
    if (answers.TryGetValue(13, out var priceAnswer))
    {
        int maxPrice = 10000;
        if (priceAnswer.Contains("less")) maxPrice = 350;
        else if (priceAnswer.Contains("350")) maxPrice = 500;
        else if (priceAnswer.Contains("500")) maxPrice = 750;
        else if (priceAnswer.Contains("750")) maxPrice = 9999;

        allLaptops = allLaptops.Where(l => l.Price <= maxPrice).ToList();
        allPCs = allPCs.Where(p => p.Price <= maxPrice).ToList();
    }

    // ✅ تأكيد وجود أجهزة من النوع المطلوب بعد كل الفلاتر
    if (typeAnswer.Contains("pc") && allPCs.Count == 0)
        return Content("❌ No suitable PC found. Try changing your answers.");
    if (typeAnswer.Contains("laptop") && allLaptops.Count == 0)
        return Content("❌ No suitable Laptop found. Try changing your answers.");

    // حساب السكور
    object bestDevice = null;
    int bestScore = -1;
    int maxPossibleScore = 0;

    int GetScore(dynamic device)
    {
        double performanceScore = 0;
        double specsScore = 0;
        double featuresScore = 0;
        double priceScore = 0;

        if (device.GPU.ToLower().Contains("rtx")) performanceScore += 0.5;
        if (device.Description.ToLower().Contains("cooling")) performanceScore += 0.2;
        if (device.Processor.ToLower().Contains("i7") || device.Processor.ToLower().Contains("ryzen 7")) performanceScore += 0.3;

        if (device.RAM.ToLower().Contains("16")) specsScore += 0.4;
        if (device.Storage.ToLower().Contains("1tb")) specsScore += 0.4;
        else if (device.Storage.ToLower().Contains("512")) specsScore += 0.3;
        else if (device.Storage.ToLower().Contains("256")) specsScore += 0.2;

        if (device.Description.ToLower().Contains("touchscreen")) featuresScore += 0.3;
        if (device.Description.ToLower().Contains("backlit")) featuresScore += 0.2;
        if (device.Description.ToLower().Contains("pen")) featuresScore += 0.2;
        if (device.Description.ToLower().Contains("lightweight")) featuresScore += 0.3;

        if (int.TryParse(device.Price.ToString(), out int price))
        {
            if (price <= 350) priceScore = 1.0;
            else if (price <= 500) priceScore = 0.8;
            else if (price <= 750) priceScore = 0.5;
            else priceScore = 0.3;
        }

        maxPossibleScore = 100;
        double weighted = performanceScore * 40 + specsScore * 30 + featuresScore * 20 + priceScore * 10;
        return (int)Math.Round(weighted);
    }

    foreach (var l in allLaptops)
    {
        int s = GetScore(l);
        if (s > bestScore)
        {
            bestScore = s;
            bestDevice = l;
        }
    }

    foreach (var p in allPCs)
    {
        int s = GetScore(p);
        if (s > bestScore)
        {
            bestScore = s;
            bestDevice = p;
        }
    }

    ViewBag.MatchPercentage = maxPossibleScore > 0
        ? (int)Math.Round((double)bestScore / maxPossibleScore * 100)
        : 0;

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

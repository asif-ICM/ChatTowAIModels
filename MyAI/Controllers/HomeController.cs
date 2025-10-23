using AI.Application;
using Microsoft.AspNetCore.Mvc;
using MyAI.Models;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MyAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _apiKey;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _apiKey = config["GROQ_API_KEY"] ?? throw new InvalidOperationException("GROQ_API_KEY not found in configuration"); // reads from env vars
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Chat()
        {
            return View(new List<MyChatMessage>
        {
            new MyChatMessage
            {
                Message = "Hello! How can I help you today?",
                IsUserMessage = false,
                Timestamp = DateTime.Now
            }
        });
        }

        [HttpPost]
        public async Task<IActionResult> Chat(string question)
        {
            var _chatHistory= await new TwoDOdalsService(_apiKey).Chat(question);
          
            return View(_chatHistory);
        }
        public IActionResult OffensiveWord()
        {
            return View(new List<MyChatMessage>
        {
            new MyChatMessage
            {
                Message = "Hello! How can I help you today?",
                IsUserMessage = false,
                Timestamp = DateTime.Now
            }
        });
        }

        [HttpPost]
        public async Task<IActionResult> OffensiveWord(string question)
        {
            var _chatHistory= await new OffensiveWordService(_apiKey).Agent(question);
          
            return View(_chatHistory);
        }

        public IActionResult ChatBot()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}


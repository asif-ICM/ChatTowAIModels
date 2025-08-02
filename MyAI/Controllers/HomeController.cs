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
        private static List<MyChatMessage> _chatHistory = new List<MyChatMessage>
        {
            new MyChatMessage
            {
                Message = "Hello! How can I help you today?",
                IsUserMessage = false,
                Timestamp = DateTime.Now
            }
        };

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly string _apiKey;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _apiKey = _config["GROQ_API_KEY"]; // reads from env vars
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
            return View(_chatHistory);
        }

        [HttpPost]
        public async Task<IActionResult> Chat(string question)
        {
            string user = "gemma2-9b-it";
            string userSystemRole = "You will pretend to be a human and ask the query in a conversational way.";
            string agent = "llama-3.3-70b-versatile";
            string agentSystemRole = @"You are an AI system. Do NOT simulate being a human or use conversational language.
- Be terse, technical, objective.
- No small talk, pleasantries, or emojis.
- Ask only essential clarifying questions.
- Prefer short, declarative sentences; structured formatting when helpful.
- State uncertainty explicitly; do not invent sources.";
            bool isUserMessage = true;
            var answer = "";
            if (!string.IsNullOrEmpty(question))
            {
                _chatHistory.Add(new MyChatMessage
                {
                    Message = question,
                    IsUserMessage = isUserMessage,
                    Timestamp = DateTime.Now
                });
            }
            for (int i = 0; i < 10; i++)
            {
                isUserMessage = !isUserMessage;
                if (isUserMessage)
                    answer = await GetAnserByAIAsync(user, userSystemRole, question);
                else
                    answer = await GetAnserByAIAsync(agent, agentSystemRole, question);

                _chatHistory.Add(new MyChatMessage
                {
                    Message = answer,
                    IsUserMessage = isUserMessage,
                    Timestamp = DateTime.Now
                });
                Console.WriteLine(answer);
                question = answer;
            }

            return View(_chatHistory);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private async Task<string> GetAnserByAIAsync(string model, string system, string question)
        {

            var client = new ChatClient(
                model: model, // pick a Groq model ID
                credential: new ApiKeyCredential(_apiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.groq.com/openai/v1")
                }
                );
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(system),
                new UserChatMessage(question)
            };

            var reply = await client.CompleteChatAsync(messages);
            var anser = reply.Value.Content[0].Text;
            return anser;
        }
    }
}


using AI.Application;
using Microsoft.AspNetCore.Mvc;
using MyAI.Models;
using System.Text;
using System.Text.Json;

namespace MyAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatBotController : ControllerBase
    {
        private readonly ILogger<ChatBotController> _logger;
        private readonly string _apiKey;

        public ChatBotController(ILogger<ChatBotController> logger, IConfiguration config)
        {
            _logger = logger;
            _apiKey = config["GROQ_API_KEY"] ?? throw new InvalidOperationException("GROQ_API_KEY not found in configuration"); // Reusing your existing API key configuration
        }

        [HttpPost("stream")]
        public async Task<IActionResult> StreamChat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                // Set up Server-Sent Events headers
                Response.Headers["Content-Type"] = "text/event-stream";
                Response.Headers["Cache-Control"] = "no-cache";
                Response.Headers["Connection"] = "keep-alive";
                Response.Headers["Access-Control-Allow-Origin"] = "*";
                Response.Headers["Access-Control-Allow-Headers"] = "Cache-Control";

                // Create streaming service using your existing patterns
                var streamingService = new ChatBotStreamingService(_apiKey);
                
                // Stream the response
                await foreach (var chunk in streamingService.StreamChatAsync(request.Message))
                {
                    var json = JsonSerializer.Serialize(new { content = chunk, timestamp = DateTime.Now });
                    var data = $"data: {json}\n\n";
                    var bytes = Encoding.UTF8.GetBytes(data);
                    
                    await Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    await Response.Body.FlushAsync();
                }

                // Send completion signal
                var endData = "data: [DONE]\n\n";
                var endBytes = Encoding.UTF8.GetBytes(endData);
                await Response.Body.WriteAsync(endBytes, 0, endBytes.Length);
                await Response.Body.FlushAsync();

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in streaming chat");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                // Reuse your existing TwoDOdalsService for regular chat
                var chatHistory = await new TwoDOdalsService(_apiKey).Chat(request.Message);
                return Ok(chatHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("think")]
        public async Task<IActionResult> ThinkAndRespond([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                // Set up Server-Sent Events headers
                Response.Headers["Content-Type"] = "text/event-stream";
                Response.Headers["Cache-Control"] = "no-cache";
                Response.Headers["Connection"] = "keep-alive";
                Response.Headers["Access-Control-Allow-Origin"] = "*";
                Response.Headers["Access-Control-Allow-Headers"] = "Cache-Control";

                // Create thinking service for step-by-step analysis
                var thinkingService = new ChatBotThinkingService(_apiKey);
                
                // Stream the thinking process and response
                await foreach (var step in thinkingService.ThinkAndRespondAsync(request.Message))
                {
                    var json = JsonSerializer.Serialize(new { 
                        content = step.Content, 
                        type = step.Type,
                        timestamp = DateTime.Now 
                    });
                    var data = $"data: {json}\n\n";
                    var bytes = Encoding.UTF8.GetBytes(data);
                    
                    await Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    await Response.Body.FlushAsync();
                }

                // Send completion signal
                var endData = "data: [DONE]\n\n";
                var endBytes = Encoding.UTF8.GetBytes(endData);
                await Response.Body.WriteAsync(endBytes, 0, endBytes.Length);
                await Response.Body.FlushAsync();

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in thinking chat");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}

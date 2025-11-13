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

        [HttpPost("html")]
        public async Task<IActionResult> ChatHtml([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                // Use two-agent flow:
                // Agent 1: Get response from chatbot (via ChatBotHtmlService)
                // Agent 2: Format response as HTML with appropriate styling (via HtmlFormattingAgent)
                var htmlService = new ChatBotHtmlService(_apiKey);
                var htmlResponse = await htmlService.GetFormattedHtmlResponseAsync(request.Message);
                
                // Wrap the HTML response in a complete HTML document with styling
                var fullHtmlDocument = WrapHtmlResponse(request.Message, htmlResponse);
                
                return Content(fullHtmlDocument, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HTML chat");
                return StatusCode(500, "Internal server error");
            }
        }

        private string WrapHtmlResponse(string question, string htmlContent)
        {
            // The htmlContent from Agent 2 is already formatted HTML with inline styles
            // We just need to wrap it in a complete HTML document structure
            var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>ChatBot HTML Response</title>
    <style>
        body {{
            margin: 0;
            padding: 20px;
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
            min-height: 100vh;
        }}
        .container {{
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
        }}
        .question {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px;
            border-radius: 12px;
            margin-bottom: 20px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            font-weight: 600;
            font-size: 18px;
        }}
        .answer-wrapper {{
            background: #ffffff;
            padding: 25px;
            border-radius: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }}
        .timestamp {{
            font-size: 12px;
            color: #718096;
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px solid #e2e8f0;
            text-align: right;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""question"">
            <strong>Question:</strong> {System.Security.SecurityElement.Escape(question)}
        </div>
        <div class=""answer-wrapper"">
            {htmlContent}
            <div class=""timestamp"">Generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>
        </div>
    </div>
</body>
</html>";

            return html;
        }

    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}

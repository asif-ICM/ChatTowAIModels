using AI.Agents;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;

namespace AI.Application
{
    public class ChatBotThinkingService
    {
        private readonly string _apiKey;

        public ChatBotThinkingService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async IAsyncEnumerable<ThinkingStep> ThinkAndRespondAsync(string message)
        {
            // Step 1: Show thinking indicator
            yield return new ThinkingStep
            {
                Content = "🤔 Let me think about this...",
                Type = "thinking"
            };
            await Task.Delay(1000);

            // Step 2: Show analysis phase
            yield return new ThinkingStep
            {
                Content = "🔍 Analyzing your question...",
                Type = "analyzing"
            };
            await Task.Delay(800);

            // Step 3: Show todo/planning phase
            yield return new ThinkingStep
            {
                Content = "📝 Planning my response...",
                Type = "todo"
            };
            await Task.Delay(600);

            // Step 4: Get the actual AI response
            var client = new ChatClient(
                model: "llama-3.3-70b-versatile", // Reusing your existing model
                credential: new ApiKeyCredential(_apiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.groq.com/openai/v1"), // Reusing your existing endpoint
                }
            );

            var messages = new ChatMessage[]
            {
                new SystemChatMessage("You are a helpful AI assistant. Provide clear, detailed, and accurate responses. Think step by step and explain your reasoning."),
                new UserChatMessage(message)
            };

            var options = new ChatCompletionOptions();

            // Step 5: Stream the actual response
            yield return new ThinkingStep
            {
                Content = "💡 Here's my response:",
                Type = "response_start"
            };
            await Task.Delay(300);

            await foreach (var update in client.CompleteChatStreamingAsync(messages, options))
            {
                if (update.ContentUpdate?.Count > 0)
                {
                    var content = update.ContentUpdate[0].Text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        yield return new ThinkingStep
                        {
                            Content = content,
                            Type = "response"
                        };
                        // Add small delay to simulate realistic streaming
                        await Task.Delay(50);
                    }
                }
            }

            // Step 6: Show completion
            yield return new ThinkingStep
            {
                Content = "✅ Response complete!",
                Type = "complete"
            };
        }
    }

    public class ThinkingStep
    {
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}

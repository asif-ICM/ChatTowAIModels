using AI.Agents;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;

namespace AI.Application
{
    public class ChatBotStreamingService
    {
        private readonly string _apiKey;

        public ChatBotStreamingService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async IAsyncEnumerable<string> StreamChatAsync(string message)
        {
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
                new SystemChatMessage("You are a helpful AI assistant. Provide clear, concise, and accurate responses."),
                new UserChatMessage(message)
            };

            var options = new ChatCompletionOptions();

            await foreach (var update in client.CompleteChatStreamingAsync(messages, options))
            {
                if (update.ContentUpdate?.Count > 0)
                {
                    var content = update.ContentUpdate[0].Text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        // Add small delay to simulate realistic streaming
                        await Task.Delay(50);
                        yield return content;
                    }
                }
            }
        }
    }
}

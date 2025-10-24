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
        private readonly ContentSafetyFilter _safetyFilter;

        public ChatBotThinkingService(string apiKey)
        {
            _apiKey = apiKey;
            _safetyFilter = new ContentSafetyFilter();
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

            // Step 2: Safety check
            yield return new ThinkingStep
            {
                Content = "🛡️ Checking content safety...",
                Type = "analyzing"
            };
            await Task.Delay(500);

            // Check for inappropriate content
            var safetyResult = _safetyFilter.CheckContentSafety(message);
            if (!safetyResult.IsSafe)
            {
                yield return new ThinkingStep
                {
                    Content = "⚠️ I cannot respond to this request as it contains inappropriate content or potential legal risks.",
                    Type = "safety_warning"
                };
                yield return new ThinkingStep
                {
                    Content = "Please rephrase your question in a more appropriate way.",
                    Type = "safety_warning"
                };
                yield return new ThinkingStep
                {
                    Content = "✅ Safety check complete - Request blocked",
                    Type = "complete"
                };
                yield break;
            }

            // Step 3: Show analysis phase
            yield return new ThinkingStep
            {
                Content = "🔍 Analyzing your question...",
                Type = "analyzing"
            };
            await Task.Delay(800);

            // Step 4: Show todo/planning phase
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
                new SystemChatMessage(@"You are a helpful AI assistant with strict content safety guidelines. 
                - Provide clear, detailed, and accurate responses
                - Think step by step and explain your reasoning
                - NEVER provide content that is:
                  * Vulgar, offensive, or inappropriate
                  * Potentially illegal or harmful
                  * Discriminatory or biased
                  * Personal information or private data
                - If a question seems inappropriate, politely decline and suggest an alternative
                - Always maintain a professional and respectful tone"),
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

            string fullResponse = "";
            await foreach (var update in client.CompleteChatStreamingAsync(messages, options))
            {
                if (update.ContentUpdate?.Count > 0)
                {
                    var content = update.ContentUpdate[0].Text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        fullResponse += content;
                        
                        // Check if the accumulated response contains inappropriate content
                        var responseSafety = _safetyFilter.CheckContentSafety(fullResponse);
                        if (!responseSafety.IsSafe)
                        {
                            yield return new ThinkingStep
                            {
                                Content = "⚠️ I cannot continue this response as it may contain inappropriate content.",
                                Type = "safety_warning"
                            };
                            yield return new ThinkingStep
                            {
                                Content = "Please rephrase your question in a more appropriate way.",
                                Type = "safety_warning"
                            };
                            yield break;
                        }
                        
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

using MyAI.Models;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Agents
{
    internal class AIClient
    {
        private readonly string _apiKey;

        internal AIClient(string apiKey)
        {
            _apiKey = apiKey;
        }
        internal async Task<string> GetAnserByAIAsync(string model, string system, string question)
        {

            var client = new ChatClient(
                model: model, // pick a Groq model ID
                credential: new ApiKeyCredential(_apiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.groq.com/openai/v1"),

                }
                );
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(system),
                new UserChatMessage(question)
            };
            var options = new ChatCompletionOptions
            {
                //Tools = { locTool, weatherTool }  // <- tools go here
            };

            var reply = await client.CompleteChatAsync(messages, options);
            var anser = reply.Value.Content[0].Text;
            return anser;
        }

    }
}

using MyAI.Models;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AI.Agents
{
    internal class OffensiveWordAgent
    {
        private readonly string _apiKey;
        private AIClient aIClient;
        public OffensiveWordAgent(string apiKey)
        {
            aIClient = new AIClient(apiKey);
            this._apiKey = apiKey;
        }
        internal async Task<bool> IsOffensive(string question)
        {
            
            string model = "meta-llama/llama-4-maverick-17b-128e-instruct";
            var systemPrompt = """
You are an AI assistant. Given a person's initials, your task is to determine whether the initials
are potentially offensive. If they are offensive, respond with 'Yes'; otherwise, respond with 'No'.

Respond ONLY in JSON format:
{
    "answer": "Yes" // or "No"
}
""";


           

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(question)
            };

            var schema = """
{
  "type": "object",
  "properties": {
    "answer": {
      "type": "string",
      "enum": ["Yes", "No"]
    }
  },
  "required": ["answer"],
  "additionalProperties": false
}
""";





            var options = new ChatCompletionOptions
            {

                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("AnswerSchema", new BinaryData(schema)),
                Temperature = 0  // deterministic
            };

            var client = new ChatClient(
                model: model,
                credential: new ApiKeyCredential(_apiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.groq.com/openai/v1")
                });

            var result = await client.CompleteChatAsync(messages, options);
            Console.WriteLine(result);

            // Parse only the "answer" field from JSON:
            var json = result.Value.Content[0].Text;
            Console.WriteLine(json);

            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Console.WriteLine(parsed);

            var answerOnly = parsed["answer"];  // "Yes" or "No"
            if(answerOnly == "Yes")
                return true;
            return false;
        }
    }
}

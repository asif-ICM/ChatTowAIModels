using MyAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Agents
{
    internal class ChatOfTwoModels
    {
        private  AIClient aIClient;
        public ChatOfTwoModels(string apiKey)
        {
            aIClient = new AIClient(apiKey);
        }
        internal async Task<List<MyChatMessage>> ModelsAsync(string question)
        {
            List<MyChatMessage> _chatHistory = new List<MyChatMessage>();
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
                    answer = await aIClient.GetAnserByAIAsync(user, userSystemRole, question);
                else
                    answer = await aIClient.GetAnserByAIAsync(agent, agentSystemRole, question);

                _chatHistory.Add(new MyChatMessage
                {
                    Message = answer,
                    IsUserMessage = isUserMessage,
                    Timestamp = DateTime.Now
                });
                Console.WriteLine(answer);
                question = answer;
            }
            return _chatHistory;
        }
    }
}

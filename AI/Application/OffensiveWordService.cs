using AI.Agents;
using MyAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Application
{
    public class OffensiveWordService
    {
        private readonly OffensiveWordAgent agent;
        public OffensiveWordService(string apiKey)
        {
            agent = new OffensiveWordAgent(apiKey);
        }
        public async Task<List<MyChatMessage>> Agent(string Name)
        {
            List<MyChatMessage> _chatHistory = new List<MyChatMessage>();
            var offinsiceList = new List<string>();
            var existingInitilas = new List<string>();
            bool isoffinsice = false;
            string initias = "";
            _chatHistory.Add(new MyChatMessage
            {
                Message = Name,
                IsUserMessage = true,
                Timestamp = DateTime.Now
            });
            do
            {
                existingInitilas.Add(initias);
                existingInitilas.Add(initias);
                int initialsMaxLetters = 4;
                InitialsGenerator initialsGenerator = new InitialsGenerator(new List<string>(), offinsiceList);
                initias = initialsGenerator.GenerateUniqueInitialWithRandonLetters(Name, initialsMaxLetters);
                isoffinsice = await agent.IsOffensive(initias);
                _chatHistory.Add(new MyChatMessage
                {

                    Message = @"{initias} : "
,
                    IsUserMessage = false,
                    Timestamp = DateTime.Now
                });
            } while (isoffinsice);
            return _chatHistory;
        }
    }
}

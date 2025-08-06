using AI.Agents;
using MyAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Application
{
    public class TwoDOdalsService
    {
        private readonly ChatOfTwoModels chatOfTwoModels;
        public TwoDOdalsService(string apiKey)
        {
            chatOfTwoModels = new ChatOfTwoModels(apiKey);
        }
        public async Task<List<MyChatMessage>> Chat(string query)
        {
            return await chatOfTwoModels.ModelsAsync(query);
        }
    }
}

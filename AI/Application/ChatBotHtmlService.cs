using AI.Agents;
using MyAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI.Application
{
    public class ChatBotHtmlService
    {
        private readonly string _apiKey;

        public ChatBotHtmlService(string apiKey)
        {
            _apiKey = apiKey;
        }

        /// <summary>
        /// Gets the chatbot response (Agent 1) and then formats it as HTML (Agent 2)
        /// </summary>
        public async Task<string> GetFormattedHtmlResponseAsync(string question)
        {
            // Agent 1: Get the response from chatbot
            var chatHistory = await new TwoDOdalsService(_apiKey).Chat(question);
            
            // Get the last AI response (non-user message)
            var textResponse = chatHistory.LastOrDefault(m => !m.IsUserMessage)?.Message ?? "";
            
            if (string.IsNullOrEmpty(textResponse))
            {
                textResponse = "No response generated.";
            }

            // Agent 2: Format the response as HTML with appropriate styling
            var htmlFormattingService = new HtmlFormattingService(_apiKey);
            var htmlResponse = await htmlFormattingService.FormatResponseAsHtmlAsync(question, textResponse);

            return htmlResponse;
        }
    }
}


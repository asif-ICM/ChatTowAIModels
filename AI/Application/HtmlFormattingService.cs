using AI.Agents;
using System.Threading.Tasks;

namespace AI.Application
{
    public class HtmlFormattingService
    {
        private readonly string _apiKey;

        public HtmlFormattingService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> FormatResponseAsHtmlAsync(string question, string textResponse)
        {
            var formattingAgent = new HtmlFormattingAgent(_apiKey);
            return await formattingAgent.FormatAsHtmlAsync(question, textResponse);
        }
    }
}


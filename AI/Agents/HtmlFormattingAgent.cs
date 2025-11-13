using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Threading.Tasks;

namespace AI.Agents
{
    internal class HtmlFormattingAgent
    {
        private readonly string _apiKey;

        public HtmlFormattingAgent(string apiKey)
        {
            _apiKey = apiKey;
        }

        internal async Task<string> FormatAsHtmlAsync(string question, string textResponse)
        {
            var client = new ChatClient(
                model: "llama-3.3-70b-versatile",
                credential: new ApiKeyCredential(_apiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.groq.com/openai/v1"),
                }
            );

            var systemPrompt = @"You are an HTML formatting specialist. Your task is to convert plain text responses into beautifully formatted HTML with inline CSS styles.

Rules:
1. Analyze the content type (code, technical documentation, lists, narrative text, etc.)
2. Apply the most suitable HTML structure and styling based on the content
3. Use inline CSS only (no external stylesheets)
4. Style should be relevant to the content type:
   - Code content: Use dark theme with green/monospace styling
   - Technical content: Use blue/purple accents with structured layout
   - Lists: Format as proper <ul> or <ol> with styled bullets/numbers
   - Narrative: Use warm colors and readable fonts
   - Mixed content: Combine styles appropriately

5. Convert markdown to HTML:
   - Code blocks (```) -> <pre><code> with dark background
   - Inline code (`) -> <code> tags
   - Headers (# ## ###) -> <h1> <h2> <h3>
   - Lists (- * 1.) -> <ul> or <ol> with <li>
   - Bold (**text**) -> <strong>
   - Paragraphs: Wrap in <p> tags

6. Apply color schemes based on content:
   - Code: Dark (#2d3748) background, green (#68d391) text
   - Technical: Blue (#667eea) accents, light blue (#f8f9ff) background
   - General: Warm colors, beige (#fffaf0) background, orange accents
   - Lists: Clean white background with colored borders

7. Return ONLY the HTML content (not a full HTML document), just the formatted answer portion.
8. Make it visually appealing with gradients, shadows, and modern styling.
9. Ensure the styling matches the question and answer content context.

Return the formatted HTML with inline styles only.";

            var userMessage = $@"Question: {question}

Response to format:
{textResponse}

Please format this response as HTML with inline CSS that matches the content type and provides an appropriate visual presentation.";

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage)
            };

            var options = new ChatCompletionOptions();

            var reply = await client.CompleteChatAsync(messages, options);
            var htmlResponse = reply.Value.Content[0].Text;
            
            return htmlResponse;
        }
    }
}


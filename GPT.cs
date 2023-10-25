//using System.Threading.Tasks;
using OpenAI_API;

namespace CsCli {
    internal class GPT {
        public static OpenAIAPI GetAPI() {
            return new OpenAIAPI(); // replace with your OpenAI key
        }

        public static async Task<string> GPTInputAsync(OpenAIAPI api, string prompt) {
            // api.Chat.CreateConversation(new ChatRequest(){})
            var chat = api.Chat.CreateConversation();
            chat.AppendUserInput(prompt);
            string result = await chat.GetResponseFromChatbotAsync();
            return result.Trim();
        }

        public static async Task<string> CorrectErrorsAsync(OpenAIAPI api, string input) {
            return await GPTInputAsync(api, "Corrige les fautes d'orthographe de: " + input);
        }

        public static async Task<string> TranslateToENAsync(OpenAIAPI api, string input) {
            return await GPTInputAsync(api, "Translate the following to English: " + input);
        }
    }
}
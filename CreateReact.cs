using System.Diagnostics;

namespace CsCli {
    internal class CreateReact {
        public static async Task CreateReactProjectAsync(string name) {
            // note: no sanitization of name param
            var process = Process.Start("npx", "create-react-app " + name);
            await process.WaitForExitAsync();
            
            // TODO: open VS code (and discard it)
            Process.Start("codium", name);
        }
    }
}

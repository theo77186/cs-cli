using System.Diagnostics;
using OpenAI_API;
using TextCopy;

namespace CsCli { // Note: actual namespace depends on the project name.
    internal class Program {
        static OpenAIAPI? api = null;
        static void Main(string[] args) {
            if (args.Length > 0)
                ParseAndRun(args);
            else {
                string[] correct = {"-c"};
                ParseAndRun(correct);
            }
            while (true) {
                Console.WriteLine("Entrez la commande à executer:");
                string? input = Console.ReadLine();
                if (input == null) {
                    Console.WriteLine("unexpected end of file");
                    Environment.Exit(0);
                }
                ParseAndRun(input.Split(" "));
            }
        }

        static OpenAIAPI GetProgramAPI() {
            api ??= GPT.GetAPI(); // assign if api is null
            return api;
        }

        static Action<string[]> GetCommandFunction(string[] args) {
            string commandName = args.Length > 0 ? args[0].Trim() : "(invalid)";
            return commandName switch {
                "-c" => CorrectErrorsCommand,
                "-t" => TranslateCommand,
                "create" => CreateReactCommand,
                "clone" => CloneGitRepoCommand,
                "notes" => NoteDatabase.NotesCommand,
                _ => (string[] a) => { Console.WriteLine("unrecognized command: " + commandName); }
            };
        }

        static void ParseAndRun(string[] args) {
            Action<string[]> ac = GetCommandFunction(args);
            ac.Invoke(args[1..]);
            AskContinue();
        }

        static void AskContinue() {
            Console.WriteLine("continuer? (y/n)");
            string? answer = Console.ReadLine();
            if (answer == null || answer[..1].ToLower() == "n")
                Environment.Exit(0);
        }

        static void CorrectErrorsCommand(string[] args) {
            try {
                Console.WriteLine("Entrez la phrase à corriger: ");
                string? input = Console.ReadLine(); // may return null
                if (input == null) {
                    Console.Error.WriteLine("unexpected end of file");
                    return;
                }
                string result = GPT.CorrectErrorsAsync(GetProgramAPI(), input).Result;
                ClipboardService.SetText(result);
                Console.WriteLine(result);
            }
            catch (Exception e) {
                Console.WriteLine("Erreur: " + e.ToString());
            }
        }

        static void TranslateCommand(string[] args) {
            try {
                Console.WriteLine("Entrez la phrase à traduire: ");
                string? input = Console.ReadLine(); // may return null
                if (input == null) {
                    Console.Error.WriteLine("unexpected end of file");
                    return;
                }
                Console.WriteLine(GPT.TranslateToENAsync(GetProgramAPI(), input).Result);
            }
            catch (Exception e) {
                Console.WriteLine("Erreur: " + e.ToString());
            }
        }

        static void CreateReactCommand(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("argument manquant <project-name>");
                return;
            }

            try {
                CreateReact.CreateReactProjectAsync(args[0]).Wait();
            }
            catch (Exception e) {
                Console.WriteLine("Erreur: " + e.ToString());
            }
        }

        static void CloneGitRepoCommand(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("argument manquant <repo-url>");
                return;
            }
            try {
                var process = Process.Start("git", "clone " + args[0]);
                process.WaitForExit();
            }
            catch (Exception e) {
                Console.WriteLine("Erreur: " + e.ToString());
            }
        }
    }
}

using Npgsql;
using NpgsqlTypes;
using System.Collections;

namespace CsCli {
    class NoteDatabase {
        static NpgsqlDataSource? connection;
        public static NpgsqlDataSource CreateConnection() {
            string connectionString = Environment.GetEnvironmentVariable("PG_CONNECTION_STRING") ??
                "Host=localhost;Username=user1;Password=;Database=katas";
            connection ??= NpgsqlDataSource.Create(connectionString) ?? throw new Exception("failed to connect");
            return connection;
        }

        static async Task<Note[]> ListFromReader(NpgsqlDataReader re) {
            List<Note> notes = new();
            while (await re.ReadAsync()) {
                Note note = new() {
                    id = re.GetInt32(0),
                    date = re.GetDateTime(1),
                    content = re.GetString(2),
                    category = re.GetString(3)
                };
                notes.Add(note);
            }
            return notes.ToArray();
        }

        public static async Task<Note[]> ListNotesAsync(NpgsqlDataSource pg) {
            var command = pg.CreateCommand("SELECT id, date, content, category FROM notes ORDER BY ID");
            var reader = await command.ExecuteReaderAsync();
            return await ListFromReader(reader);
        }

        // returns null if id not found
        // FIXME: lots of duplicated code with above
        public static async Task<Note?> GetNoteAsync(NpgsqlDataSource pg, int id) {
            var command = pg.CreateCommand("SELECT id, date, content, category FROM notes WHERE id = @id");
            command.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
            var reader = await command.ExecuteReaderAsync();
            Note[] notes = await ListFromReader(reader);
            return notes.Length > 0 ? notes[0] : null;
        }

        public static async Task<int> CreateNoteAsync(NpgsqlDataSource pg, Note note) {
            var command = pg.CreateCommand("INSERT INTO notes (date, content, category) VALUES (@date, @content, @category) RETURNING id");
            command.Parameters.AddWithValue("@date", NpgsqlDbType.Date, note.date);
            command.Parameters.AddWithValue("@content", NpgsqlDbType.Text, note.content);
            command.Parameters.AddWithValue("@category", NpgsqlDbType.Text, note.category);
            var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) {
                throw new Exception("no line inserted");
            }
            return reader.GetInt32(0);
        }

        public static void NotesCommand(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("missing subcommand <create|delete|get|list>");
                return;
            }
            Action<string[]>? command = null;
            command = args[0] switch {
                "create" => NotesCreateCommand,
                "get" => NotesGetCommand,
                "list" => NotesListCommand,
                _ => (string[] s) => { Console.WriteLine("commande non reconnue " + args[0]); }
            };
            command(args[1..]);
        }

        public static void NotesCreateCommand(string[] args) {
            try {
                Console.WriteLine("Entrez le contenu:");
                string content = Console.ReadLine() ?? "";
                Console.WriteLine("Entrez la catégorie:");
                string category = Console.ReadLine() ?? "";
                Note note = Note.GetNoteToday(content, category);
                int id = CreateNoteAsync(CreateConnection(), note).Result;
                Console.WriteLine("note créé avec id " + id);
            }
            catch (Exception e) {
                Console.WriteLine("erreur: " + e.ToString());
            }
        }

        public static void NotesGetCommand(string[] args) {
            try {
                if (args.Length == 0) {
                    Console.WriteLine("id manquant");
                }
                int id = Int32.Parse(args[0]);
                Note? note = GetNoteAsync(CreateConnection(), id).Result;
                if (note == null) {
                    Console.WriteLine("Notes d'identifiant " + args[0] + " introuvable");
                }
                else {
                    Console.WriteLine(note);
                }
            }
            catch (Exception e) {
                Console.WriteLine("erreur: " + e.ToString());
            }
        }

        public static void NotesListCommand(string[] args) {
            try {
                Note[] notes = ListNotesAsync(CreateConnection()).Result;
                if (notes.Length == 0)
                    Console.WriteLine("pas de note");
                Array.ForEach(notes, (Note n) => { Console.WriteLine(n.ToString()); });
            }
            catch (Exception e) {
                Console.WriteLine("erreur: " + e.ToString());
            }
        }
    }
}
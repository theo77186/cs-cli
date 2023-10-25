namespace CsCli {
    class Note {
        public int id;
        public DateTime date;
        public string content = "";
        public string category = "";

        public static Note GetNoteToday(string content, string category) {
            return new Note() {
                id = 0,
                date = DateTime.Now,
                content = content,
                category = category.Trim()
            };
        }

        public override string ToString() {
            string date8601 = date.ToString("O")[..10];
            string result = $"Note {id} ({date8601})\n";
            result += $"Catégorie: {category}\n";
            result += content;
            return result;
        }
    }
}
using LiteDB;
using RunMate.Models;

namespace RunMate.Services
{
    public static class CommandRepositoryService
    {
        private static readonly string DbPath = "commands.db";

        public static List<CommandEntry> LoadCommands()
        {
            using var db = new LiteDatabase(DbPath);
            var col = db.GetCollection<CommandEntry>("commands");
            return col.Query().ToList();
        }

        public static void SaveCommands(IEnumerable<CommandEntry> commands)
        {
            using var db = new LiteDatabase(DbPath);
            var col = db.GetCollection<CommandEntry>("commands");
            col.DeleteAll();
            col.InsertBulk(commands);
        }
    }
}

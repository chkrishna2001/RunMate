using LiteDB;

namespace RunMate.Models
{
    public class ShellGroupEntity
    {
        [BsonId]
        public string ShellType { get; set; }

        public List<TreeNodeEntity> Children { get; set; } = new();
    }
}

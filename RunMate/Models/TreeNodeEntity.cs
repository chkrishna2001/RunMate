namespace RunMate.Models
{
    public class TreeNodeEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public bool IsCommand { get; set; }
        public string CommandText { get; set; }
        public string ShellType { get; set; }
        public List<TreeNodeEntity> Children { get; set; } = new();
    }
}

using System.Collections.ObjectModel;
using LiteDB;
using RunMate.Helpers;
using RunMate.Models;

namespace RunMate.Services
{
    public static class CommandRepositoryLiteDbService
    {
        private const string DatabasePath = "RunMate.db";

        public static void SaveTree(IEnumerable<TreeNode> rootNodes)
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<ShellGroupEntity>("shell_groups");

            col.DeleteAll();

            foreach (var rootNode in rootNodes)
            {
                var shellGroup = new ShellGroupEntity
                {
                    ShellType = rootNode.Name,
                    Children = rootNode.Children?.Select(TreeNodeMapper.MapToEntity).ToList() ?? new List<TreeNodeEntity>()
                };

                col.Upsert(shellGroup);
            }
        }

        public static List<TreeNode> LoadTree()
        {
            using var db = new LiteDatabase(DatabasePath);
            var col = db.GetCollection<ShellGroupEntity>("shell_groups");

            var shellGroups = col.FindAll();

            return shellGroups.Select(group => new TreeNode
            {
                Name = group.ShellType,
                OriginalName = group.ShellType,
                ShellType = group.ShellType,
                IsCommand = false,
                CommandText = null,
                Children = new ObservableCollection<TreeNode>(group.Children?.Select(TreeNodeMapper.MapToModel) ?? Enumerable.Empty<TreeNode>())
            }).ToList();
        }
    }
}

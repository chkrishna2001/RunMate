using System.Collections.ObjectModel;
using RunMate.Models;

namespace RunMate.Helpers
{
    public static class TreeNodeMapper
    {
        public static TreeNodeEntity MapToEntity(TreeNode node)
        {
            return new TreeNodeEntity
            {
                Id = Guid.NewGuid(),
                Name = node.Name,
                IsCommand = node.IsCommand,
                CommandText = node.CommandText,
                ShellType = node.ShellType,
                Children = node.Children?.Select(MapToEntity).ToList() ?? new List<TreeNodeEntity>()
            };
        }

        public static TreeNode MapToModel(TreeNodeEntity entity)
        {
            return new TreeNode
            {
                Id = entity.Id,
                Name = entity.Name,
                OriginalName = entity.Name,
                IsCommand = entity.IsCommand,
                CommandText = entity.CommandText,
                ShellType = entity.ShellType,
                Children = new ObservableCollection<TreeNode>(entity.Children?.Select(MapToModel) ?? Enumerable.Empty<TreeNode>())
            };
        }
    }
}

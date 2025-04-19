using System.Windows;
using System.Windows.Controls;

namespace RunMate.Helpers
{
    public static class TreeViewHelper
    {
        public static readonly DependencyProperty BindableSelectedItemProperty =
            DependencyProperty.RegisterAttached("BindableSelectedItem",
                typeof(object),
                typeof(TreeViewHelper),
                new UIPropertyMetadata(null, OnBindableSelectedItemChanged));

        public static object GetBindableSelectedItem(DependencyObject obj)
        {
            return obj.GetValue(BindableSelectedItemProperty);
        }

        public static void SetBindableSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(BindableSelectedItemProperty, value);
        }

        private static void OnBindableSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
                treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
            }
        }

        private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView treeView)
            {
                SetBindableSelectedItem(treeView, e.NewValue);
            }
        }
    }
}

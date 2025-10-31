using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Xiletrade.UI.Avalonia.Util.Extensions;

public static class TreeViewItemExtensions
{
    public static int GetDepth(this TreeViewItem item)
    {
        TreeViewItem parent;
        while ((parent = GetParent(item)) != null)
        {
            return parent.GetDepth() + 1;
        }
        return 0;
    }

    private static TreeViewItem GetParent(TreeViewItem item)
    {
        var parent = item.GetVisualParent();

        while (parent is not null and not TreeViewItem and not TreeView)
        {
            parent = parent.GetVisualParent();
        }

        return parent as TreeViewItem;
    }
}
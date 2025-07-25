﻿using System.Windows.Controls;
using System.Windows.Media;

namespace Xiletrade.UI.WPF.Util.Extensions;

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
        var parent = VisualTreeHelper.GetParent(item);

        while (!(parent is TreeViewItem || parent is TreeView))
        {
            if (parent == null) return null;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as TreeViewItem;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace pcclient
{
    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            FrameworkElement elem = item;
            while (elem.Parent != null)
            {
                var tvi = elem.Parent as TreeViewItem;
                if (null != tvi)
                    return tvi.GetDepth() + 1;
                elem = elem.Parent as FrameworkElement;
            }
            return 0;
        }
    } 
}

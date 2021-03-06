﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CustomWindow;
using RenmeiLib;

namespace pcclient
{
    /// <summary>
    /// Interaction logic for ShowFriendInfo.xaml
    /// </summary>
    public partial class ShowFriendInfo : EssentialWindow
    {

        protected override Decorator GetWindowButtonsPlaceholder()
        {
            return WindowButtonsPlaceholder;
        }

        public ShowFriendInfo( User us )
        {
            InitializeComponent();
            UserTitleBar.DataContext = us;
        }

        private void Header_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
        private void Menu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Left)
            //{
            //    curItemRelated2ContextMenu = ((ListBoxItem)CommentsListBox.ContainerFromElement((Image)sender)).Content as Comment;
            //    if (curItemRelated2ContextMenu == null) return;
            //    Image image = sender as Image;
            //    ContextMenu cm = PrepareTweetContextMenuTemplate();

            //    cm.PlacementTarget = image;
            //    cm.IsOpen = true;

            //}
        }
        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Width + e.HorizontalChange > 10)
                this.Width += e.HorizontalChange;
            if (this.Height + e.VerticalChange > 10)
                this.Height += e.VerticalChange;
        }

    }
}

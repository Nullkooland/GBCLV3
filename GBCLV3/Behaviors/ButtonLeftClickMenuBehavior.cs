using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GBCLV3.Behaviors
{
    static class ButtonLeftClickMenuBehavior
    {
        #region Attached Properties

        public static readonly DependencyProperty ShowMenuOnLeftClickProperty = DependencyProperty.RegisterAttached(
            "ShowMenuOnLeftClick", typeof(bool),
            typeof(WindowBehavior), new PropertyMetadata(false, OnShowMenuOnLeftClickChanged));

        #endregion

        #region Getters & Setters

        public static bool GetShowMenuOnLeftClick(DependencyObject element)
            => (bool)element.GetValue(ShowMenuOnLeftClickProperty);

        public static void SetShowMenuOnLeftClick(DependencyObject element, bool value)
            => element.SetValue(ShowMenuOnLeftClickProperty, value);

        #endregion

        #region AttachedPropertyChanged Handlers

        private static void OnShowMenuOnLeftClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as Button;

            if (e.OldValue is true)
            {
                button.Click -= OnButtonClicked;
            }

            if (e.NewValue is true)
            {
                button.Click += OnButtonClicked;
            }
        }

        #endregion

        #region Control Event Handlers

        private static void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                if (button.ContextMenu.DataContext == null)
                {
                    button.ContextMenu.SetBinding(
                        FrameworkElement.DataContextProperty,
                        new Binding { Source = button.DataContext });
                }

                button.ContextMenu.IsOpen = true;
            }
        }

        #endregion
    }
}

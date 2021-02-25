using System.Windows;
using System.Windows.Controls;

namespace GBCLV3.Behaviors
{
    public static class WindowBehavior
    {
        #region Attached Properties

        public static readonly DependencyProperty CaptionButtonStateProperty = DependencyProperty.RegisterAttached(
            "CaptionButtonState", typeof(WindowState?),
            typeof(WindowBehavior), new PropertyMetadata(null, OnButtonStateChanged));

        public static readonly DependencyProperty IsCaptionCloseButtonProperty = DependencyProperty.RegisterAttached(
            "IsCaptionCloseButton", typeof(bool), typeof(WindowBehavior),
            new PropertyMetadata(false, OnIsCloseButtonChanged));

        #endregion

        #region Getters & Setters

        public static WindowState? GetCaptionButtonState(DependencyObject element)
            => (WindowState?)element.GetValue(CaptionButtonStateProperty);

        public static void SetCaptionButtonState(DependencyObject element, WindowState? value)
            => element.SetValue(CaptionButtonStateProperty, value);


        public static bool GetIsCaptionCloseButton(DependencyObject element)
            => (bool)element.GetValue(IsCaptionCloseButtonProperty);

        public static void SetIsCaptionCloseButton(DependencyObject element, bool value)
            => element.SetValue(IsCaptionCloseButtonProperty, value);

        #endregion

        #region AttachedPropertyChanged Handlers

        private static void OnButtonStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as Button;

            if (e.OldValue is WindowState)
            {
                button.Click -= OnStateButtonClicked;
            }

            if (e.NewValue is WindowState)
            {
                button.Click += OnStateButtonClicked;
            }
        }

        private static void OnIsCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as Button;

            if (e.OldValue is true)
            {
                button.Click -= OnCloseButtonClicked;
            }

            if (e.NewValue is true)
            {
                button.Click += OnCloseButtonClicked;
            }
        }

        #endregion

        #region Control Event Handlers

        private static void OnStateButtonClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as DependencyObject;
            var window = Window.GetWindow(button);
            var state = GetCaptionButtonState(button);

            if (window != null && state != null)
            {
                window.WindowState = state.Value;
            }
        }

        private static void OnCloseButtonClicked(object sender, RoutedEventArgs e)
            => Window.GetWindow(sender as DependencyObject)?.Close();

        #endregion
    }
}

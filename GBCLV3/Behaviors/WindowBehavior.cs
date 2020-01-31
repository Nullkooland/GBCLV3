using GBCLV3.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace GBCLV3.Behaviors
{
    static class WindowBehavior
    {
        #region Attached Properties

        public static readonly DependencyProperty IsBlurBehindProperty = DependencyProperty.RegisterAttached(
            "IsBlurBehind", typeof(bool),
            typeof(WindowBehavior), new PropertyMetadata(false, OnIsBlurBehindChanged));

        public static readonly DependencyProperty CaptionButtonStateProperty = DependencyProperty.RegisterAttached(
            "CaptionButtonState", typeof(WindowState?),
            typeof(WindowBehavior), new PropertyMetadata(null, OnButtonStateChanged));

        public static readonly DependencyProperty IsCaptionCloseButtonProperty = DependencyProperty.RegisterAttached(
            "IsCaptionCloseButton", typeof(bool), typeof(WindowBehavior),
            new PropertyMetadata(false, OnIsCloseButtonChanged));

        #endregion

        #region Getters & Setters

        public static bool GetIsBlurBehind(DependencyObject element)
            => (bool)element.GetValue(IsBlurBehindProperty);

        public static void SetIsBlurBehind(DependencyObject element, bool value)
            => element.SetValue(IsBlurBehindProperty, value);

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

        private static void OnIsBlurBehindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;

            if (e.OldValue is true)
            {
                window.Loaded -= OnWindowLoaded;
            }

            if (e.NewValue is true)
            {
                window.Loaded += OnWindowLoaded;
            }
        }

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

        private static void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(sender as Window).Handle;
            NativeUtil.EnableBlur(handle);
        }

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

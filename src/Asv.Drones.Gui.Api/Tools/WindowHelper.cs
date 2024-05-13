using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Asv.Drones.Gui.Api
{
    public class WindowHelper
    {
        public static readonly AttachedProperty<bool> EnableDragProperty =
            AvaloniaProperty.RegisterAttached<WindowHelper, Control, bool>("EnableDrag");

        public static readonly AttachedProperty<bool> DoubleTappedWindowStateProperty =
            AvaloniaProperty.RegisterAttached<WindowHelper, Control, bool>("DoubleTappedWindowState");

        public static readonly AttachedProperty<bool> IgnoreDragProperty =
            AvaloniaProperty.RegisterAttached<WindowHelper, Control, bool>("IgnoreDrag");

        static WindowHelper()
        {
            EnableDragProperty.Changed.Subscribe(OnChangedEnableDrag);
            DoubleTappedWindowStateProperty.Changed.Subscribe(OnChangedDoubleClickWindowState);
        }

        #region IgnoreDragProperty

        public static void SetIgnoreDrag(AvaloniaObject element, bool commandValue)
        {
            element.SetValue(IgnoreDragProperty, commandValue);
        }

        public static bool GetIgnoreDrag(AvaloniaObject element)
        {
            return element.GetValue(IgnoreDragProperty);
        }

        #endregion

        #region DoubleTappedWindowStateProperty

        private static void OnChangedDoubleClickWindowState(AvaloniaPropertyChangedEventArgs<bool> source)
        {
            if (source.Sender is InputElement uiElement)
            {
                if (source.OldValue == false && source.NewValue.Value)
                {
                    uiElement.DoubleTapped += DoubleTappedHandler;
                }
                else
                {
                    uiElement.PointerPressed -= DoubleTappedHandler;
                }
            }
        }

        private static void DoubleTappedHandler(object? sender, RoutedEventArgs e)
        {
            if (sender is not Visual uiElement) return;
            if (uiElement is AvaloniaObject avalonia)
            {
                if (GetIgnoreDrag(avalonia)) return;
            }

            var parent = uiElement;
            var avoidInfiniteLoop = 0;
            // Search up the visual tree to find the first parent window.
            while (parent is Window == false)
            {
                parent = parent.GetVisualParent();
                avoidInfiniteLoop++;
                if (avoidInfiniteLoop == 1000)
                {
                    // Something is wrong - we could not find the parent window.
                    return;
                }
            }

            var window = parent as Window;
            window.WindowState = window.WindowState switch
            {
                WindowState.Normal => WindowState.Maximized,
                WindowState.Minimized => WindowState.Maximized,
                WindowState.Maximized => WindowState.Normal,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static void SetDoubleTappedWindowState(AvaloniaObject element, bool commandValue)
        {
            element.SetValue(DoubleTappedWindowStateProperty, commandValue);
        }

        public static bool GetDoubleTappedWindowState(AvaloniaObject element)
        {
            return element.GetValue(DoubleTappedWindowStateProperty);
        }

        #endregion

        #region EnableDrag

        private static void OnChangedEnableDrag(AvaloniaPropertyChangedEventArgs<bool> source)
        {
            if (source.Sender is IInputElement uiElement)
            {
                if (source.OldValue == false && source.NewValue.Value)
                {
                    uiElement.PointerPressed += MouseDownHandler;
                }
                else
                {
                    uiElement.PointerPressed -= MouseDownHandler;
                }
            }
        }

        private static void MouseDownHandler(object sender, PointerPressedEventArgs e)
        {
            if (sender is not Visual uiElement) return;
            var parent = uiElement;
            var avoidInfiniteLoop = 0;
            // Search up the visual tree to find the first parent window.
            while (parent is Window == false)
            {
                parent = parent.GetVisualParent();
                avoidInfiniteLoop++;
                if (avoidInfiniteLoop == 1000)
                {
                    // Something is wrong - we could not find the parent window.
                    return;
                }
            }

            var window = parent as Window;
            window.BeginMoveDrag(e);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="CommandProperty"/>.
        /// </summary>
        public static void SetEnableDrag(AvaloniaObject element, bool commandValue)
        {
            element.SetValue(EnableDragProperty, commandValue);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="CommandProperty"/>.
        /// </summary>
        public static bool GetEnableDrag(AvaloniaObject element)
        {
            return element.GetValue(EnableDragProperty);
        }

        #endregion
    }
}
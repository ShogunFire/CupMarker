using CupMarker.ViewModels;
using SharpVectors.Dom.Svg;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;


namespace CupMarker.Views
{
    public partial class CupControl : UserControl
    {
        private bool isDragging;
        private Line? activeLine;

        private CupControlViewModel? viewModel; // I know it's bad but the alternative was complex and for this small project I think it's ok.
        public CupControl()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.DataContextChanged += CupControl_DataContextChanged;

        }
        private void CupControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = DataContext as CupControlViewModel;
        }

        private void Line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            activeLine = sender as Line;
            Mouse.Capture(activeLine);
        }

        private void Line_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && activeLine != null)
            {
                var pos = e.GetPosition((IInputElement)activeLine.Parent);
                if(pos.Y < 0 || pos.Y > PreviewCanvas.ActualHeight)
                {
                    return;
                }



                if (activeLine.Tag.ToString() == "FirstLine")
                    viewModel.Y1 = pos.Y;
                else
                    viewModel.Y2 = pos.Y;
            }
        }

        private void Line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            Mouse.Capture(null);
        }

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            viewModel.CanvasHeight = e.NewSize.Height;
        }


        private void BarcodeInput_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null)
                tb.SelectAll();
        }

        private void BarcodeInput_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;

            // If the textbox is not yet focused, focus it and prevent the click from placing the cursor
            if (tb != null && !tb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                tb.Focus();
            }
        }
    }
}
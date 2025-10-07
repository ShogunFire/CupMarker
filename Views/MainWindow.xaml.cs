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
    public partial class MainWindow : Window
    {
        private bool isDragging;
        private Line? activeLine;
        MainViewModel viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.viewModel = viewModel;
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
    }
}
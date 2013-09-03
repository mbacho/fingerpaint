using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Phone.Shell;

namespace FingerPaint.views
{
    public partial class HomePage : PhoneApplicationPage
    {
        private List<SolidColorBrush> colors;
        public HomePage()
        {
            InitializeComponent();

            colors = new List<SolidColorBrush>() { 
                new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Blue), 
                new SolidColorBrush(Colors.Brown), new SolidColorBrush(Colors.Cyan), 
                new SolidColorBrush(Colors.DarkGray), new SolidColorBrush(Colors.Gray), 
                new SolidColorBrush(Colors.Green) ,new SolidColorBrush(Colors.LightGray),
                new SolidColorBrush(Colors.Magenta) ,new SolidColorBrush(Colors.Orange),
                new SolidColorBrush(Colors.Purple) ,new SolidColorBrush(Colors.Red),
                new SolidColorBrush(Colors.Yellow) 
            };
            lst.DataContext = colors;
            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);
        }

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rct.Stroke = (SolidColorBrush)((ListBox)sender).SelectedItem;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            PhoneApplicationService.Current.State["color"] = lst.SelectedItem;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (PhoneApplicationService.Current.State.ContainsKey("color"))
                lst.SelectedItem = PhoneApplicationService.Current.State["color"];
        }

        private void rct_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (StylusPoint p in e.StylusDevice.GetStylusPoints(canvas))
            {
                paintShape((SolidColorBrush)lst.SelectedItem, p, new Ellipse());
            }
        }

        private void paintShape(Brush color, StylusPoint p, Shape shape)
        {
            shape.SetValue(Canvas.LeftProperty, p.X);
            shape.SetValue(Canvas.TopProperty, p.Y);
            shape.Opacity = p.PressureFactor;
            shape.Width = 20d;
            shape.Height = 20d;
            shape.IsHitTestVisible = false;
            shape.Stroke = shape.Fill = rct.Stroke;
            canvas.Children.Add(shape);
        }

        private bool pointInRect(Point p)
        {
            return true;

            Point origin = new Point(rct.Clip.Bounds.X, rct.Clip.Bounds.Y);
            double height = rct.Height, width = rct.Width;
            double h_temp = p.X - origin.X, w_temp = p.Y - origin.Y;
            return (h_temp > 0) && (w_temp > 0) && (h_temp < height) && (w_temp < width);
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            WriteableBitmap img = new WriteableBitmap(canvas, null);
            img.Invalidate();
            string imgName = "test.jpg";
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream isofs = iso.OpenFile(imgName, System.IO.FileMode.OpenOrCreate);
                img.SaveJpeg(isofs, img.PixelWidth, img.PixelHeight, 0, 100);
                isofs.Seek(0, System.IO.SeekOrigin.Begin);
                MediaLibrary lib = new MediaLibrary();
                Picture pic = lib.SavePicture(imgName, isofs);
                isofs.Close();
                iso.DeleteFile(imgName);
            }
        }

        private void mnuClear_Click(object sender, EventArgs e)
        {
            canvas.Children.Clear();
            canvas.Children.Add(rct);
        }

        private void mnuSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/views/SettingsPage.xaml", UriKind.Relative));
        }
    }
}
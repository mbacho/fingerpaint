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
using System.Windows.Data;

namespace FingerPaint.views
{
    public partial class HomePage : PhoneApplicationPage
    {
        private List<SolidColorBrush> colors;

        private int point;
        private bool hasPressure;
        private SettingsPage.SHAPES shape;
        private IsolatedStorageSettings sets = IsolatedStorageSettings.ApplicationSettings;

        public HomePage()
        {
            InitializeComponent();

            colors = new List<SolidColorBrush>() { 
                 new SolidColorBrush(Colors.Cyan), new SolidColorBrush(Colors.Blue), new SolidColorBrush(Colors.Purple) ,
                new SolidColorBrush(Colors.Brown),new SolidColorBrush(Colors.Red),
                new SolidColorBrush(Colors.Green) ,
                new SolidColorBrush(Colors.Magenta) ,new SolidColorBrush(Colors.Orange),
                new SolidColorBrush(Colors.Yellow) ,
                new SolidColorBrush(Colors.White), 
                new SolidColorBrush(Colors.LightGray),new SolidColorBrush(Colors.DarkGray), new SolidColorBrush(Colors.Gray),
                new SolidColorBrush(Colors.Black)
            };
            lstColor.DataContext = colors;
            lstColor.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);

        }

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rct.Stroke = (SolidColorBrush)((ListBox)sender).SelectedItem;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            sets[SettingsPage.PREF_COLOR] = lstColor.SelectedIndex;
            sets[SettingsPage.PREF_POINT] = point;
            sets[SettingsPage.PREF_PRESSURE] = hasPressure;
            sets[SettingsPage.PREF_SHAPE] = shape;
            sets.Save();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (sets.Contains(SettingsPage.PREF_POINT)) { point = (int)sets[SettingsPage.PREF_POINT]; }
            if (sets.Contains(SettingsPage.PREF_PRESSURE)) { hasPressure = (bool)sets[SettingsPage.PREF_PRESSURE]; }
            if (sets.Contains(SettingsPage.PREF_SHAPE)) { shape = (SettingsPage.SHAPES)sets[SettingsPage.PREF_SHAPE]; }
            if (sets.Contains(SettingsPage.PREF_COLOR)) { lstColor.SelectedIndex = (int)sets[SettingsPage.PREF_COLOR]; }
        }

        private void rct_MouseMove(object sender, MouseEventArgs e)
        {
            Shape s = null;
            int norm = (int)point; int change = 10;

            switch (shape)
            {
                case SettingsPage.SHAPES.CIRCLE: s = new Ellipse { Width = norm, Height = norm }; break;
                case SettingsPage.SHAPES.OVAL_H: s = new Ellipse { Width = norm - change, Height = norm }; break;
                case SettingsPage.SHAPES.OVAL_W: s = new Ellipse { Width = norm, Height = norm - change }; break;
                case SettingsPage.SHAPES.RECTANGLE_H: s = new Rectangle { Width = norm - change, Height = norm }; break;
                case SettingsPage.SHAPES.RECTANGLE_W: s = new Rectangle { Width = norm, Height = norm - change }; break;
                case SettingsPage.SHAPES.SQUARE: s = new Rectangle { Width = norm, Height = norm }; break;
                default: break;
            }
            foreach (StylusPoint p in e.StylusDevice.GetStylusPoints(canvas))
            {
                paintShape((SolidColorBrush)lstColor.SelectedItem, p, s);
            }
        }

        private void paintShape(Brush color, StylusPoint p, Shape shape)
        {
            shape.SetValue(Canvas.LeftProperty, p.X - shape.Width / 2);
            shape.SetValue(Canvas.TopProperty, p.Y - shape.Height / 2);
            if (hasPressure)
                shape.Opacity = p.PressureFactor;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("pressure =" + p.PressureFactor);
#endif
            shape.IsHitTestVisible = false;
            shape.Stroke = shape.Fill = rct.Stroke;
            canvas.Children.Add(shape);
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            WriteableBitmap img = new WriteableBitmap(canvas, null);
            img.Invalidate();
            string imgName = "fingerpaint.jpg";
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

    public class VisibleSelectedConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ContentPresenter)
            {
                ContentPresenter content = (ContentPresenter)value;
                if (content.Content is SolidColorBrush && content.Content.Equals(content.Content))
                    return System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
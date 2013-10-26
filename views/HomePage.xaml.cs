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
using Coding4Fun.Toolkit.Controls;

namespace FingerPaint.views
{
    public partial class HomePage : PhoneApplicationPage
    {
        private int point;
        private bool hasPressure;
        private IsolatedStorageSettings sets = IsolatedStorageSettings.ApplicationSettings;

        public readonly static string PREF_POINT = "point";
        public readonly static string PREF_SHAPE = "shape";
        public readonly static string PREF_PRESSURE = "pressure";
        public readonly static string PREF_COLOR_A = "color_alpha";
        public readonly static string PREF_COLOR_R = "color_red";
        public readonly static string PREF_COLOR_G = "color_green";
        public readonly static string PREF_COLOR_B = "color_blue";
        private SHAPES shape;

        public enum SHAPES
        {
            CIRCLE, OVAL_W, OVAL_H, SQUARE, RECTANGLE_W, RECTANGLE_H
        };

        public List<Shape> shapeList;

        public HomePage()
        {
            InitializeComponent();
            SolidColorBrush scb = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
            Thickness m = new Thickness { Bottom = 10, Left = 10, Right = 10, Top = 10 };
            shapeList = new List<Shape>() { 
                new Ellipse{Width=70, Height=70, Fill=scb,Stroke=scb, Margin=m},
                new Ellipse{Width=70, Height=50, Fill=scb,Stroke=scb, Margin=m},
                new Ellipse{Width=50, Height=70, Fill=scb,Stroke=scb, Margin=m},
                new Rectangle{Width=70, Height=70, Fill=scb,Stroke=scb, Margin=m},
                new Rectangle{Width=70, Height=50, Fill=scb,Stroke=scb, Margin=m},
                new Rectangle{Width=50, Height=70, Fill=scb,Stroke=scb, Margin=m}
            };
            lst.DataContext = shapeList;
            lst.SelectionChanged += (sender, args) => { sets[PREF_SHAPE] = lst.SelectedIndex; sets.Save(); rct.Stroke = ((Shape)lst.SelectedValue).Stroke; };
            tgl.Checked += (sender,args) => { sets[PREF_PRESSURE] = tgl.IsChecked;sets.Save(); };
            sld.ValueChanged += (sender,args) => { sets[PREF_POINT] = (int)sld.Value;sets.Save(); };
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            sets[PREF_COLOR_R] = colPicker.Color.R;
            sets[PREF_COLOR_G] = colPicker.Color.G;
            sets[PREF_COLOR_B] = colPicker.Color.B;
            sets[PREF_COLOR_A] = colPicker.Color.A;

            sets[PREF_POINT] = point;
            sets[PREF_PRESSURE] = hasPressure;
            sets[PREF_SHAPE] = shape;
            sets.Save();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (sets.Contains(PREF_POINT)) { point = (int)sets[PREF_POINT]; }
            if (sets.Contains(PREF_PRESSURE)) { hasPressure = (bool)sets[PREF_PRESSURE]; }
            if (sets.Contains(PREF_SHAPE)) { shape = (SHAPES)sets[PREF_SHAPE]; }
            //if (sets.Contains(SettingsPage.PREF_COLOR)) { lstColor.SelectedIndex = (int)sets[SettingsPage.PREF_COLOR]; }
            //if (sets.Contains(PREF_COLOR_R)) { colPicker.Color.R = colPicker.Color.R; }
            //if (sets.Contains(PREF_COLOR_G)) { colPicker.Color.G = colPicker.Color.G; }
            //if (sets.Contains(PREF_COLOR_B)) { colPicker.Color.B = colPicker.Color.B; }
            //if (sets.Contains(PREF_COLOR_A)) { colPicker.Color.A = colPicker.Color.A; }
        }

        private void rct_MouseMove(object sender, MouseEventArgs e)
        {
            Shape s = null;
            int norm = (int)point; int change = 10;

            switch (shape)
            {
                case SHAPES.CIRCLE: s = new Ellipse { Width = norm, Height = norm }; break;
                case SHAPES.OVAL_H: s = new Ellipse { Width = norm - change, Height = norm }; break;
                case SHAPES.OVAL_W: s = new Ellipse { Width = norm, Height = norm - change }; break;
                case SHAPES.RECTANGLE_H: s = new Rectangle { Width = norm - change, Height = norm }; break;
                case SHAPES.RECTANGLE_W: s = new Rectangle { Width = norm, Height = norm - change }; break;
                case SHAPES.SQUARE: s = new Rectangle { Width = norm, Height = norm }; break;
                default: break;
            }
            foreach (StylusPoint p in e.StylusDevice.GetStylusPoints(canvas))
            {
                paintShape(colPicker.SolidColorBrush, p, s);
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

        private void colPicker_ColorChanged(object sender, Color color)
        {
            SolidColorBrush brush = ((ColorBaseControl)sender).SolidColorBrush;
            rct.Stroke = brush;
        }

        private void btnSetts_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Visibility vis = (stckSett.Visibility == System.Windows.Visibility.Collapsed) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
                                     visOpp = (vis == System.Windows.Visibility.Collapsed) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            stckSett.Visibility = vis;
            colSlider.Visibility = visOpp;
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

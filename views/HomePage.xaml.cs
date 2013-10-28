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
        private IsolatedStorageSettings sets = IsolatedStorageSettings.ApplicationSettings;

        public readonly static string PREF_POINT = "point";
        public readonly static string PREF_SHAPE = "shape";
        //public readonly static string PREF_PRESSURE = "pressure";
        public readonly static string PREF_COLOR_A = "color_alpha";
        public readonly static string PREF_COLOR_R = "color_red";
        public readonly static string PREF_COLOR_G = "color_green";
        public readonly static string PREF_COLOR_B = "color_blue";

        public enum SHAPES
        {
            CIRCLE, OVAL_W, OVAL_H, SQUARE, RECTANGLE_W, RECTANGLE_H
        };

        public List<Shape> shapeList;

        public HomePage()
        {
            InitializeComponent();
            Thickness m = new Thickness { Bottom = 10, Left = 10, Right = 10, Top = 10 };
            shapeList = new List<Shape>() { 
                new Ellipse { Width = 70, Height = 70, Margin = m },
                new Ellipse{Width=70, Height=50, Margin=m,},
                new Ellipse{Width=50, Height=70, Margin=m},
                new Rectangle{Width=70, Height=70, Margin=m},
                new Rectangle{Width=70, Height=50, Margin=m},
                new Rectangle{Width=50, Height=70, Margin=m,}
            };
            Binding b = new Binding
            {
                Source = colPicker,
                Path = new PropertyPath("SolidColorBrush")
            };
            foreach (Shape i in shapeList)
            {
                BindingOperations.SetBinding(i, Ellipse.StrokeProperty, b);
                BindingOperations.SetBinding(i, Ellipse.FillProperty, b);
            }
            BindingOperations.SetBinding(sld, Slider.ForegroundProperty, b);
            
            lst.DataContext = shapeList;
            lst.SelectionChanged += (sender, args) => { sets[PREF_SHAPE] = lst.SelectedIndex; sets.Save(); rct.Stroke = ((Shape)lst.SelectedValue).Stroke; };
            //tgl.Checked += (sender, args) => { sets[PREF_PRESSURE] = tgl.IsChecked; sets.Save(); };
            sld.ValueChanged += (sender, args) => { sets[PREF_POINT] = (int)sld.Value; sets.Save(); };
            loadSetts();
        }
        private void saveSetts()
        {
            sets[PREF_COLOR_R] = colPicker.Color.R;
            sets[PREF_COLOR_G] = colPicker.Color.G;
            sets[PREF_COLOR_B] = colPicker.Color.B;
            sets[PREF_COLOR_A] = colPicker.Color.A;

            sets[PREF_POINT] = (int)sld.Value;
            //sets[PREF_PRESSURE] = tgl.hasPressure;
            sets[PREF_SHAPE] = lst.SelectedIndex;
            sets.Save();
        }
        private void loadSetts()
        {
            try
            {
                Color col = new Color
                {
                    R = byte.Parse(sets[PREF_COLOR_R].ToString()),
                    G = byte.Parse(sets[PREF_COLOR_G].ToString()),
                    B = byte.Parse(sets[PREF_COLOR_B].ToString()),
                    A = byte.Parse(sets[PREF_COLOR_A].ToString()),
                };
                colSlider.Color = colPicker.Color = col;
                //tgl.IsChecked = (bool)sets[PREF_PRESSURE]; 
                lst.SelectedIndex = (int)sets[PREF_SHAPE];
                sld.Value = (int)sets[PREF_POINT];
            }catch(Exception ex){
#if DEBUG
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
#endif
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (stckSett.Visibility == System.Windows.Visibility.Collapsed)
            {
                base.OnBackKeyPress(e);
            }
            else
            {
                e.Cancel = true;
                toggleSettings();
            }
        }
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            saveSetts();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            loadSetts();
        }

        private void rct_MouseMove(object sender, MouseEventArgs e)
        {
            Shape s = null;
            int norm = (int)sets[PREF_POINT]; int change = 10;
            SHAPES shape = (SHAPES)sets[PREF_SHAPE];
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
            SolidColorBrush scb = new SolidColorBrush(new Color());
            foreach (StylusPoint p in e.StylusDevice.GetStylusPoints(canvas))
            {
                paintShape(scb, p, s);
            }
        }

        private void paintShape(Brush color, StylusPoint p, Shape shape)
        {
            shape.SetValue(Canvas.LeftProperty, p.X - shape.Width / 2);
            shape.SetValue(Canvas.TopProperty, p.Y - shape.Height / 2);
            //if (hasPressure)
            //    shape.Opacity = p.PressureFactor;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("pressure =" + p.PressureFactor);
#endif
            shape.IsHitTestVisible = false;
            shape.Stroke = shape.Fill = new SolidColorBrush(new Color
            {
                R = byte.Parse(sets[PREF_COLOR_R].ToString()),
                G = byte.Parse(sets[PREF_COLOR_G].ToString()),
                B = byte.Parse(sets[PREF_COLOR_B].ToString()),
                A = byte.Parse(sets[PREF_COLOR_A].ToString()),
            });
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
            ToastPrompt tp = new ToastPrompt
            {
                Message = "Picture saved!",
                FontSize = 30
            };
            tp.Show();
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            TextBlock tb = new TextBlock
            {
                Text = "Have fun with your finger and alot of colors, but don't make yourself dirty\n\nBy "
                + Coding4Fun.Toolkit.Controls.Common.PhoneHelper.GetAppAttribute("Author")
                + "\n\nVersion " + Coding4Fun.Toolkit.Controls.Common.PhoneHelper.GetAppAttribute("Version"),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.Wrap,
            };
            AboutPrompt abt = new AboutPrompt
            {
                VersionNumber = "",
                Body = tb
            };
            abt.Show();
        }

        private void mnuClear_Click(object sender, EventArgs e)
        {
            canvas.Children.Clear();
            canvas.Children.Add(rct);
        }

        private void colPicker_ColorChanged(object sender, Color color)
        {
            SolidColorBrush brush = ((ColorBaseControl)sender).SolidColorBrush;
            rct.Stroke = brush;
            saveSetts();
        }

        private void btnSetts_Click(object sender, RoutedEventArgs e)
        {
            toggleSettings();
        }

        private void toggleSettings()
        {
            System.Windows.Visibility vis = (stckSett.Visibility == System.Windows.Visibility.Collapsed) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
                                     visOpp = (vis == System.Windows.Visibility.Collapsed) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            stckSett.Visibility = vis;
            colSlider.Visibility = visOpp;
            imgSett.Source = (vis == System.Windows.Visibility.Visible) ?
                new BitmapImage(new Uri("/images/settings.dark.png", UriKind.RelativeOrAbsolute)) :
                new BitmapImage(new Uri("/images/settings.png", UriKind.RelativeOrAbsolute));
        }
        private void imgSett_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            toggleSettings();
        }
    }
}

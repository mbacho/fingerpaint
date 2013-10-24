using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace FingerPaint.views
{

    public partial class SettingsPage : PhoneApplicationPage
    {
        public readonly static string PREF_POINT = "point";
        public readonly static string PREF_SHAPE = "shape";
        public readonly static string PREF_PRESSURE = "pressure";
        public readonly static string PREF_COLOR = "color";
        public enum SHAPES
        {
            CIRCLE, OVAL_W, OVAL_H, SQUARE, RECTANGLE_W, RECTANGLE_H
        };
        public List<Shape> shapeList;
        private IsolatedStorageSettings sets = IsolatedStorageSettings.ApplicationSettings;

        public SettingsPage()
        {
            InitializeComponent();
            SolidColorBrush scb = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"];
            Thickness m = new Thickness { Bottom = 10, Left = 10, Right = 10, Top = 10 };
            shapeList = new List<Shape>() { 
                new Ellipse{Width=70,Height=70, Fill=scb,Stroke=scb,Margin=m},
                new Ellipse{Width=70,Height=50, Fill=scb,Stroke=scb,Margin=m},
                new Ellipse{Width=50,Height=70, Fill=scb,Stroke=scb,Margin=m},
                new Rectangle{Width=70,Height=70, Fill=scb,Stroke=scb,Margin=m},
                new Rectangle{Width=70,Height=50, Fill=scb,Stroke=scb,Margin=m},
                new Rectangle{Width=50,Height=70, Fill=scb,Stroke=scb,Margin=m}
            };
            lst.DataContext = shapeList;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            sets[PREF_POINT] = (int)sld.Value;
            sets[PREF_PRESSURE] = tgl.IsChecked;
            sets[PREF_SHAPE] = lst.SelectedIndex;
            sets.Save();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (sets.Contains(PREF_POINT)) { sld.Value = (int)sets[SettingsPage.PREF_POINT]; }
            if (sets.Contains(PREF_PRESSURE)) { tgl.IsChecked = (bool)sets[SettingsPage.PREF_PRESSURE]; }
            if (sets.Contains(PREF_SHAPE)) { lst.SelectedIndex = (int)sets[SettingsPage.PREF_SHAPE]; }

        }
    }

}
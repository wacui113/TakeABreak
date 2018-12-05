using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TakeABreak
{
    /// <summary>
    /// Interaction logic for ToggleSwitchButton.xaml
    /// </summary>
    public partial class ToggleSwitchButton : UserControl
    {
        public event EventHandler ToggleChanged;

        Thickness positionOff = new Thickness(-39, 0, 0, 0);
        Thickness positionOn = new Thickness(0, 0, -39, 0);

        SolidColorBrush backgrOff = new SolidColorBrush(System.Windows.Media.Color.FromRgb(209, 209, 209));
        SolidColorBrush backgrOn = new SolidColorBrush(System.Windows.Media.Color.FromRgb(63, 224, 47));

        SolidColorBrush thicknessOffBrush = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        SolidColorBrush thicknessOnBrush = new SolidColorBrush(Color.FromRgb(245, 245, 245));

        // Gets or Set ToggleSwitchStatus is ON or OFF
        private bool toggled = false;
        public bool Toggled
        {
            get
            {
                return toggled;
            }

            set
            {
                toggled = value;
                f_Toggled();
            }
        }

        public ToggleSwitchButton()
        {
            InitializeComponent();

            rectBack.Fill = backgrOff;
            toggled = false;
            ellDot.Margin = positionOff;

            ellDot.Stroke = thicknessOffBrush;
            rectBack.Stroke = thicknessOffBrush;
        }

        private void f_Toggled()
        {
            if (!toggled)
            {
                rectBack.Fill = backgrOn;
                toggled = true;
                ellDot.Margin = positionOn;

                ellDot.Stroke = thicknessOnBrush;
                rectBack.Stroke = thicknessOnBrush;
            }
            else
            {
                rectBack.Fill = backgrOff;
                toggled = false;
                ellDot.Margin = positionOff;

                ellDot.Stroke = thicknessOffBrush;
                rectBack.Stroke = thicknessOffBrush;
            }
            ToggleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void rectBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            f_Toggled();
            //ToggleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ellDot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            f_Toggled();
            //ToggleChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ANT.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BottomSlideMenu : ContentView
    {
        public BottomSlideMenu()
        {
            InitializeComponent();
        }

        #region Bindable properties
        public double HeightMenu
        {
            get { return (double)GetValue(HeightMenuProperty); }
            set { SetValue(HeightMenuProperty, value); }
        }

        public static readonly BindableProperty HeightMenuProperty =
            BindableProperty.Create(
                propertyName: nameof(HeightMenu),
                returnType: typeof(double),
                declaringType: typeof(BottomSlideMenu),
                defaultValue: 25.0,
                defaultBindingMode: BindingMode.TwoWay);

        public Color BackgroundColorMenu
        {
            get { return (Color)GetValue(BackgroundColorMenuProperty); }
            set { SetValue(BackgroundColorMenuProperty, value); }
        }

        public static readonly BindableProperty BackgroundColorMenuProperty =
            BindableProperty.Create(
                propertyName: nameof(BackgroundColorMenu),
                returnType: typeof(Color),
                declaringType: typeof(BottomSlideMenu),
                defaultValue: Color.CadetBlue,
                defaultBindingMode: BindingMode.OneWay);

        public double CornerRadiusMenu
        {
            get { return (double)GetValue(CornerRadiusMenuProperty); }
            set { SetValue(CornerRadiusMenuProperty, value); }
        }

        public static readonly BindableProperty CornerRadiusMenuProperty =
            BindableProperty.Create(
                propertyName: nameof(CornerRadiusMenu),
                returnType: typeof(double),
                declaringType: typeof(BottomSlideMenu),
                defaultValue: 0.0,
                defaultBindingMode: BindingMode.TwoWay);

        public View ViewContent
        {
            get { return (View)GetValue(ViewContentProperty); }
            set { SetValue(ViewContentProperty, value); }
        }

        public static readonly BindableProperty ViewContentProperty =
            BindableProperty.Create(
                propertyName: nameof(ViewContent),
                returnType: typeof(View),
                declaringType: typeof(BottomSlideMenu),
                defaultBindingMode: BindingMode.TwoWay);

        public View MenuContent
        {
            get { return (View)GetValue(MenuContentProperty); }
            set { SetValue(MenuContentProperty, value); }
        }

        public static readonly BindableProperty MenuContentProperty =
            BindableProperty.Create(
                propertyName: nameof(MenuContent),
                returnType: typeof(View),
                declaringType: typeof(BottomSlideMenu),
                defaultBindingMode: BindingMode.TwoWay);


        #endregion

        public async Task ScrollHappenedAsync(double y)
        {
            if (y > 0)
                await Menu.TranslateTo(0, _bottomSlideMenuControl.Bounds.Bottom, length:500, easing: Easing.Linear);
            else
                await Menu.TranslateTo(0, _bottomSlideMenuControl.Bounds.Top, easing: Easing.Linear);
        }
    }
}
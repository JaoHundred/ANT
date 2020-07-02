using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ANT.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CatalogueButton : ContentView
    {
        public CatalogueButton()
        {
            InitializeComponent();
        }

        #region bindable properties
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(
                propertyName: nameof(Command),
                returnType: typeof(ICommand),
                declaringType: typeof(CatalogueButton),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(
                propertyName: nameof(CommandParameter),
                returnType: typeof(object),
                declaringType: typeof(CatalogueButton),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create(
                propertyName: nameof(FontFamily),
                returnType: typeof(string),
                declaringType: typeof(CatalogueButton),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public string TextIcon
        {
            get { return (string)GetValue(TextIconProperty); }
            set { SetValue(TextIconProperty, value); }
        }

        public static readonly BindableProperty TextIconProperty =
            BindableProperty.Create(
                propertyName: nameof(TextIcon),
                returnType: typeof(string),
                declaringType: typeof(CatalogueButton),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly BindableProperty ButtonTextProperty =
            BindableProperty.Create(
                propertyName: nameof(ButtonText),
                returnType: typeof(string),
                declaringType: typeof(CatalogueButton),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);
        #endregion

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (sender is VisualElement ve)
            {
                await ve.ScaleTo(0.95, 100, easing: Easing.Linear);
                await ve.ScaleTo(1, 100, easing: Easing.Linear);
            }
        }
    }
}
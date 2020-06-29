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
    public partial class MenuImageButton : ContentView
    {
        public MenuImageButton()
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
                declaringType: typeof(MenuImageButton),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public Style ImageStyle
        {
            get { return (Style)GetValue(ImageStyleProperty); }
            set { SetValue(ImageStyleProperty, value); }
        }

        public static readonly BindableProperty ImageStyleProperty =
            BindableProperty.Create(
                propertyName: nameof(ImageStyle),
                returnType: typeof(Style),
                declaringType: typeof(MenuImageButton),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public string ImageText
        {
            get { return (string)GetValue(ImageTextProperty); }
            set { SetValue(ImageTextProperty, value); }
        }

        public static readonly BindableProperty ImageTextProperty =
            BindableProperty.Create(
                propertyName: nameof(ImageText),
                returnType: typeof(string),
                declaringType: typeof(MenuImageButton),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        //TODO:tentar ver mais tarde se essa é a melhor forma de se chamar eventos
        //e também de desinscreve-lo
        public event EventHandler Tapped;

        #endregion

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
            => Tapped?.Invoke(this, EventArgs.Empty);

    }
}
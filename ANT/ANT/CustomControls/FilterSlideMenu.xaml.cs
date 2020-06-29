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
    public partial class FilterSlideMenu : ContentView
    {
        public FilterSlideMenu()
        {
            InitializeComponent();
        }

        #region bindable properties
        public View PageContent
        {
            get { return (View)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        public static readonly BindableProperty PageContentProperty =
            BindableProperty.Create(
                propertyName: nameof(PageContent),
                returnType: typeof(View),
                declaringType: typeof(FilterSlideMenu),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public View Filters
        {
            get { return (View)GetValue(FiltersProperty); }
            set { SetValue(FiltersProperty, value); }
        }

        public static readonly BindableProperty FiltersProperty =
            BindableProperty.Create(
                propertyName: nameof(Filters),
                returnType: typeof(View),
                declaringType: typeof(FilterSlideMenu),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public View FilterMenu
        {
            get { return (View)GetValue(FilterMenuProperty); }
            set { SetValue(FilterMenuProperty, value); }
        }

        public static readonly BindableProperty FilterMenuProperty =
            BindableProperty.Create(
                propertyName: nameof(FilterMenu),
                returnType: typeof(View),
                declaringType: typeof(FilterSlideMenu),
                defaultValue: default,
                defaultBindingMode: BindingMode.OneWay);

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set
            {
                if (value)
                {
                    SlideMenu.TranslateTo(0, _filterSlideMenu.Bounds.Top, easing: Easing.Linear);
                }
                else
                {
                    SlideMenu.TranslateTo(0, _filterSlideMenu.Bounds.Bottom, easing: Easing.Linear);
                }

                SetValue(IsOpenProperty, value);
            }
        }

        public static readonly BindableProperty IsOpenProperty =
            BindableProperty.Create(
                propertyName: nameof(IsOpen),
                returnType: typeof(bool),
                declaringType: typeof(FilterSlideMenu),
                defaultValue: default,
                defaultBindingMode: BindingMode.TwoWay);
        #endregion

        private void CloseSlideMenuTapped(object sender, EventArgs e)
            => IsOpen = false;
    }
}
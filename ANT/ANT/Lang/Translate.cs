using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ANT.Lang
{
    // You exclude the 'Extension' suffix when using in XAML
    [ContentProperty("Text")]
    public class Translate : IMarkupExtension
    {
        readonly CultureInfo ci = null;
        const string ResourceId = "ANT.Lang.Lang";

        static readonly Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(
            () => new ResourceManager(ResourceId, IntrospectionExtensions.GetTypeInfo(typeof(Translate)).Assembly));

        public string Text { get; set; }

        public Translate()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                ILocalize service = DependencyService.Get<ILocalize>();
                ci = service.GetCurrentCultureInfo();
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return string.Empty;

            var translation = ResMgr.Value.GetString(Text, ci);
            if (translation == null)
            {
#if DEBUG
                throw new ArgumentException(
                    string.Format("Key '{0}' was not found in resources '{1}' for culture '{2}'.", Text, ResourceId, ci.Name),
                    "Text");
#else
                translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
#endif
            }
            return translation;
        }
    }
}


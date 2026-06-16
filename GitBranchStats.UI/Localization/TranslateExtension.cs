using System;
using System.Windows.Data;
using System.Windows.Markup;
using GitBranchStats.Core.Localization;

namespace GitBranchStats.UI.Localization
{
    /// <summary>
    /// XAML markup extension that binds a property to a localized string.
    /// Usage: Text="{loc:Translate Main_Refresh}". The binding updates live whenever
    /// the selected language changes.
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class TranslateExtension : MarkupExtension
    {
        public TranslateExtension() { }

        public TranslateExtension(string key)
        {
            Key = key;
        }

        [ConstructorArgument("key")]
        public string Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding($"[{Key}]")
            {
                Source = LocalizationService.Instance,
                Mode = BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}

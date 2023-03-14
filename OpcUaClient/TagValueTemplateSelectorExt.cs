using System;
using System.Windows.Controls;
using System.Windows.Markup;

namespace OpcUaClient
{
    [MarkupExtensionReturnType(typeof(DataTemplateSelector))]
    public class TagValueTemplateSelectorExt : MarkupExtension
    {
        public TagValueDataTemplateDictionary TemplateDictionary { get; set; }
        public string Property { get; set; }

        public TagValueTemplateSelectorExt()
        {
        }

        public TagValueTemplateSelectorExt(TagValueDataTemplateDictionary template)
        {
            TemplateDictionary = template;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new TagValueTemplateProvider(this);
        }
    }
}

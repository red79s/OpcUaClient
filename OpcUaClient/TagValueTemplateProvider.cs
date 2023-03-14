using System.Windows;
using System.Windows.Controls;

namespace OpcUaClient
{
    public class TagValueTemplateProvider : DataTemplateSelector
    {
        private TagValueTemplateSelectorExt TemplateSelector { get; set; }

        public TagValueTemplateProvider(TagValueTemplateSelectorExt extension)
            : base()
        {
            TemplateSelector = extension;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;
            if (container as FrameworkElement != null)
            {
                var tagInfo = item as TagInfo;
                if (tagInfo == null)
                    return null;

                if (tagInfo.DataType == typeof (System.Boolean))
                {
                    if (TemplateSelector.TemplateDictionary.ContainsKey("boolTemplate"))
                    {
                        return TemplateSelector.TemplateDictionary["boolTemplate"];
                    }
                }
                else if (tagInfo.DataType == typeof (System.Int16) || 
                    tagInfo.DataType == typeof (System.Int32) || 
                    tagInfo.DataType == typeof(System.Int64))
                {
                    if (TemplateSelector.TemplateDictionary.ContainsKey("intTemplate"))
                    {
                        return TemplateSelector.TemplateDictionary["intTemplate"];
                    }
                }
                else
                {
                    if (TemplateSelector.TemplateDictionary.ContainsKey("defaultTemplate"))
                    {
                        return TemplateSelector.TemplateDictionary["defaultTemplate"];
                    }
                }
            }
            return null;
        }
    }
}

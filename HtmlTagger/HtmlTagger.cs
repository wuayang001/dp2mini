using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Html;

namespace DigitalPlatform
{
    public class HtmlTagger : TagBuilder
    {
        // 附加的数据
        public object Param { get; set; }

        public static HtmlTagger Create(string tagName)
        {
            return new HtmlTagger(tagName);
        }

        public static HtmlTagger Create(string tagName,
            string innerText)
        {
            return new HtmlTagger(tagName).SetInnerText(innerText);
        }


        public HtmlTagger(string tagName) : base(tagName)
        {
        }

        //
        // 摘要:
        //     Adds a CSS class to the list of CSS classes in the tag.
        //
        // 参数:
        //   value:
        //     The CSS class to add.
        public new HtmlTagger AddCssClass(string value)
        {
            base.AddCssClass(value);
            return this;
        }

        //
        // 摘要:
        //     Generates a sanitized ID attribute for the tag by using the specified name.
        //
        // 参数:
        //   name:
        //     The name to use to generate an ID attribute.
        public new HtmlTagger GenerateId(string name)
        {
            base.GenerateId(name);
            return this;
        }

        //
        // 摘要:
        //     Adds a new attribute to the tag.
        //
        // 参数:
        //   key:
        //     The key for the attribute.
        //
        //   value:
        //     The value of the attribute.
        public new HtmlTagger MergeAttribute(string key, string value)
        {
            base.MergeAttribute(key, value);
            return this;
        }

        //
        // 摘要:
        //     Adds a new attribute or optionally replaces an existing attribute in the opening
        //     tag.
        //
        // 参数:
        //   key:
        //     The key for the attribute.
        //
        //   value:
        //     The value of the attribute.
        //
        //   replaceExisting:
        //     true to replace an existing attribute if an attribute exists that has the specified
        //     key value, or false to leave the original attribute unchanged.
        public new HtmlTagger MergeAttribute(string key,
            string value,
            bool replaceExisting)
        {
            base.MergeAttribute(key, value, replaceExisting);
            return this;
        }

        //
        // 摘要:
        //     Adds new attributes to the tag.
        //
        // 参数:
        //   attributes:
        //     The collection of attributes to add.
        //
        // 类型参数:
        //   TKey:
        //     The type of the key object.
        //
        //   TValue:
        //     The type of the value object.
        public new HtmlTagger MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes)
        {
            base.MergeAttributes(attributes);
            return this;
        }

        //
        // 摘要:
        //     Adds new attributes or optionally replaces existing attributes in the tag.
        //
        // 参数:
        //   attributes:
        //     The collection of attributes to add or replace.
        //
        //   replaceExisting:
        //     For each attribute in attributes, true to replace the attribute if an attribute
        //     already exists that has the same key, or false to leave the original attribute
        //     unchanged.
        //
        // 类型参数:
        //   TKey:
        //     The type of the key object.
        //
        //   TValue:
        //     The type of the value object.
        public new HtmlTagger MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting)
        {
            base.MergeAttributes(attributes, replaceExisting);
            return this;
        }

        //
        // 摘要:
        //     Sets the System.Web.Mvc.TagBuilder.InnerHtml property of the element to an HTML-encoded
        //     version of the specified string.
        //
        // 参数:
        //   innerText:
        //     The string to HTML-encode.
        public new HtmlTagger SetInnerText(string innerText)
        {
            base.SetInnerText(innerText);
            return this;
        }

        public HtmlTagger SetInnerHtml(string html)
        {
            //if (_children.Count > 0)
            //    throw new Exception("AddChild() 以后就不允许使用 SetInnerHtml() 了");
            this.InnerHtml = html;
            return this;
        }

        private List<HtmlTagger> _children = new List<HtmlTagger>();

        public HtmlTagger AddChild(HtmlTagger tagger)
        {
            //if (string.IsNullOrEmpty(this.InnerHtml) == false)
            //    throw new Exception("SetInnerHtml() 以后就不允许使用 AddChild() 了");

            _children.Add(tagger);
            return this;
        }


        string GetChildrenHtml()
        {
            StringBuilder result = new StringBuilder();
            foreach (var child in _children)
            {
                result.Append(child.ToString());
            }
            return result.ToString();
        }

        private void AppendAttributes(StringBuilder sb)
        {
            foreach (KeyValuePair<string, string> attribute in Attributes)
            {
                string key = attribute.Key;
                if (!string.Equals(key, "id", StringComparison.Ordinal) || !string.IsNullOrEmpty(attribute.Value))
                {
                    string value = HttpUtility.HtmlAttributeEncode(attribute.Value);
                    sb.Append(' ').Append(key).Append("=\"")
                        .Append(value)
                        .Append('"');
                }
            }
        }

        //
        // 摘要:
        //     Renders the element as a System.Web.Mvc.TagRenderMode.Normal element.
        public override string ToString()
        {
            return ToString(TagRenderMode.Normal);
        }


        //
        // 摘要:
        //     Renders the HTML tag by using the specified render mode.
        //
        // 参数:
        //   renderMode:
        //     The render mode.
        //
        // 返回结果:
        //     The rendered HTML tag.
        public new string ToString(TagRenderMode renderMode)
        {
            StringBuilder stringBuilder = new StringBuilder();
            switch (renderMode)
            {
                case TagRenderMode.StartTag:
                    stringBuilder.Append('<').Append(TagName);
                    AppendAttributes(stringBuilder);
                    stringBuilder.Append('>');
                    break;
                case TagRenderMode.EndTag:
                    stringBuilder.Append("</").Append(TagName).Append('>');
                    break;
                case TagRenderMode.SelfClosing:
                    stringBuilder.Append('<').Append(TagName);
                    AppendAttributes(stringBuilder);
                    stringBuilder.Append(" />");
                    break;
                default:
                    stringBuilder.Append('<').Append(TagName);
                    AppendAttributes(stringBuilder);
                    stringBuilder.Append('>');

                    if (_children.Count > 0)
                        stringBuilder.Append(GetChildrenHtml());

                    stringBuilder.Append(InnerHtml);
                    stringBuilder.Append("</")
                        .Append(TagName)
                        .Append('>');
                    break;
            }

            return stringBuilder.ToString();
        }
    }
}

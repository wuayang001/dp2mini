using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DigitalPlatform
{
#if REMOVED
    public static class NestedTagger
    {
        private static List<HtmlTagger> _taggers = new List<HtmlTagger>();


        public static HtmlTagger Push(string tagName)
        {
            var tagger = new HtmlTagger(tagName);
            _taggers.Add(tagger);
            return tagger;
        }

        public static HtmlTagger Pop()
        {
            if (_taggers.Count == 0)
                throw new Exception("没有可 Pop 的对象");

            var result = _taggers[_taggers.Count - 1];
            _taggers.RemoveAt(_taggers.Count - 1);
            return result;
        }


        public static new string ToString()
        {
            var result = _output(_taggers);
            _taggers.Clear();
            return result;
        }

        private static string _output(List<HtmlTagger> taggers)
        {
            StringBuilder result = new StringBuilder();
            var tagger = taggers.FirstOrDefault();
            if (tagger != null) 
            {
                result.Append(tagger.ToString(TagRenderMode.StartTag));
                var rest = taggers.Skip(1).ToList();
                result.Append(_output(rest));
                result.Append(tagger.ToString(TagRenderMode.EndTag));
            }

            return result.ToString();
        }
    }
#endif
}

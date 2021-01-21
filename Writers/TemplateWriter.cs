using GaleForceCore.Helpers;
using System.Collections.Generic;

namespace GaleForceCore.Writers
{
    /// <summary>
    /// Allows for writing values to keys in a template format.
    /// </summary>
    public class TemplateWriter
    {
        public string Template { get; set; }

        public char LeftBracket { get; set; } = '{';

        public char RightBracket { get; set; } = '}';

        public Dictionary<string, string> Terms { get; set; } = new Dictionary<string, string>();

        public int Length { get { return this.ToString().Length; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GaleForceCore.Writers.TemplateWriter"/> class.
        /// </summary>
        /// <param name="template">The template (ex: This {name} is {description}.</param>
        /// <param name="left">The left bracket character.</param>
        /// <param name="right">The right bracket character.</param>
        public TemplateWriter(string template = null, char left = '{', char right = '}')
        {
            this.Template = template;
            this.LeftBracket = left;
            this.RightBracket = right;
        }

        /// <summary>
        /// Adds the specified key/value pair to replace in the template on ToString().
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, string value)
        {
            if(!Terms.ContainsKey(key))
            {
                Terms.Add(key, value);
            }
            else
            {
                Terms[key] = value;
            }
        }

        public override string ToString()
        {
            var template = this.Template;
            foreach(var term in Terms)
            {
                template = template.Replace(LeftBracket + term.Key + RightBracket, term.Value);
            }

            template = StringHelper.RemoveAllBetween(template, LeftBracket, RightBracket, true);
            return template;
        }
    }
}

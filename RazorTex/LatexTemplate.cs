using System.IO;

namespace RazorTex
{
    public abstract class LatexTemplate<TModel> : LatexTemplate, ILatexTemplate<TModel>
    {
        #region Properties
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public TModel Model { get; set; }
        #endregion
    }

    public abstract class LatexTemplate : ILatexTemplate
    {
        public LatexHelper Latex { get; set; }

        // TODO
        public bool SuppressEmptyLines { get; set; }

        protected TextWriter Output
        {
            get
            {
                // This is not very clean. MVC, on the other hand, uses a more 
                // complicated concept that involves a stack of writers
                return Latex.Writer;
            }
        }

        public string Result
        {
            get
            {
                return Output.ToString();
            }
        }

        private bool IsEmptyLine(string s)
        {
            return (s.Replace(@"\", "").Trim().Length == 0);
        }

        /// <summary>
        /// LaTeX-Escapes the string representation of the given object and writes it to the result.
        /// </summary>
        /// <param name="object"></param>
        public void Write(object @object)
        {
            if (@object == null)
                return;

            string tempString = LatexEscape(@object.ToString());

            if (SuppressEmptyLines && IsEmptyLine(tempString))
                return;

            Output.Write(tempString);
        }

        /// <summary>
        /// Writes the string representation of the given object to the result without LaTeX-Escaping it.
        /// </summary>
        /// <param name="literal"></param>
        public void WriteLiteral(object literal)
        {
            if (literal == null)
                return;

            if (SuppressEmptyLines && IsEmptyLine(literal.ToString()))
                return;

            Output.Write(literal);
        }

        /// <summary>
        /// TODO: Make this solid.
        /// Note that this is non-trivial if we allow the backslash character, because 
        /// we'd have to memorize the positions already processed - a quite tedious task.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string LatexEscape(string input)
        {
            input = input.Replace(@"\", @"\textbackslash "); // Replace backslashs with '\textbackslash'. This must be the first step!
            input = input.Replace("\r\n", "\\\\ \r\n"); // Replace linebreaks with '\\' and a linebreak -- debatable
            
            // Escape Latex-Characters. There's probably a lot missing here:
            input = input.Replace("&", @"\&");
            input = input.Replace("$", @"\$");
            input = input.Replace("€", @"\euro");
            input = input.Replace("%", @"\%");
            input = input.Replace("_", @"\_");
            input = input.Replace("{", @"\{");
            input = input.Replace("}", @"\}");
            return input;
        }

        public virtual void Clear() { }

        /// <summary>
        /// This method will be overridden through the .cstex 'views'. Each of these files will be compiled
        /// into a class and that class implements Execute()
        /// </summary>
        public abstract void Execute();
    }
}

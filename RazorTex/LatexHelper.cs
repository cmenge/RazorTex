using System.IO;
using System;
using System.Security.Permissions;

namespace RazorTex
{
    public class LatexHelper //: IDisposable
    {
        private TextWriter _writer;
        private LatexRazorRenderer _renderer;

        internal TextWriter Writer { get { return _writer; } }

        public LatexHelper(LatexRazorRenderer renderer, TextWriter writer)
        {
            _writer = writer;
            _renderer = renderer;
        }

        /// <summary>
        /// Writes a latex string directly to the output
        /// </summary>
        /// <param name="plainString"></param>
        public void Emit(string plainString)
        {
            _writer.Write(plainString);
        }

        /// <summary>
        /// Outputs a formatted string. Note that this will interpret, e.g. '{0}', as
        /// placeholders!
        /// </summary>
        /// <param name="formatString"></param>
        /// <param name="args"></param>
        public void Format(string formatString, params object[] args)
        {
            _writer.Write(formatString, args);
        }

        [PermissionSetAttribute(SecurityAction.LinkDemand)]
        public string Partial(string name, object model)
        {
            // TODO: This should look more like this
            //using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            //{
            //    _renderer.RenderInternalTo(writer, model);
            //    return LatexString.Create(writer.ToString());
            //}

            // FIXME: This is ugly - this call will write to out active _writer, because they share it. 
            // That's very hacky.
            string s = _renderer.Render(model);
            return string.Empty; 
        }

        [PermissionSetAttribute(SecurityAction.LinkDemand)]
        public string Display<T>(T model)
        {
            // HACK: Again, this should render to a temporary writer and return a LatexString
            string s = _renderer.Render(model);
            return string.Empty;
        }
    }
}

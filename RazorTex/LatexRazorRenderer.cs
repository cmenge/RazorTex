using System.IO;
using System;
using System.Collections.Generic;
using System.Security.Permissions;

namespace RazorTex
{
    /// <summary>
    /// 
    /// </summary>
    [PermissionSetAttribute(SecurityAction.LinkDemand)]
    public sealed class LatexRazorRenderer : IDisposable
    {
        private TextWriter _outputWriter;

        public LatexHelper Latex { get; set; }
        public string TemplatePath { get; set; }
        public LatexContext Context { get; private set; }
        
        public LatexRazorRenderer()
        {
            _outputWriter = new StringWriter();
            Latex = new LatexHelper(this, _outputWriter);
            //Context = new LatexContext { Renderer = this, Helper = Latex, Writer = _tw };
            TemplatePath = string.Empty;
            Context = new LatexContext();
            Context.Helper = Latex;
        }

        /// <summary>
        /// Renders the given object to LaTeX, assuming a template with the same name as typeof(T).Name exist in
        /// the template path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="object"></param>
        /// <returns></returns>
        public string Render<T>(T @object)
        {
            return RenderInternal(@object, null);
        }

        public string Render<T>(T @object, IEnumerable<string> additionalNamespaces)
        {
            return RenderInternal(@object, additionalNamespaces);
        }

        // TODO:
        //public void RenderInternalTo<T>(TextWriter destination, T @object)
        //{
        //}

        private string RenderInternal<T>(T item, IEnumerable<string> additionalNamespaces)
        {
            StreamReader templateReader = null;
            string result = null;

            Type itemType = typeof(T);
            string typeName = itemType.Name;
            string cacheName = typeName;

            try
            {
                if (Razor.IsCached(cacheName))
                {
                    result = Razor.ProcessFromCache(item, Context, cacheName);
                }
                else
                {
                    string templateFileName = Path.Combine(TemplatePath, typeName + ".cstex");
                    string templateString = File.ReadAllText(templateFileName);
                    result = Razor.Process(templateString, item, Context, additionalNamespaces, cacheName);
                }

                return result;
            }
            catch (Exception ex)
            {
                // TODO: Handle errors, log
                throw;
            }
            finally
            {
                if (null != templateReader)
                {
                    templateReader.Close();
                }
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            if (_outputWriter != null)
            {
                _outputWriter.Dispose();
                _outputWriter = null;
            }
        }
        #endregion
    }
}

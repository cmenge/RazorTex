namespace RazorTex
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Razor;
    using System.Web.Razor.Parser;

    /// <summary>
    /// Compiles razor templates.
    /// </summary>
    [PermissionSetAttribute(SecurityAction.LinkDemand)]
    internal class RazorCompiler
    {
        #region Fields
        private readonly IRazorProvider provider;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="RazorCompiler"/>.
        /// </summary>
        /// <param name="provider">The provider used to compile templates.</param>
        public RazorCompiler(IRazorProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            this.provider = provider;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Compiles the template.
        /// </summary>
        /// <param name="className">The class name of the dynamic type.</param>
        /// <param name="template">The template to compile.</param>
        /// <param name="modelType">[Optional] The mode type.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private CompilerResults Compile(string className, string template, IEnumerable<string> additionalNamespaces, Type modelType = null)
        {
            var languageService = provider.CreateLanguageService();
            var codeDom = provider.CreateCodeDomProvider();
            var host = new RazorEngineHost(languageService);
            host.NamespaceImports.Add("System");
            host.NamespaceImports.Add("System.Collections.Generic");
            host.NamespaceImports.Add("System.IO");
            host.NamespaceImports.Add("System.Linq");
            host.NamespaceImports.Add("RazorTex");

            if (additionalNamespaces != null)
            {
                additionalNamespaces.ToList().ForEach(p => host.NamespaceImports.Add(p));
            }
            //Razor.importedNamespaces.ForEach(p => host.NamespaceImports.Add(p));

            var generator = languageService.CreateCodeGenerator(className, "Razor.Dynamic", null, host);
            var parser = new RazorParser(languageService.CreateCodeParser(), new HtmlMarkupParser()); //FIXME!

            // TODO: Make this association file-type dependent!
            Type baseType = (modelType == null)
                ? typeof(LatexTemplate)
                : typeof(LatexTemplate<>).MakeGenericType(modelType);

            generator.GeneratedClass.BaseTypes.Add(baseType);

            using(StreamReader reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(template))))
            {
                parser.Parse(reader, generator);
            }

            var statement = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Clear");
            generator.GeneratedExecuteMethod.Statements.Insert(0, new CodeExpressionStatement(statement));

            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                codeDom.GenerateCodeFromCompileUnit(generator.GeneratedCode, writer, new CodeGeneratorOptions());
            }

            var @params = new CompilerParameters();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                try
                {
                    @params.ReferencedAssemblies.Add(assembly.Location);
                }
                catch (Exception ex)
                { 
                }
            }

            @params.GenerateInMemory = true;
            @params.IncludeDebugInformation = false;
            @params.GenerateExecutable = false;
            @params.CompilerOptions = "/target:library /optimize";
            string source = builder.ToString();

            var result = codeDom.CompileAssemblyFromSource(@params, new[] { source });
            return result;
        }

        /// <summary>
        /// Creates a <see cref="ILatexTemplate" /> from the specified template string.
        /// </summary>
        /// <param name="template">The template to compile.</param>
        /// <param name="modelType">[Optional] The model type.</param>
        /// <returns>An instance of <see cref="ILatexTemplate"/>.</returns>
        public ILatexTemplate CreateTemplate(string template, IEnumerable<string> additionalNamespaces, Type modelType = null)
        {
            string className = Regex.Replace(Guid.NewGuid().ToString("N"), @"[^A-Za-z]*", "");

            var result = Compile(className, template, additionalNamespaces, modelType);

            if (result.Errors != null && result.Errors.Count > 0)
                throw new TemplateException(result.Errors);

            ILatexTemplate instance = (ILatexTemplate)result.CompiledAssembly.CreateInstance("Razor.Dynamic." + className);

            return instance;
        }
        #endregion
    }
}
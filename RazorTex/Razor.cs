namespace RazorTex
{
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;

    /// <summary>
    /// Process razor templates.
    /// </summary>
    public static class Razor
    {
        #region Fields
        private static RazorCompiler _compiler;
        private static readonly IDictionary<string, ILatexTemplate> _templateCache;
        private static object cacheSyncRoot = new object();
        #endregion

        #region Constructor
        /// <summary>
        /// Statically initialises the <see cref="Razor"/> type.
        /// </summary>
        static Razor()
        {
            _compiler = new RazorCompiler(new CSharpRazorProvider());
            _templateCache = new Dictionary<string, ILatexTemplate>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether a template of the given name already exist in cache, or not.
        /// </summary>
        /// <param name="cacheName">The name of the cached item</param>
        /// <returns>True if the template is in cache, false otherwise</returns>
        public static bool IsCached(string cacheName)
        {
            return (!string.IsNullOrEmpty(cacheName) && _templateCache.ContainsKey(cacheName));
        }

        /// <summary>
        /// Gets an <see cref="ILatexTemplate"/> for the specified template.
        /// </summary>
        /// <param name="template">The template to parse.</param>
        /// <param name="modelType">The model to use in the template.</param>
        /// <param name="name">[Optional] The name of the template.</param>
        /// <returns></returns>
        [PermissionSetAttribute(SecurityAction.LinkDemand)]
        private static ILatexTemplate GetTemplate(LatexContext context, string template, Type modelType, IEnumerable<string> additionalNamespaces, string name = null)
        {
            if (IsCached(name))
            {
                return _templateCache[name];
            }

            var instance = _compiler.CreateTemplate(template, additionalNamespaces, modelType);

            // TODO: INJECT Dependencies!
            instance.Latex = context.Helper;

            if (!IsCached(name))
            {
                lock (cacheSyncRoot)
                {
                    _templateCache.Add(name, instance);
                }
            }

            return instance;
        }

        /// <summary>
        /// Processes a template from cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <param name="cacheName">Cache name of the template</param>
        /// <returns></returns>
        public static string ProcessFromCache<T>(T model, LatexContext context, string cacheName)
        {
            // Throws if the item isn't cached. The caller is responsible here
            ILatexTemplate template = _templateCache[cacheName];

            // Assign the model
            if (template is ILatexTemplate<T>)
                ((ILatexTemplate<T>)template).Model = model;
            else if (template is IGenericTemplate<T>)
                ((GenericTemplate<T>)template).Model = model;

            template.Latex = context.Helper;

            template.Execute();
            return template.Result;
        }

        /// <summary>
        /// Processes the supplied template by compiling it and rendering it using the given model. If a cacheName is specified, the compiled template will be cached.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template to parse.</param>
        /// <param name="model">The model to use in the template.</param>
        /// <param name="cacheName">[Optional] A name for the template used for caching.</param>
        /// <returns>The parsed template.</returns>
        [PermissionSetAttribute(SecurityAction.LinkDemand)]
        public static string Process<T>(string template, T model, LatexContext context, IEnumerable<string> additionalNamespaces, string cacheName = null)
        {
            var instance = GetTemplate(context, template, typeof(T), additionalNamespaces, cacheName);

            // Assign the model
            if(instance is ILatexTemplate<T>)
                ((ILatexTemplate<T>)instance).Model = model;
            else if (instance is IGenericTemplate<T>)
                ((GenericTemplate<T>)instance).Model = model;

            instance.Execute();
            return instance.Result;
        }

        /// <summary>
        /// Sets the razor provider used for compiling templates.
        /// </summary>
        /// <param name="provider">The razor provider.</param>
        public static void SetRazorProvider(IRazorProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _compiler = new RazorCompiler(provider);
        }
        #endregion
    }
}
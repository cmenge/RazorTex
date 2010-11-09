namespace RazorTex
{
    /// <summary>
    /// A razor template.
    /// </summary>
    public interface ITemplate
    {
        #region Properties
        /// <summary>
        /// Gets the parsed result of the template.
        /// </summary>
        string Result { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Clears the template.
        /// </summary>
        void Clear();

        /// <summary>
        /// Executes the template.
        /// </summary>
        void Execute();

        /// <summary>
        /// Writes the specified object to the template.
        /// </summary>
        /// <param name="object"></param>
        void Write(object @object);

        /// <summary>
        /// Writes a literal to the template.
        /// </summary>
        /// <param name="literal"></param>
        void WriteLiteral(object literal);
        #endregion
    }

    public interface ILatexTemplate : ITemplate
    {
        LatexHelper Latex { get; set; }
    }

    /// <summary>
    /// A razor template with a model.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    public interface IGenericTemplate<TModel> : ITemplate
    {
        #region Properties
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        TModel Model { get; set; }
        #endregion
    }

    /// <summary>
    /// A latex razor template with a model.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    public interface ILatexTemplate<TModel> : ILatexTemplate
    {
        #region Properties
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        TModel Model { get; set; }
        #endregion
    }
}
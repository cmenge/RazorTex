namespace RazorTex
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines an exception that occurs during compilation of a template.
    /// </summary>
    [Serializable]
    public class TemplateException : Exception
    {
        /// <summary>
        /// Gets the collection of compiler errors.
        /// </summary>
        public ReadOnlyCollection<CompilerError> Errors { get; private set; }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateException"/>
        /// </summary>
        /// <param name="errors">The collection of compilation errors.</param>
        internal TemplateException(CompilerErrorCollection errors)
            : base("Unable to compile template.")
        {
            var list = new List<CompilerError>();
            foreach (CompilerError error in errors)
            {
                list.Add(error);
            }
            Errors = new ReadOnlyCollection<CompilerError>(list);
        }

        public TemplateException() { }
        public TemplateException(string message) : base(message) { }
        public TemplateException(string message, Exception inner) : base(message, inner) { }

        protected TemplateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            // TODO: Deserialize error list!
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // TODO: Serialize error list!
            base.GetObjectData(info, context);
        }
    }
}
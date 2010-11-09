
namespace RazorTex
{
    public class LatexString
    {
        public string Content { get; private set; }

        public static LatexString Create(string s)
        {
            return new LatexString(s);
        }

        public LatexString(string latex)
        {
            Content = latex;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}

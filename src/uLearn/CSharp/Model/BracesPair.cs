using Microsoft.CodeAnalysis;

namespace uLearn.CSharp.Model
{
    public class BracesPair
    {
        public readonly SyntaxToken Open;
        public readonly SyntaxToken Close;

        public BracesPair(SyntaxToken open, SyntaxToken close)
        {
            Open = open;
            Close = close;
        }

        public bool TokenInsideBraces(SyntaxToken token)
        {
            return token.SpanStart > Open.SpanStart && token.Span.End < Close.Span.End;
        }

        public override string ToString()
        {
            return $"������ {Open.GetLine() + 1}, {Close.GetLine() + 1}";
        }
    }
}
namespace uLearn.CSharp.Model
{
    public class ReportWithLine
    {
        public int Line { get; set; }
        public string Report { get; set; }

        public override string ToString() => $"Line: {Line}, Report: {Report}";
    }
}
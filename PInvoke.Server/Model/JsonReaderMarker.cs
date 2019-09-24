namespace PInvoke.Server.Model
{
    public struct JsonReaderMarker
    {
        public readonly int Depth;
        public readonly int LineNumber;
        public readonly int LinePosition;

        public JsonReaderMarker(int depth, int lineNumber, int linePosition)
        {
            this.Depth = depth;
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }
    }
}

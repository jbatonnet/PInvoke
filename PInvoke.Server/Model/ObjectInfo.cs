namespace PInvoke.Server.Model
{
    public abstract class ObjectInfo
    {
        internal FastJsonStreamReader jsonReader;
        internal long jsonPosition;

        internal ObjectInfo(FastJsonStreamReader jsonReader, long jsonPosition)
        {
            this.jsonReader = jsonReader;
            this.jsonPosition = jsonPosition;
        }

        internal void SkipToProperty(string property)
        {
            while (true)
            {
                if (jsonReader.TokenType == JsonTokenType.PropertyName && jsonReader.ReadString() == property)
                {
                    break;
                }

                jsonReader.Read();
            }
        }
    }
}

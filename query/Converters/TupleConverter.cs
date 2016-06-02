namespace QueryEngine.Converters
{
    using System;
    using Newtonsoft.Json;

    // Tuple<string,string> is used for column info, this converter makes the data a bit more workable
    public class TupleConverter : JsonConverter
    {   
        public override bool CanConvert(Type objectType)
        {
            return typeof(Tuple<string, string>) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tuple = value as Tuple<string, string>;
            string[] val = null;
            if (tuple != null)
            {
                val = new string[] { tuple.Item1, tuple.Item2 };
            }
            serializer.Serialize(writer, val);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}

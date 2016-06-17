namespace QueryEngine.Converters
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;

    // by default newtonsoft renders number types as bare numbers, but this can exceed the 
    // range for JS, to avoid data loss we convert these to strings instead.
    public class NumberConverter : JsonConverter
    {
        Type[] canHande = new Type[] {
            typeof(long),
            typeof(Nullable<long>),
            typeof(decimal),
            typeof(Nullable<decimal>),
        };
        
        public override bool CanConvert(Type objectType)
        {
            return canHande.Any(x => x.Equals(objectType));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Nullable<long> nl = value as Nullable<long>;
            Nullable<decimal> nd = value as Nullable<decimal>;
            if (nl != null) 
            {
                serializer.Serialize(writer, nl.HasValue ? (object)nl.Value.ToString() : nl);
            }
            else if (nd != null)
            {
                serializer.Serialize(writer, nd.HasValue ? (object)nd.Value.ToString() : nd);
            }
            else 
            {
                serializer.Serialize(writer, value != null ? value.ToString() : value);
            }
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}

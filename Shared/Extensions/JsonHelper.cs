using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ESXSpectateControl.Shared
{
    public static class JsonHelper
    {
        public static readonly Dictionary<Type, Type> Substitutes = new Dictionary<Type, Type>();

        public static readonly List<JsonConverter> Converters = new()
		{
        };

        public static readonly JsonSerializerSettings Empty = new()
		{
            Converters = Converters,
            ContractResolver = new ContractResolver()
        };

        public static readonly JsonSerializerSettings IgnoreJsonIgnoreAttributes = new()
        {
            ContractResolver = new IgnoreJsonAttributesResolver()
        };

        public static readonly JsonSerializerSettings LowerCaseSettings = new()
        {
            Converters = Converters,
            ContractResolver = new ContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string ToJson(this object value, bool pretty = false, JsonSerializerSettings settings = null)
        {
            return (string)InvokeWithRepresentation(
                () => JsonConvert.SerializeObject(value, pretty ? Formatting.Indented : Formatting.None, settings ?? Empty));
        }

        public static T FromJson<T>(this string serialized, JsonSerializerSettings settings = null) => (T)FromJsonInternal(serialized, typeof(T), out _, settings);

        public static object FromJson(this string serialized, Type type, JsonSerializerSettings settings = null) => FromJsonInternal(serialized, type, out _, settings);

        public static T FromJson<T>(this string serialized, out bool result, JsonSerializerSettings settings = null)
        {
            var value = FromJsonInternal(serialized, typeof(T), out var transient, settings);

            result = transient;
            return (T)value;
        }

        private static object FromJsonInternal(string serialized, Type type, out bool result, JsonSerializerSettings settings)
        {
            try
            {
                var deserialized = InvokeWithRepresentation(() => JsonConvert.DeserializeObject(serialized, type, settings ?? Empty), false);

                result = true;

                return deserialized;
            }
            catch (Exception)
            {
                result = false;

                throw;
            }
        }

        private static object InvokeWithRepresentation(Func<object> func, bool suppressErrors = true)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception)
            {
                if (!suppressErrors)
                    throw;
            }

            return null;
        }
    }
}
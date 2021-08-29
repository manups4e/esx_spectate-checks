using CitizenFX.Core;
using Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
#if CLIENT
using ESXSpectateControl.Client.NativeUI;
#endif

namespace ESXSpectateControl.Shared
{
	internal class IgnoreJsonAttributesResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
			foreach (JsonProperty prop in props)
			{
				prop.Ignored = false;   // Ignore [JsonIgnore]
										//prop.Converter = null;  // Ignore [JsonConverter]
										//prop.PropertyName = prop.UnderlyingName;  // Use original property name instead of [JsonProperty] name
			}
			return props;
		}
	}

	public class MinMaggioreDiMax : Exception { }
}
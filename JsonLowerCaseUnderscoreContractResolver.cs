using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StripeAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StripeAPI
{
	public class JsonLowerCaseUnderscoreContractResolver : DefaultContractResolver
	{
		private Regex regex = new Regex("(?!(^[A-Z]))([A-Z])");

		protected override string ResolvePropertyName(string propertyName)
		{
			var newName = regex.Replace(propertyName, "_$2").ToLower();
			if (newName.Length > 3 && newName.EndsWith("_id"))
				newName = newName.Substring(0, newName.Length - 3);

			return newName;
		}
	}

	public class StripeDateTimeConverter : DateTimeConverterBase
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteRawValue(@"""\/Date(" + ConvertDateTimeToEpoch((DateTime)value).ToString() + @")\/""");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.Value == null) return null;

			if (reader.TokenType == JsonToken.Integer)
				return ConvertEpochToDateTime((long)reader.Value);

			return DateTime.Parse(reader.Value.ToString());
		}

		private DateTime ConvertEpochToDateTime(long seconds)
		{
			return new DateTime(1970, 1, 1).AddSeconds(seconds);
		}

		private long ConvertDateTimeToEpoch(DateTime datetime)
		{
			var epochStart = new DateTime(1970, 1, 1);
			if (datetime < epochStart) return 0;

			return Convert.ToInt64(datetime.Subtract(epochStart).TotalSeconds);
		}
	}

	//public class StripeContractResolver : JsonLowerCaseUnderscoreContractResolver
	//{
	//	public new static readonly StripeContractResolver Instance = new StripeContractResolver();

	//	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	//	{
	//		JsonProperty property = base.CreateProperty(member, memberSerialization);

	//		//if (typeof(StripeId).IsAssignableFrom(property.DeclaringType) && property.PropertyName.EndsWith("Id"))
	//		if (property.DeclaringType == typeof(StripeSubscription) && property.PropertyName.StartsWith("plan"))
	//		{
	//			property.ShouldSerialize =
	//				instance =>
	//				{
	//					var stripeObj = (StripeSubscription)instance;
	//					//return e.Manager != e;
	//					if (property.PropertyName.EndsWith("Id"))
	//						return !string.IsNullOrWhiteSpace(stripeObj.PlanId);
	//					else
	//						return stripeObj.Plan != null;
	//				};
	//			//return null;
	//		}

	//		return property;
	//	}
	//}


	//public class StripeListConverter : CustomCreationConverter<StripeList<StripeSubscription>>
	//{
	//	public override bool CanConvert(Type objectType)
	//	{
	//		return (objectType == typeof(StripeSubscription) || typeof(StripeSubscription).IsAssignableFrom(objectType));
	//	}

	//	public override StripeList<StripeSubscription> Create(Type objectType)
	//	{
	//		return new StripeList<StripeSubscription>();
	//	}

	//	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	//	{
	//		StripeId sobj = (StripeId)value;
	//		if (sobj.IdOnly)
	//		{
	//			writer.WriteRawValue(sobj.Id);
	//		}
	//		else
	//		{

	//			serializer.Serialize(writer, value);
	//		}
	//	}

	//	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	//	{
	//		JObject jo = JObject.Load(reader);
	//		if (jo["id"] == null)
	//			return new StripeSubscription() { Id = reader.Value.ToString(), IdOnly = true };
	//		else
	//		{
	//			var result = new StripeSubscription();
	//			result = jo.ToObject<StripeSubscription>(serializer);
	//			return result;

	//		}
	//		//return JsonConvert.DeserializeObject(jo.ToString(),
	//		//									objectType,
	//		//									new JsonSerializerSettings()
	//		//									{
	//		//										ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
	//		//										Converters = { new StripeDateTimeConverter() }
	//		//									});
	//	}
	//}
	//public class StripeSubscriptionConverter : CustomCreationConverter<StripeSubscription>
	//{
	//	public override bool CanConvert(Type objectType)
	//	{
	//		var can =  (objectType == typeof(StripeSubscription) || typeof(StripeSubscription).IsAssignableFrom(objectType));
	//		return can;
	//	}

	//	public override StripeSubscription Create(Type objectType)
	//	{
	//		return new StripeSubscription();
	//	}

	//	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	//	{
	//		StripeId sobj = (StripeId)value;
	//		if (sobj.IdOnly)
	//		{
	//			writer.WriteRawValue(sobj.Id);
	//		}
	//		else
	//		{
				
	//			serializer.Serialize(writer, value);
	//		}
	//	}

	//	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	//	{
	//		JObject jo = JObject.Load(reader);
	//		if (jo["id"] == null)
	//			return new StripeSubscription() { Id = reader.Value.ToString(), IdOnly = true };
	//		else
	//		{
	//			var result = new StripeSubscription();
	//			result = jo.ToObject<StripeSubscription>(serializer);
	//			return result;

	//		}
	//			//return JsonConvert.DeserializeObject(jo.ToString(),
	//			//									objectType,
	//			//									new JsonSerializerSettings()
	//			//									{
	//			//										ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
	//			//										Converters = { new StripeDateTimeConverter() }
	//			//									});
	//	}
	//}
}

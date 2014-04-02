using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StripeAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StripeAPI
{
    public class StripeClient
    {
		public string ApiUrl { get; set; }
		public string ApiVersion { get; set; }
		public string ApiSecretKey { get; set; }
		public string ApiPublicKey { get; set; }

		private HttpHelper httpHelper;

		public StripeClient(string secretKey)
		{
			ApiUrl = "https://api.stripe.com/";
			ApiVersion = "v1";
			ApiSecretKey = secretKey;
			httpHelper = new HttpHelper(ApiUrl, ApiVersion, ApiSecretKey);
		}

		public async Task<StripeCustomer> GetCustomer(StripeCustomer customer)
		{
			return await GetCustomer(customer.Id);
		}

		public async Task<StripeCustomer> GetCustomer(string customerId)
		{
			var result =  await httpHelper.ExecuteGet("customers/" + customerId);
			if (result.Success)
				return Deserialize<StripeCustomer>(result.Response);
			else
				return null;
		}

		public async Task<StripeList<StripeCustomer>> GetCustomers()
		{
			var result = await httpHelper.ExecuteGet("customers");
			if (result.Success)
				return Deserialize<StripeList<StripeCustomer>>(result.Response);
			else
				return null;
		}

		public async Task<JObject> GetPlans()
		{
			var result = await httpHelper.ExecuteGet("plans");
			if (result.Success)
				return Deserialize<JObject>(result.Response);
			else
				return null;
		}

		public async Task<bool> AddCustomer(StripeCustomer customer)
		{
			var result = await httpHelper.ExecutePostForm("customers", customer.ToJObject());
			if (result.Success)
			{
				var respObj = Deserialize<StripeCustomer>(result.Response);
			}
			
			return true;
		}

		public T Deserialize<T>(string value)
		{
			return JsonConvert.DeserializeObject<T>(value,
													new JsonSerializerSettings()
													{
														ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
														Converters = { new StripeDateTimeConverter() }
													});
		}
		
    }

	public class RestResult
	{
		public string Response { get; set; }
		public bool Success { get; set; }
		public RestError Error { get; set; }
	}

	public class RestError
	{
		public string Type { get; set; }
		public string Message { get; set; }
		public string Param { get; set; }
	}
}

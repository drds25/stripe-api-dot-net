using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

		public StripeClient()
		{
			ApiUrl = "https://api.stripe.com/";
			ApiVersion = "v1";
		}

		public async Task<JObject> GetCustomer(string customerId)
		{
			var result =  await ExecuteGet("customers/" + customerId);
			if (result.Success)
				return result.ResponseObject;
			else
				return null;
		}

		public async Task<bool> AddCustomer()
		{
			var cust = (dynamic)new JObject();
			cust.description = "new test customer";
			cust.email = "dsmith@tragon.com";
			cust["account_balance"] = -10;
			cust.card = (dynamic)new JObject();
			cust.card.number = "4242424242424242";
			cust.card.exp_month = "01";
			cust.card.exp_year = "2015";
			cust.card.cvc = "123";
			

			var result = await ExecutePost("customers", cust);
			
			return true;
		}

		private async Task<RestResult> ExecuteGet(string command)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(String.Format("{0}{1}/",ApiUrl,ApiVersion));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", ApiSecretKey, ""))));

				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				RestResult result = new RestResult();
				HttpResponseMessage response = await client.GetAsync(command);
				if (response.IsSuccessStatusCode)
				{
					String responseData = await response.Content.ReadAsStringAsync();
					result.Success = true;
					result.ResponseObject = (JObject)JsonConvert.DeserializeObject(responseData);
				}
				else
				{
					result.Success = false;
					result.Error = await response.Content.ReadAsAsync<RestError>();
				}
				return result;
			}
		}

		private async Task<RestResult> ExecutePost(string command, JObject payload)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(String.Format("{0}{1}/", ApiUrl, ApiVersion));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", ApiSecretKey, ""))));

				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var x = JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(payload.ToString());

				var propList = GetKeyValueList(string.Empty, payload);
				var content = new FormUrlEncodedContent(propList);

				RestResult result = new RestResult();
				HttpResponseMessage response = await client.PostAsync(command, content);
				if (response.IsSuccessStatusCode)
				{
					String responseData = await response.Content.ReadAsStringAsync();
					result.Success = true;
					result.ResponseObject = (JObject)JsonConvert.DeserializeObject(responseData);
				}
				else
				{
					result.Success = false;
					result.Error = await response.Content.ReadAsAsync<RestError>();
				}
				return result;
			}
		}

		private static List<KeyValuePair<string, string>> GetKeyValueList( string parent, IEnumerable<KeyValuePair<string,JToken>> payload)
		{
			var propList = new List<KeyValuePair<string, string>>();
			foreach (var prop in payload)
			{
				if (prop.Value.Type == JTokenType.Object)
				{
					propList.AddRange(GetKeyValueList(prop.Key, (JObject)prop.Value));
				}
				else
				{
					var keyString = prop.Key;
					if (!String.IsNullOrWhiteSpace(parent))
					{
						keyString = String.Format("{0}[{1}]", parent, prop.Key);
					}
					propList.Add(new KeyValuePair<string, string>(keyString, prop.Value.ToString()));
				}
			}

			
			//foreach (var p in payload.Properties())
			//{
			//	if (p.Value.Type == JTokenType.Object)
			//	{
			//		foreach(var innerP in p.Value)
			//		{
			//			propList.Add(new KeyValuePair<string, string>(String.Format("{0}[{1}]", p.Name, innerP), p.Value.ToString()));	
			//		}
					
			//	}
			//	else
			//	{
			//		propList.Add(new KeyValuePair<string, string>(p.Name, p.Value.ToString()));
			//	}
			//}
			return propList;
		}
    }

	public class RestResult
	{
		public JObject ResponseObject { get; set; }
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

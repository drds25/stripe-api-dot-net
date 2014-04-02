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
	public class HttpHelper
	{
		public Uri BaseAddress { get; set; }
		public String ApiKey { get; set; }

		private AuthenticationHeaderValue _authHeader;
		private AuthenticationHeaderValue authHeader {
			get
			{
				if (_authHeader == null)
				{
					_authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", ApiKey, ""))));
				}
				return _authHeader;
			}

		}

		public HttpHelper(string apiUrl, string apiVersion)
		{
			BaseAddress = new Uri(String.Format("{0}{1}/", apiUrl, apiVersion));
		}

		public HttpHelper(string apiUrl, string apiVersion, string apiKey)
		{
			ApiKey = apiKey;
			BaseAddress = new Uri(String.Format("{0}{1}/", apiUrl, apiVersion));
		}

		public async Task<RestResult> ExecuteGet(string command)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = BaseAddress;
				client.DefaultRequestHeaders.Authorization = authHeader;

				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				RestResult result = new RestResult();
				HttpResponseMessage response = await client.GetAsync(command);
				
				if (response.IsSuccessStatusCode)
				{
					String responseData = await response.Content.ReadAsStringAsync();
					result.Success = true;
					result.Response = responseData;
				}
				else
				{
					result.Success = false;
					result.Error = await response.Content.ReadAsAsync<RestError>();
				}
				return result;
			}
		}

		public async Task<RestResult> ExecutePostForm(string command, JObject payload)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = BaseAddress;
				client.DefaultRequestHeaders.Authorization = authHeader;

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
					result.Response = responseData;
				}
				else
				{
					result.Success = false;
					result.Error = await response.Content.ReadAsAsync<RestError>();
				}
				return result;
			}
		}

		private static List<KeyValuePair<string, string>> GetKeyValueList(string parent, IEnumerable<KeyValuePair<string, JToken>> payload)
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

			return propList;
		}
	}
}

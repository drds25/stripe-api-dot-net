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
	public class StripeGetOptions
	{
		public DateTime? Date;
		public int? Limit;
		public string StartingAfterId;
		public string TargetID;
		public string TargetType;

		private const string paramFormat = "{0}={1}";
		public string ToQuerystring()
		{
			var sb = new StringBuilder();
			if (!String.IsNullOrWhiteSpace(TargetID) && !String.IsNullOrWhiteSpace(TargetType))
			{
				sb.AppendFormat(paramFormat,TargetType, TargetID);
			}
			if (Limit.HasValue)
				sb.AppendFormat(paramFormat, "limit", Limit.Value.ToString());

			if (sb.Length > 0)
				return "?" + sb.ToString();
			else
				return "";
		}
	}
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

		public RestResult ExecuteGet(string command, StripeGetOptions getOptions = null)
		{
			getOptions = getOptions == null ? new StripeGetOptions() : getOptions;
			using (var client = new HttpClient())
			{
				client.BaseAddress = BaseAddress;
				client.DefaultRequestHeaders.Authorization = authHeader;

				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				RestResult result = new RestResult();
				var response = client.GetAsync(command + getOptions.ToQuerystring());
				
				if (response.Result.IsSuccessStatusCode)
				{
					String responseData = response.Result.Content.ReadAsStringAsync().Result;
					result.Success = true;
					result.Response = responseData;
				}
				else
				{
					result.Success = false;
					result.Error = response.Result.Content.ReadAsAsync<RestError>().Result;
				}
				return result;
			}
		}

		public RestResult ExecutePostForm(string command, JObject payload)
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
				var response = client.PostAsync(command, content);
				if (response.Result.IsSuccessStatusCode)
				{
					String responseData = response.Result.Content.ReadAsStringAsync().Result;
					result.Success = true;
					result.Response = responseData;
				}
				else
				{
					var errorString = response.Result.Content.ReadAsStringAsync().Result;
					result = JsonConvert.DeserializeObject<RestResult>(errorString,
													new JsonSerializerSettings()
													{
														ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
														Converters = { new StripeDateTimeConverter() }
													});
					result.Success = false;
				}
				return result;
			}
		}

		public async Task<RestResult> ExecuteGetAsync(string command)
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

		public async Task<RestResult> ExecutePostFormAsync(string command, JObject payload)
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

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
		#region Customer
		public StripeList<StripeCustomer> GetCustomers(StripeGetOptions getOptions = null)
		{
			var result = httpHelper.ExecuteGet("customers",getOptions);
			if (result.Success)
				return Deserialize<StripeList<StripeCustomer>>(result.Response);
			else
				return null;
		}

		public StripeCustomer GetCustomer(StripeCustomer customer, StripeGetOptions getOptions = null)
		{
			return GetCustomer(customer.Id, getOptions);
		}

		public StripeCustomer GetCustomer(string customerId, StripeGetOptions getOptions = null)
		{
			var result =  httpHelper.ExecuteGet("customers/" + customerId, getOptions);
			if (result.Success)
				return Deserialize<StripeCustomer>(result.Response);
			else
				return null;
		}

		public StripeCustomer AddCustomer(StripeCustomer customer)
		{
			var result = httpHelper.ExecutePostForm("customers", customer.ToJson());
			if (result.Success)
			{
				return Deserialize<StripeCustomer>(result.Response);
			}

			return null;
		}

		public StripeCustomer UpdateCustomer(StripeCustomer customer)
		{
			var url = "customers/" + customer.Id;
			//remove properties not allowed for update command.
			customer.Id = null;
			customer.Discount = null;
			customer.Subscriptions = null;
			customer.Cards = null;
			customer.Created = null;
			customer.Delinquent = null;
			customer.Currency = null;

			var result = httpHelper.ExecutePostForm(url, customer.ToJson());
			if (result.Success)
			{
				return Deserialize<StripeCustomer>(result.Response);
			}

			return null;
		}
		#endregion
		#region Subscriptions

		public StripeList<StripeSubscription> GetCustomerSubscriptions(StripeCustomer customer, StripeGetOptions getOptions = null)
		{
			return GetCustomerSubscriptions(customer.Id, getOptions);
		}

		public StripeList<StripeSubscription> GetCustomerSubscriptions(string customerId, StripeGetOptions getOptions = null)
		{
			var url = "customers/" + customerId + "/subscriptions";
			var result = httpHelper.ExecuteGet(url, getOptions);
			if (result.Success)
				return Deserialize<StripeList<StripeSubscription>>(result.Response);
			else
				return null;
		}
		public StripeSubscription GetSubscription(StripeCustomer customer, string subscriptionId)
		{
			return GetSubscription(customer.Id, subscriptionId);
		}

		public StripeSubscription GetSubscription(string customerId, string subscriptionId, StripeGetOptions getOptions = null)
		{
			var url = "customers/" + customerId + "/subscriptions/" + subscriptionId;
			var result = httpHelper.ExecuteGet(url, getOptions);
			if (result.Success)
				return Deserialize<StripeSubscription>(result.Response);
			else
				return null;
		}

		public StripeSubscription AddSubscription(StripeSubscription newSubscription, string customerId)
		{
			var url = "customers/" + customerId + "/subscriptions";
			newSubscription.Id = null;
			var result = httpHelper.ExecutePostForm(url, newSubscription.ToJson());
			if (result.Success)
			{
				var a = Deserialize<StripeSubscription>(result.Response);
				return a;
			}

			return null;
		}

		public StripeSubscription UpdateSubscription(StripeSubscriptionUpdate subscriptionUpdate, string customerId)
		{
			var url = "customers/" + customerId + "/subscriptions/" + subscriptionUpdate.Id;
			//remove properties not allowed for update command.
			subscriptionUpdate.Id = null;
			

			var result = httpHelper.ExecutePostForm(url, subscriptionUpdate.ToJson());
			if (result.Success)
			{
				return Deserialize<StripeSubscription>(result.Response);
			}

			return null;
		}
		#endregion

		#region Plans
		public StripeList<StripePlan> GetPlans(StripeGetOptions getOptions = null)
		{
			var result = httpHelper.ExecuteGet("plans", getOptions);
			if (result.Success)
				return Deserialize<StripeList<StripePlan>>(result.Response);
			else
				return null;
		}

		public StripeList<StripePlan> GetPlans(string planId, StripeGetOptions getOptions = null)
		{
			var url = "plans/{0}";
			var result = httpHelper.ExecuteGet(String.Format(url, planId),getOptions);
			if (result.Success)
				return Deserialize<StripeList<StripePlan>>(result.Response);
			else
				return null;
		}

		#endregion
		#region Invoices
		public StripeList<StripeInvoice> GetInvoices(StripeGetOptions getOptions = null)
		{
			var url = "invoices";
			var result = httpHelper.ExecuteGet(String.Format(url),getOptions);
			if (result.Success)
				return Deserialize<StripeList<StripeInvoice>>(result.Response);
			else
				return null;
		}

		public StripeList<StripeInvoice> GetInvoices(string customerId, StripeGetOptions getOptions = null)
		{
			var url = "invoices";
			getOptions = getOptions == null ? new StripeGetOptions() : getOptions;
			getOptions.TargetID = customerId;
			getOptions.TargetType = "customer";

			var result = httpHelper.ExecuteGet(String.Format(url), getOptions);
			if (result.Success)
				return Deserialize<StripeList<StripeInvoice>>(result.Response);
			else
				return null;
		} 

		public StripeInvoice GetNextInvoice(string customerId, StripeGetOptions getOptions = null)
		{
			var url = "invoices/upcoming";
			getOptions = getOptions == null ? new StripeGetOptions() : getOptions;
			getOptions.TargetID = customerId;
			getOptions.TargetType = "customer";

			var result = httpHelper.ExecuteGet(String.Format(url), getOptions);
			if (result.Success)
				return Deserialize<StripeInvoice>(result.Response);
			else
				return null;
		}
		#endregion

		public T Deserialize<T>(string value) where T : StripeObject, new()
		{
			return (T)new T().FromJson(value);
		}
		
		public JObject Serialize(StripeObject value)
		{
			return (JObject)JToken.FromObject(value, new JsonSerializer()
			{
				NullValueHandling = NullValueHandling.Ignore,
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

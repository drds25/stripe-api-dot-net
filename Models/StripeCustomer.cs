using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StripeAPI.Models
{
	public class StripeObject
	{
		public string Description { get; set; }
		public Dictionary<string, string> MetaData { get; set; }
		public bool? LiveMode { get; set; }

		//protected dynamic JsonRepresentation;
	}

	public class StripeCreditCard : StripeObject
	{
		public static string Object{ get { return "card"; } }

		public string Id { get; set; }
		public string Token { get; set; }
		public string Number { get; set;}
		public string Cvc { get; set; }
		public string Last4 { get; set; }
		public string Type { get; set; }
		public int? ExpMonth { get; set; }
		public int? ExpYear { get; set; }

		public string CvcCheck { get; set; }
		public string AddressLine1Check { get; set; }
		public string AddressZipCheck { get; set; }

		public string Fingerprint { get; set; }
		public string Customer { get; set; }
		public string Country { get; set; }
		public string Name { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressCity { get; set; }
		public string AddressState { get; set; }
		public string AddressZip { get; set; }
		public string AddressCountry { get; set; }

	}

	public class StripeCustomer : StripeObject
	{
		public static string Object { get { return "customer"; } }

		public StripeCustomer()
		{
			//JsonRepresentation = (dynamic)new JObject();
		}
		

		public string Id { get; set; }
		public string Email { get; set; }
		public DateTime? Created { get; set; }
		public bool? Delinquent { get; set; }
		public StripeList<StripeSubscription> Subscriptions { get; set; }
		public StripeDiscount Discount { get; set; }
		public decimal? AccountBalance { get; set; }
		public string Currency { get; set; }
		public StripeList<StripeCreditCard> Cards { get; set; }
		public string DefaultCard { get; set; }
		public StripeCreditCard Card { get; set; }

	}
	public class StripeCoupon : StripeObject
	{
		public static string Object{ get { return "coupon"; } }

		public string Id { get; set; }
		public DateTime? Created { get; set; }
		public int? PercentOff { get; set; }
		public int? AmountOff { get; set; }
		public string Currency { get; set; }
		public string Duration { get; set; }
		public int? DurationInMonths { get; set; }
		public int? MaxRedemptions { get; set; }
		public int? TimeRedeemed { get; set; }
		public DateTime? RedeemBy { get; set; }
		public bool? Valid { get; set; }
	}
	public class StripeDiscount : StripeObject
	{
		public static string Object{ get { return "discount"; } }

		public StripeCoupon Coupon { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public string Customer { get; set; }
		public string Subscription { get; set; }
	}

	public class StripeSubscription : StripeObject
	{
		public static string Object{ get { return "subscription"; } }

		public string Id { get; set; }
		public StripePlan Plan {get;set;}
		public int? Quantity { get; set; }
		public string Customer { get; set; }
		public StripeDiscount Discount { get; set; }

		public bool? CancelAtPeriodEnd { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? CurrentPeriodStart { get; set; }
		public DateTime? CurrentPeriodEnd { get; set; }
		public DateTime? EndedAt { get; set; }
		public DateTime? TrialStart { get; set; }
		public DateTime? TrialEnd { get; set; }
		public DateTime? CanceledAt { get; set; }

		public decimal? ApplicationFeePercent { get; set; }
	}
	public class StripeList<T>
	{
		public int? TotalCount { get; set; }
		public bool? HasMore { get; set; }
		public string Url { get; set; }
		public List<T> Data { get; set; }
		
		public void Add(T item)
		{
			if (Data == null)
			{
				Data = new List<T>();
			}
			Data.Add(item);
		}

		public int Count()
		{
			return Data.Count();
		}
	}
	public class StripePlan : StripeObject
	{
		public static string Object{ get { return "plan"; } }

		public string Interval { get; set; }
		public int? IntervalCount { get; set; }
		public string Name { get; set; }
		public DateTime? Created { get; set; }
		public decimal? Amount { get; set; }
		public string Currency { get; set; }
		public string Id { get; set; }
		public int? TrialPeriodDays { get; set; }
		public string StatementDescription { get; set; }
	}
	
	public class StripeDuration
	{
		public static string Once { get { return "once"; } }
		public static string Repeating { get { return "repeating"; } }
		public static string Forever { get { return "forever"; } }
	}
	public class StripeInterval
	{
		public static string Week { get { return "week"; } }
		public static string Month { get { return "month"; } }
		public static string Year { get { return "year"; } }
	}

	public class StripeCurrency
	{
		public static string USD { get { return "usd"; } }
	}

	public class StripeCheckResult
	{
		public static string Pass { get { return  "pass"; } }
		public static string Fail { get { return  "fail"; } }
		public static string Unchecked { get { return "unchecked"; } }
	}



}

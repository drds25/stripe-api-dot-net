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
		[JsonIgnore]
		public virtual string Object { get { return "object"; } }
		
		public string Id { get; set; }
		public string Description { get; set; }
		public Dictionary<string, string> MetaData { get; set; }
		public bool? LiveMode { get; set; }
		

		[JsonIgnore]
		public bool IdOnly { get; set; }

		public virtual JObject ToJson()
		{
			JObject json;
			if (this.IdOnly)
			{
				json = new JObject();
				json[this.Object] = this.Id;
			}
			else
			{
				json = (JObject)JToken.FromObject(this, new JsonSerializer()
				{
					NullValueHandling = NullValueHandling.Ignore,
					ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
					Converters = { new StripeDateTimeConverter() }
				});
			}
			return json;
		}

		public virtual object FromJson(string json)
		{
			return JsonConvert.DeserializeObject(json, 
												this.GetType(), 
												new JsonSerializerSettings()
													{
														ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
														Converters = { new StripeDateTimeConverter() }
													});
		}
	}

	public class StripeList<T> : StripeObject where T : StripeObject, new()
	{
		[JsonIgnore]
		public override string Object { get { return "list"; } }

		public int? TotalCount { get; set; }
		public bool? HasMore { get; set; }
		public string Url { get; set; }
		[JsonIgnore]
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

		public override JObject ToJson()
		{
			var json = base.ToJson();
			if (Data != null && Data.Count > 0)
			{
				var arr = new JArray();
				Data.ForEach(d => arr.Add(d.ToJson()));
				json["data"] = arr;
			}

			return json;
		}

		public override object FromJson(string json)
		{
			var result = (StripeList<T>)base.FromJson(json);
			var jobj = JObject.Parse(json);
			var listItems = jobj["data"];
			if (listItems != null)
			{
				result.Data = new List<T>();
				foreach (var item in listItems)
				{
					result.Data.Add((T)new T().FromJson(item.ToString()));
				}
				//result.Subscriptions = (StripeList<StripeSubscription>)new StripeList<StripeSubscription>().FromJson(subList.ToString());
			}

			return result;
		}
	}

	public class StripeCreditCard : StripeObject
	{
		[JsonIgnore]
		public override string Object{ get { return "card"; } }

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
		[JsonIgnore]
		public override string Object { get { return "customer"; } }
		
		[JsonIgnore]
		public StripeList<StripeSubscription> Subscriptions { get; set; }
		[JsonIgnore]
		public StripeList<StripeCreditCard> Cards { get; set; }

		public StripeCreditCard Card { get; set; }
		public StripeDiscount Discount { get; set; }

		public string Email { get; set; }
		public DateTime? Created { get; set; }
		public bool? Delinquent { get; set; }
		public decimal? AccountBalance { get; set; }
		public string Currency { get; set; }
		
		[JsonProperty("default_card")]
		public string DefaultCardId { get; set; }

		public override JObject ToJson()
		{
			var json = base.ToJson();
			if (Subscriptions != null && Subscriptions.Data != null && Subscriptions.Data.Count > 0)
			{
				json["subscriptions"] = Subscriptions.ToJson();
			}
			if (Cards != null && Cards.Data != null && Cards.Data.Count > 0)
			{
				json["cards"] = Cards.ToJson();
			}
			return json;
		}

		public override object FromJson(string json)
		{
			var result = (StripeCustomer)base.FromJson(json);
			var jobj = JObject.Parse(json);
			var subList = jobj["subscriptions"];
			if (subList != null)
			{
				result.Subscriptions = (StripeList<StripeSubscription>)new StripeList<StripeSubscription>().FromJson(subList.ToString());
			}

			var cardList = jobj["cards"];
			if (cardList != null)
			{
				result.Cards = (StripeList<StripeCreditCard>)new StripeList<StripeCreditCard>().FromJson(cardList.ToString());
			}

			return result;
		}

	}

	public class StripeInvoice : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "invoice"; } }

		public string CustomerId { get; set; }
		public string ChargeId { get; set; }
		public string SubscriptionId { get; set; }

		public DateTime? Date { get; set; }
		public DateTime? PeriodStart { get; set; }
		public DateTime? PeriodEnd { get; set; }
		public DateTime? NextPaymentAttempt { get; set; }
		
		public Decimal? Subtotal { get; set; }
		public Decimal? Total { get; set; }
		public Decimal? AmountDue { get; set; }
		public Decimal? StartingBalance { get; set; }
		public Decimal? EndingBalance { get; set; }
		public Decimal? ApplicationFee { get; set; }

		public int? AttemptCount { get; set; }
		public bool? Attempted { get; set; }
		public bool? Closed { get; set; }
		public bool? Paid { get; set; }

		public StripeDiscount Discount { get; set; }

		[JsonIgnore]
		public StripeList<StripeInvoiceLineItem> LineItems { get; set; }

		public override JObject ToJson()
		{
			var json = base.ToJson();
			if (LineItems != null && LineItems.Data != null && LineItems.Data.Count > 0)
			{
				json["lines"] = LineItems.ToJson();
			}
			return json;
		}

		public override object FromJson(string json)
		{
			var result = (StripeInvoice)base.FromJson(json);
			var jobj = JObject.Parse(json);
			var lineList = jobj["lines"];
			if (lineList != null)
			{
				result.LineItems = (StripeList<StripeInvoiceLineItem>)new StripeList<StripeInvoiceLineItem>().FromJson(lineList.ToString());
			}

			return result;
		}	
	}

	public class StripeInvoiceLineItem : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "line_item"; } }

		public string Type { get; set; }
		public Decimal? Amount { get; set; }
		public string Currency { get; set; }
		public bool? Proration { get; set; }
		public int? Quantity { get; set; }

		public StripePlan Plan { get; set; }
		public StripePeriod Period { get; set; }
	}

	public class StripeCharge : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "charge"; } }
		
		public string CustomerId { get; set; }
		public string InvoiceId { get; set; }
		public string BalanceTransactionId { get; set; }

		public DateTime? Created { get; set; }
		public bool? Paid { get; set; }
		public bool? Captured { get; set; }
		public bool? Refunded { get; set; }

		public int? Amount { get; set; }
		public int? AmountRefunded { get; set; }

		public string Currency { get; set; }
		public string FailureMessage{ get; set; }
		public string FailureCode { get; set; }
		public string StatementDescription { get; set; }

		public List<StripeRefund> refunds { get; set; }
		public StripeCreditCard card { get; set; }
		public StripeDispute Dispute { get; set; }
	}

	public class StripeRefund : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "refund"; } }
	}
	public class StripeDispute : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "dispute"; } }
	}
	public class StripeCoupon : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "coupon"; } }

		public DateTime? Created { get; set; }
		public int? PercentOff { get; set; }
		public int? AmountOff { get; set; }
		public string Currency { get; set; }
		public string Duration { get; set; }
		public int? DurationInMonths { get; set; }
		public int? MaxRedemptions { get; set; }
		public int? TimesRedeemed { get; set; }
		public DateTime? RedeemBy { get; set; }
		public bool? Valid { get; set; }
	}

	public class StripeDiscount : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "discount"; } }

		public StripeCoupon Coupon { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public string Customer { get; set; }
		public string Subscription { get; set; }
	}

	public class StripeSubscriptionUpdate : StripeObject
	{
		[JsonProperty("prorate")]
		public bool? ProrateOnUpdate { get; set; }
		[JsonProperty("plan")]
		public string PlanId { get; set; }
		[JsonProperty("coupon")]
		public string CouponCode { get; set; }
		public int? Quantity { get; set; }
		public DateTime? TrialEnd { get; set; }
		[JsonIgnore]
		public string NewCardToken { get; set; }
		[JsonIgnore]
		public StripeCreditCard NewCard { get; set; }

		public StripeSubscriptionUpdate(string subscriptionId)
		{
			this.Id = subscriptionId;
		}

		public override JObject ToJson()
		{
			var json = base.ToJson();
			if (NewCard != null)
			{
				json["card"] = NewCard.ToJson();
			}
			else if (!String.IsNullOrWhiteSpace(NewCardToken))
			{
				json["card"] = NewCardToken;
			}
			return json;
		}

	}
	public class StripeSubscription : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "subscription"; } }		

		[JsonIgnore]
		public StripePlan Plan {get;set;}
		public StripeDiscount Discount { get; set; }

		public string CustomerId { get; set; }
		public int? Quantity { get; set; }
		
		public bool? CancelAtPeriodEnd { get; set; }
		public string Status { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? CurrentPeriodStart { get; set; }
		public DateTime? CurrentPeriodEnd { get; set; }
		public DateTime? EndedAt { get; set; }
		public DateTime? TrialStart { get; set; }
		public DateTime? TrialEnd { get; set; }
		public DateTime? CanceledAt { get; set; }

		public decimal? ApplicationFeePercent { get; set; }

		public JObject ToJson()
		{
			var json = base.ToJson();
			if (Plan != null && !String.IsNullOrWhiteSpace(Plan.Id))
			{
				if (Plan.IdOnly)
				{
					json["plan"] = Plan.Id;
				}
				else
				{
					json["plan"] = Plan.ToJson();
				}
			}

			return json;
		}

		public StripeSubscription FromJson(string json)
		{
			var result = (StripeSubscription)base.FromJson(json);
			var jobj = JObject.Parse(json);
			var planVal = jobj["plan"];
			if (planVal != null)
			{
				if (planVal["object"] != null && planVal["object"].ToString() == "plan")
				{
					result.Plan = (StripePlan)new StripePlan().FromJson(planVal.ToString());
				}
				else
				{
					result.Plan = new StripePlan() { Id = planVal.ToString(), IdOnly = true };
				}
			}

			return result;
		}
	}
	public class StripePlan : StripeObject
	{
		[JsonIgnore]
		public override string Object { get { return "plan"; } }

		public string Interval { get; set; }
		public int? IntervalCount { get; set; }
		public string Name { get; set; }
		public DateTime? Created { get; set; }
		public decimal? Amount { get; set; }
		public string Currency { get; set; }
		public int? TrialPeriodDays { get; set; }
		public string StatementDescription { get; set; }
	}
	
	public class StripeMoneyAmount
	{
		private decimal value;
		public decimal Value
		{
			get { return value; }
			set { this.value = value; }
		}
		public StripeMoneyAmount() { }
		public StripeMoneyAmount(decimal amount)
		{
			value = amount;
		}

		public override bool Equals(object other)
		{
			if (other == null) return false;
			return value.Equals(other);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static implicit operator StripeMoneyAmount(Decimal amount)
		{
			return new StripeMoneyAmount(amount);
		}

		public static implicit operator decimal(StripeMoneyAmount amount)
		{
			return amount.Value;
		}

		public static explicit operator decimal(StripeMoneyAmount amount)
		{
			return amount.Value;
		}

		public static explicit operator StripeMoneyAmount(decimal amount)
		{
			return new StripeMoneyAmount(amount);
		}
	}
	public class StripePeriod
	{
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
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

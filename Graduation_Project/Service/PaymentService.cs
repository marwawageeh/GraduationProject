using Graduation_Project.Models;
using Stripe;

namespace Graduation_Project.Service
{
	public class PaymentService : IPaymentService
	{
		public async Task<PaymentResult> CreatePaymentIntentAsync(decimal amount, int rentalId)
		{
			var options = new PaymentIntentCreateOptions
			{
				Amount = (long)(amount * 100),
				Currency = "usd",
				PaymentMethodTypes = new List<string> { "card" },
				Metadata = new Dictionary<string, string>
				{
					{ "RentalId", rentalId.ToString() }
				}
			};

			var service = new PaymentIntentService();
			var paymentIntent = await service.CreateAsync(options);

			return new PaymentResult
			{
				PaymentIntentId = paymentIntent.Id,
				ClientSecret = paymentIntent.ClientSecret,
				PaymentUrl = "#"
			};
		}
		public async Task<PaymentResult> CreatePaymentIntentAsync(decimal amount, string referenceKey, int referenceId)
		{
			var options = new PaymentIntentCreateOptions
			{
				Amount = (long)(amount * 100),
				Currency = "egp",
				PaymentMethodTypes = new List<string> { "card" },
				Metadata = new Dictionary<string, string>
				{
					{ referenceKey, referenceId.ToString() }
				}
			};

			var service = new PaymentIntentService();
			var intent = await service.CreateAsync(options);

			return new PaymentResult
			{
				PaymentIntentId = intent.Id,
				ClientSecret = intent.ClientSecret
			};
		}

		public async Task<bool> VerifyPaymentAsync(string paymentIntentId)
		{
			var service = new PaymentIntentService();
			var intent = await service.GetAsync(paymentIntentId);
			return intent.Status == "succeeded";
		}

		//public async Task<bool> VerifyPaymentAsync(string paymentIntentId)
		//{
		//	var service = new PaymentIntentService();
		//	var intent = await service.GetAsync(paymentIntentId);
		//	return intent.Status == "succeeded";
		//}

	}
}

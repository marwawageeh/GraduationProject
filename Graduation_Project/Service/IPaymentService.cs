using Graduation_Project.Models;

namespace Graduation_Project.Service
{
	public interface IPaymentService
	{
		Task<PaymentResult> CreatePaymentIntentAsync(decimal amount, int rentalId);
		//Task<bool> VerifyPaymentAsync(string paymentIntentId);
		Task<PaymentResult> CreatePaymentIntentAsync(decimal amount, string referenceKey, int referenceId);
		Task<bool> VerifyPaymentAsync(string paymentIntentId);
	}
}

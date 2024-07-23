using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Twikey.Model;

namespace Twikey
{
    public class CustomerGateway : Gateway
    {
        public CustomerGateway(TwikeyClient twikeyClient) : base(twikeyClient)
        {
        }

        /// <summary>Update a customer via this request. It is not possible create a new customer or to merge customers using this request.</summary>
        /// <param name="customer">Customer details</param>
        /// <exception cref="IOException">When no connection could be made</exception>
        /// <exception cref="Twikey.TwikeyClient.UserException">When Twikey returns a user error (400)</exception>
        public async Task UpdateAsync(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.CustomerNumber))
            {
                throw new ArgumentException("Customer number cannot be null or empty");
            }

            var queryParamBuilder = new StringBuilder();
            queryParamBuilder.Append("l=").Append(customer.Lang).Append('&');
            queryParamBuilder.Append("email=").Append(customer.Email).Append('&');
            queryParamBuilder.Append("lastname=").Append(customer.Lastname).Append('&');
            queryParamBuilder.Append("firstname=").Append(customer.Firstname).Append('&');
            queryParamBuilder.Append("mobile=").Append(customer.Mobile).Append('&');
            queryParamBuilder.Append("address=").Append(customer.Street).Append('&');
            queryParamBuilder.Append("city=").Append(customer.City).Append('&');
            queryParamBuilder.Append("zip=").Append(customer.Zip).Append('&');
            queryParamBuilder.Append("country=").Append(customer.Country).Append('&');
            queryParamBuilder.Append("companyName=").Append(customer.CompanyName).Append('&');
            queryParamBuilder.Append("coc=").Append(customer.Coc);

            HttpRequestMessage request = new()
            {
                RequestUri = _twikeyClient.GetUrl($"/customer/{customer.CustomerNumber}?{queryParamBuilder}"),
                Method = HttpMethod.Patch
            };
            request.Headers.Add("User-Agent", _twikeyClient.UserAgent);
            request.Headers.Add("Authorization", await _twikeyClient.GetSessionToken());

            HttpResponseMessage response = await _twikeyClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return;
            }

            string apiError = response.Headers.GetValues("ApiError").First();
            throw new TwikeyClient.UserException(apiError);
        }
    }
}

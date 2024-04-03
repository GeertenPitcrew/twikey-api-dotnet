using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using Twikey.Model;
using System.Threading.Tasks;

namespace Twikey
{
    public class DocumentGateway : Gateway
    {
        protected internal DocumentGateway(TwikeyClient twikeyClient): base(twikeyClient){}

        /// <param name="ct">Template to use can be found @ https://www.twikey.com/r/admin#/c/template</param>
        /// <param name="customer">Customer details</param>
        /// <param cref="MandateRequest">MandateRequest containing details about the request</param>
        /// <exception cref="IOException">When no connection could be made</exception>
        /// <exception cref="Twikey.TwikeyClient.UserException">When Twikey returns a user error (400)</exception>
        /// <returns>Url to redirect the customer to or to send in an email</returns>
        public SignableMandate Create(Customer customer, MandateRequest mandaterequest)
        {
            return CreateAsync(customer, mandaterequest).Result;
        }

        /// <param name="ct">Template to use can be found @ https://www.twikey.com/r/admin#/c/template</param>
        /// <param name="customer">Customer details</param>
        /// <param cref="MandateRequest">MandateRequest containing details about the request</param>
        /// <exception cref="IOException">When no connection could be made</exception>
        /// <exception cref="Twikey.TwikeyClient.UserException">When Twikey returns a user error (400)</exception>
        /// <returns>Url to redirect the customer to or to send in an email</returns>
        public async Task<SignableMandate> CreateAsync(Customer customer, MandateRequest mandaterequest)
        {
            var parameters = new Dictionary<string, string>();
            AddIfExists(parameters,"ct", mandaterequest.Ct);
            AddIfExists(parameters,"tc", mandaterequest.Tc);
            AddIfExists(parameters,"iban", mandaterequest.Iban);
            AddIfExists(parameters,"bic", mandaterequest.Bic);
            AddIfExists(parameters,"mandateNumber", mandaterequest.MandateNumber);
            AddIfExists(parameters,"contractNumber", mandaterequest.ContractNumber);
            AddIfExists(parameters,"campaign", mandaterequest.Campaign);
            AddIfExists(parameters,"prefix", mandaterequest.Prefix);
            AddIfExists(parameters,"check", mandaterequest.Check);
            AddIfExists(parameters,"reminderDays", mandaterequest.ReminderDays);
            AddIfExists(parameters,"sendInvite", mandaterequest.SendInvite);
            AddIfExists(parameters,"document", mandaterequest.Document);
            AddIfExists(parameters,"amount", mandaterequest.Amount);
            AddIfExists(parameters,"token", mandaterequest.Token);
            AddIfExists(parameters,"requireValidation", mandaterequest.RequireValidation.ToString());

            if (customer != null)
            {
                AddIfExists(parameters,"customerNumber", customer.CustomerNumber);
                AddIfExists(parameters,"email", customer.Email);
                AddIfExists(parameters,"firstname", customer.Firstname);
                AddIfExists(parameters,"lastname", customer.Lastname);
                AddIfExists(parameters,"l", customer.Lang);
                AddIfExists(parameters,"address", customer.Street);
                AddIfExists(parameters,"city", customer.City);
                AddIfExists(parameters,"zip", customer.Zip);
                AddIfExists(parameters,"country", customer.Country);
                AddIfExists(parameters,"mobile", customer.Mobile);
                if (customer.CompanyName != null)
                {
                    AddIfExists(parameters,"companyName", customer.CompanyName);
                    AddIfExists(parameters,"coc", customer.Coc);
                }
            }

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = _twikeyClient.GetUrl("/invite");
            request.Method = HttpMethod.Post;
            request.Headers.Add("User-Agent", _twikeyClient.UserAgent);
            request.Headers.Add("Authorization", await _twikeyClient.GetSessionToken());

            request.Content = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await _twikeyClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SignableMandate>(responseString);
            }

            string apiError = response.Headers.GetValues("ApiError").First();
            throw new TwikeyClient.UserException(apiError);

        }

        /// Get updates about all mandates (new/updated/cancelled)
        /// <param name="xTypes">Array of x-types. For example CORE,CREDITCARD</param>
        /// <exception cref="IOException">When a network issue happened</exception>
        /// <exception cref="Twikey.TwikeyClient.UserException">When there was an issue while retrieving the mandates (eg. invalid apikey)</exception>
        public IEnumerable<MandateFeedMessage> Feed(params string[] xTypes)
        {
            bool isEmpty;
            do
            {
                var messages = FeedAsync(xTypes).Result;

                foreach(var message in messages)
                {
                    yield return message;
                }
                isEmpty = !messages.Any();

            } while (!isEmpty);
        }

        /// Get updates about all mandates (new/updated/cancelled)
        /// <param name="xTypes">Array of x-types. For example CORE,CREDITCARD</param>
        /// <exception cref="IOException">When a network issue happened</exception>
        /// <exception cref="Twikey.TwikeyClient.UserException">When there was an issue while retrieving the mandates (eg. invalid apikey)</exception>
        public async Task<IEnumerable<MandateFeedMessage>> FeedAsync(params string[] xTypes)
        {
            Uri myUrl = _twikeyClient.GetUrl("/mandate");
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = myUrl;
            request.Method = HttpMethod.Get;
            request.Headers.Add("User-Agent", _twikeyClient.UserAgent);
            request.Headers.Add("Authorization", await _twikeyClient.GetSessionToken());
            if (xTypes != null && xTypes.Length != 0)
                request.Headers.Add("X-TYPES", string.Join(",", xTypes));

            HttpResponseMessage response = await _twikeyClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var feed = JsonConvert.DeserializeObject<MandateFeed>(responseString);
                
                return feed.Messages;
            }
            else
            {
                string apiError = response.Headers.GetValues("ApiError").FirstOrDefault();
                throw new TwikeyClient.UserException(apiError);
            }
        }

        public void CancelMandate(string mandateId, string reason, bool notify = false)
        {
            CancelMandateAsync(mandateId, reason, notify).RunSynchronously();
        }

        public async Task CancelMandateAsync(string mandateId, string reason, bool notify = false)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = _twikeyClient.GetUrl($"/mandate?mndtId={mandateId}&rsn={reason}{(notify ? "notify=true" : string.Empty)}");
            request.Method = HttpMethod.Delete;
            request.Headers.Add("User-Agent", _twikeyClient.UserAgent);
            request.Headers.Add("Authorization", await _twikeyClient.GetSessionToken());

            HttpResponseMessage response = await _twikeyClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return;
            }
            else
            {
                var apiError = response.Headers.GetValues("ApiError").FirstOrDefault();
                throw new TwikeyClient.UserException(apiError);
            }
        }
    }
}

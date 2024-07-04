using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Twikey;
using Twikey.Model;

namespace TwikeyAPITests
{
    [TestClass]
    public class CustomerTests
    {
        private static readonly string s_testVersion = "twikey-test/.net-0.1.0";
        private static readonly string _apiKey = Environment.GetEnvironmentVariable("TWIKEY_API_KEY"); // found in https://www.twikey.com/r/admin#/c/settings/api
        private static TwikeyClient _api;

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            _api = new TwikeyClient(_apiKey, true).WithUserAgent("twikey-api-dotnet/msunit");
        }

        [TestMethod]
        public async Task Update_Customer()
        {
            if (_apiKey == null)
            {
                Assert.Inconclusive("apiKey is null");
                return;
            }

            var customer = ConstructCustomer();
            customer.CustomerNumber = "200054";
            await _api.Customer.UpdateAsync(customer);
        }

        [TestMethod]
        public async Task Update_Customer_should_throw_exception_when_customernumber_is_null_or_empty()
        {
            if (_apiKey == null)
            {
                Assert.Inconclusive("apiKey is null");
                return;
            }

            var customer = ConstructCustomer();
            customer.CustomerNumber = null;

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _api.Customer.UpdateAsync(customer));
        }

        [TestMethod]
        public async Task Update_Customer_should_throw_exception_when_customer_not_found()
        {
            if (_apiKey == null)
            {
                Assert.Inconclusive("apiKey is null");
                return;
            }

            var customer = ConstructCustomer();
            customer.CustomerNumber = "347343";

            var exception = await Assert.ThrowsExceptionAsync<TwikeyClient.UserException>(async () => await _api.Customer.UpdateAsync(customer));
            Assert.AreEqual("err_debtor_not_found", exception.Message);
        }

        private static Customer ConstructCustomer()
        {
            return new Customer()
            {
                CustomerNumber = s_testVersion,
                Email = "no-reply@example.com",
                Firstname = "Twikey",
                Lastname = "Support",
                Street = "Derbystraat 43",
                City = "Gent",
                Zip = "9000",
                Country = "BE",
                Lang = "nl",
                Mobile = "32498665995",
                CompanyName = "Twikey API Tests"
            };
        }
    }
}

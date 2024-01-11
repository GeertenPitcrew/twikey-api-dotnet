using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Twikey;
using Twikey.Model;
using Newtonsoft.Json;

namespace TwikeyAPITests
{
    [TestClass]
    public class InvoiceTest
    {
        private static readonly string s_testVersion = "twikey-test/.net-0.1.0";
        private static readonly string _apiKey = Environment.GetEnvironmentVariable("TWIKEY_API_KEY"); // found in https://www.twikey.com/r/admin#/c/settings/api
        private static readonly long _ct = Environment.GetEnvironmentVariable("CT") == null ? 0L : Convert.ToInt64(Environment.GetEnvironmentVariable("CT")); // found @ https://www.twikey.com/r/admin#/c/template
        private static readonly string _mandateNumber = Environment.GetEnvironmentVariable("MNDTNUMBER");
        private static Customer _customer;
        private static TwikeyClient _api;

        [ClassInitialize]
        public static void CreateCustomer(TestContext testContext)
        {
            _customer = new Customer()
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
                Mobile = "32498665995"
            };
            _api = new TwikeyClient(_apiKey, true).WithUserAgent("twikey-api-dotnet/msunit");

        }

        [TestMethod]
        public void TestCreateInvoice()
        {
            if (_apiKey == null)
            {
                Assert.Inconclusive("apiKey is null");
                return;
            }
            var invoice = new Invoice()
            {
                Number = "Invoice"+DateTime.Now.ToString("yyyyMMdd"),
                Title = "Invoice April",
                Remittance = s_testVersion,
                Amount = 10.90,
                Date = DateTime.Now,
                Duedate = DateTime.Now.AddDays(30),
            };

            invoice = _api.Invoice.Create(_ct, _customer, invoice);
            Console.WriteLine("New invoice: " + JsonConvert.SerializeObject(invoice, Formatting.Indented));
        }

        [TestMethod]
        public void TestCreateInvoiceWithCustomerNullEmtpyFields()
        {
            if (_apiKey == null)
            {
                Assert.Inconclusive("apiKey is null");
                return;
            }
            Customer customer = new Customer()
            {
                Email = "no-reply@example.com",
                Firstname = "Twikey",
                Lastname = "Support",
                Street = "Derbystraat 43",
                City = "Gent",
                Zip = "9000",
                Mobile = null
            };
            var invoice = new Invoice()
            {
                Number = "Invoice-2-"+DateTime.Now.ToString("yyyyMMdd"),
                Title = "Invoice April",
                Remittance = s_testVersion,
                Amount = 10.90,
                Date = DateTime.Now,
                Duedate = DateTime.Now.AddDays(30),
                Customer = customer,
            };

            invoice = _api.Invoice.Create(_ct, customer, invoice);
            Console.WriteLine("New invoice: " + JsonConvert.SerializeObject(invoice, Formatting.Indented));
        }

        [TestMethod]
        public void GetInvoiceAndDetails(){
            if (_apiKey == null)
            {
                Assert.Inconclusive("apiKey is null");
                return;
            }
            foreach(var invoice in _api.Invoice.Feed())
            {
                Console.WriteLine("Updated invoice: " + JsonConvert.SerializeObject(invoice, Formatting.Indented));
            }
        }
    }
}

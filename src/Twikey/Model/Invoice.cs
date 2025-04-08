using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Twikey.Model.Parameters;

namespace Twikey.Model
{
    public class InvoiceUpdates
    {
        public IEnumerable<Invoice> Invoices { get; set;}
    }

    public class Invoice
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("remittance")]
        public string Remittance { get; set; }

        [JsonProperty("ct")]
        public long Ct { get; set; }

        [JsonProperty("manual", NullValueHandling = NullValueHandling.Ignore)]
        public bool Manual { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("date")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Date { get; set; }

        [JsonProperty("duedate", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Duedate { get; set; }

        [JsonProperty("ref", NullValueHandling = NullValueHandling.Ignore)]
        public string Ref { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("customerByDocument", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerByDocument { get; set; }

        [JsonProperty("customer", NullValueHandling = NullValueHandling.Ignore)]
        public Customer Customer { get; set; }

        [JsonProperty("pdf", NullValueHandling = NullValueHandling.Ignore)]
        public Byte[] Pdf { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore]
        public Meta Meta { get; set; }

        [JsonProperty("lastpayment", NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore]
        public List<LastPayment> LastPayment { get; set; }
    }

    public class CustomDateTimeConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}


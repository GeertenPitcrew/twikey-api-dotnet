using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Twikey.Model.Parameters;

public class LastPayment
{
    [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
    public string PaymentMethod { get; set; }
    
    [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
    public string Action { get; set; }
    
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public int Id { get; set; }
    
    [JsonProperty("e2e", NullValueHandling = NullValueHandling.Ignore)]
    public string EndToEndReference { get; set; }
    
    [JsonProperty("pmtinf", NullValueHandling = NullValueHandling.Ignore)]
    public string PaymentInformation { get; set; }
    
    [JsonProperty("iban", NullValueHandling = NullValueHandling.Ignore)]
    public string Iban { get; set; }
    
    [JsonProperty("bic", NullValueHandling = NullValueHandling.Ignore)]
    public string Bic { get; set; }
    
    [JsonProperty("rc", NullValueHandling = NullValueHandling.Ignore)]
    public string ReturnCode { get; set; }
    
    [JsonProperty("link", NullValueHandling = NullValueHandling.Ignore)]
    public string PaymentLink { get; set; }
    
    [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
    public string Message { get; set; }
    
    [JsonProperty("mndtId", NullValueHandling = NullValueHandling.Ignore)]
    public string MandateId { get; set; }
    
    [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime TransactionDate { get; set; }
    
}
using Newtonsoft.Json;

namespace Twikey.Model.Parameters;

public class Meta
{
    [JsonProperty("reminderLevel", NullValueHandling = NullValueHandling.Ignore)]
    public int ReminderLevel { get; set; }
    
    [JsonProperty("partial", NullValueHandling = NullValueHandling.Ignore)]
    public string Partial { get; set; }
    
    [JsonProperty("lastError", NullValueHandling = NullValueHandling.Ignore)]
    public string LastError { get; set; }
    
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
    
    [JsonProperty("paymentMethod", NullValueHandling = NullValueHandling.Ignore)]
    public string PaymentMethod { get; set; }
}
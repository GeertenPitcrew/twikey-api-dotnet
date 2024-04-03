<p align="center">
  <img src="https://cdn.twikey.com/img/logo.png" height="64"/>
</p>
<h1 align="center">Twikey API client for .NET</h1>

Want to allow your customers to pay in the most convenient way, then Twikey is right what you need.

Recurring or occasional payments via (Recurring) Credit Card, SEPA Direct Debit or any other payment method by bringing 
your own payment service provider or by leveraging your bank contract.

Twikey offers a simple and safe multichannel solution to negotiate and collect recurring (or even occasional) payments.
Twikey has integrations with a lot of accounting and CRM packages. It is the first and only provider to operate on a
European level for Direct Debit and can work directly with all major Belgian and Dutch Banks. However you can use the
payment options of your favorite PSP to allow other customers to pay as well.

## Requirements ##

To use the Twikey API client, the following things are required:

+ Get yourself a [Twikey account](https://www.twikey.com).
+ .NET Framework >= 4.6.1, .NET Core >= 3.0 or .NET >= 5.0
+ Up-to-date OpenSSL (or other SSL/TLS toolkit)

## Installation 

Check [NuGet packages](https://www.nuget.org/packages/TwikeySDK/)

### Using the [.NET Core command-line interface (CLI) tools](https://docs.microsoft.com/en-us/dotnet/core/tools/):
```bash
dotnet add package TwikeySDK
```

### Using the [NuGet Command Line Interface (CLI)](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference):

```bash
nuget install TwikeySDK
```

### Using the [Package Manager Console](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell):

```bash
Install-Package TwikeySDK
```

## How to create anything ##

The api works the same way regardless if you want to create a mandate, a transaction, an invoice or even a paylink.
the following steps should be implemented:

1. Use the Twikey API client to create or import your item.

2. Once available, our platform will send an asynchronous request to the configured webhook
   to allow the details to be retrieved. As there may be multiple items ready for you a "feed" endpoint is provided
   which acts like a queue that can be read until empty till the next time.

3. The customer returns, and should be satisfied to see that the action he took is completed.

Find our full documentation online on [api.twikey.com](https://api.twikey.com).

## Getting started ##

Initializing the Twikey API client using the Requests library. 
and configure your API key which you can find in the [Twikey merchant interface](https://www.twikey.com).

```csharp
TwikeyClient twikeyClient = new TwikeyClient(apiKey).WithUserAgent("myApp");
``` 

## Documents

Invite a customer to sign a SEPA mandate using a specific behaviour template (ct) that allows you to configure 
the behaviour or flow that the customer will experience. This 'ct' can be found in the template section of the settings.

```csharp
var customer = new Customer()
{
    CustomerNumber = "customerNum123",
    Email = "no-reply@example.com",
    Firstname = "Twikey",
    Lastname = "Support",
    Street = "Derbystraat 43",
    City = "Gent",
    Zip = "9000",
    Country = "BE",
    Lang = "nl",
    Mobile = "32412345678"
};
var profile = 123; // Profile to use
var request = new MandateRequest(profile); // Can contain optional account data
SignableMandate invite = await twikeyClient.Document.CreateAsync(customer, request); 
```

_After creation, the link available in invite.Url can be used to redirect the customer into the signing flow or even 
send him a link through any other mechanism. Ideally you store the mandatenumber for future usage (eg. sending transactions)._


### Feed


```csharp
//Generator to work with response from Twikey
foreach(var mandateUpdate in await twikeyClient.Document.FeedAsync())
{
    if(mandateUpdate.IsNew())
    {
        Console.WriteLine("New mandate: " + JsonConvert.SerializeObject(mandateUpdate, Formatting.Indented));
    }
    else if(mandateUpdate.IsUpdated())
    {
        Console.WriteLine("Updated mandate: " + JsonConvert.SerializeObject(mandateUpdate, Formatting.Indented));
    }
    else if(mandateUpdate.IsCancelled())
    {
        Console.WriteLine("Cancelled mandate: " + JsonConvert.SerializeObject(mandateUpdate, Formatting.Indented));
    }
}
```

## Invoices
Create new invoices 

```csharp
var customer = new Customer()
{
    CustomerNumber = "customerNum123",
    Email = "no-reply@example.com",
    Firstname = "Twikey",
    Lastname = "Support",
    Street = "Derbystraat 43",
    City = "Gent",
    Zip = "9000",
    Country = "BE",
    Lang = "nl",
    Mobile = "32412345678"
};

var invoice = new Invoice()
{
    Number = "Invoice 123",
    Title = "Invoice April",
    Remittance = "123564984",
    Amount = 10.90,
    Date = DateTime.Now,
    Duedate = DateTime.Now.AddDays(30),
};
await twikeyClient.Invoice.CreateAsync(customer, invoice);
```

### Feed

Retrieve the list of updates on invoices that had changes since the last call.

```csharp
//Generator to work with response from Twikey
foreach(var invoice in await twikeyClient.Invoice.FeedAsync())
{
    Console.WriteLine("Updated invoice: " + invoice);
}
```

## Paymentlinks
Create a payment link 

**You need an integration, for example iDeal**

```csharp
var request = new PaylinkRequest("Your payment", 10.55);
await twikeyClient.Paylink.CreateAsync(customer, request);

```

### Feed

Get payment link feed since the last retrieval

```csharp
//Generator to work with response from Twikey
foreach(var link in await twikeyClient.Paylink.FeedAsync())
{
   if(link.IsPaid())
   {
     Console.WriteLine("Paid paylink: " + link);
   }
   else
   {
     Console.WriteLine("Updated Paylink: " + link);
   }
}

```

## Transactions

Send new transactions and act upon feedback from the bank.

```csharp
var request = new TransactionRequest("MyMessage",10.55);
await twikeyClient.Transaction.CreateAsync(mandateNumber, request);
```

### Feed

```csharp
//Generator to work with response from Twikey
foreach(var transaction in await twikeyClient.Transaction.FeedAsync())
{
    Console.WriteLine("Updated Transaction: " + transaction);
}

```

## Webhook ##

When wants to inform you about new updates about documents or payments a `webhookUrl` specified in your api settings be called.  

```csharp
/**
    * Formats query string from TwiKey to a query string for the webhook request validation
    * @return query string for the webhook request validation, for example msg=dummytest&type=event
    */
private string ParseTwiKeyCreateQuerySubString()
{
    var queryParameters = HttpUtility.UrlDecode(Request.QueryString.Value);
    
    // question mark needs to be removed
    if(queryParameters != null && queryParameters[0] == '?')
    {
        queryParameters = queryParameters.Substring(1);
    }
    return queryParameters;
}
string incomingSignature = Request.Headers["X-SIGNATURE"].First();

// query parameter needs to be in the following format "msg=dummytest&type=event"
string payload = ParseTwiKeyCreateQuerySubString();

bool valid = twikeyClient.VerifyWebHookSignature(incomingSignature,payload);
```

## API documentation ##

If you wish to learn more about our API, please visit the [Twikey Api Page](https://api.twikey.com).
API Documentation is available in English.

## Want to help us make our API client even better? ##

Want to help us make our API client even better? We
take [pull requests](https://github.com/twikey/twikey-api-python/pulls). 

## Support ##

Contact: [www.twikey.com](https://www.twikey.com)

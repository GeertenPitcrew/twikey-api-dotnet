using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Twikey
{
    public class TwikeyClient
    {
        //private static readonly string s_Utf8 = "UTF-8";
        private static readonly string s_defaultUserHeader = "twikey/.net-0.1.0";
        private static readonly string s_prodEnvironment = "https://api.twikey.com/creditor";
        private static readonly string s_testEnvironment = "https://api.beta.twikey.com/creditor";
        private static readonly long s_maxSessionAge = 23 * 60 * 60 * 1000; // max 1day, but use 23 to be safe
        private static readonly HttpClient s_client = new HttpClient();
        private static readonly string s_saltOwn = "own";
        private readonly string _apiKey;
        private readonly string _endpoint;
        private long _lastLogin;
        private string _sessionToken;
        private string _privateKey;

        public static readonly string FORM_URL = "application/x-www-form-urlencoded";
        public static readonly string JSON = "application/json";
        public string UserAgent { get; private set; }
        public CustomerGateway Customer { get; }
        public DocumentGateway Document { get; }
        public InvoiceGateway Invoice { get; }
        public PaylinkGateway Paylink { get; }
        public TransactionGateway Transaction { get; }

        /// <param name="apiKey">API key</param>
        /// <param name="test">Use the test environment</param>
        public TwikeyClient(string apiKey, bool test)
        {
            _apiKey = apiKey;
            _endpoint = test ? s_testEnvironment : s_prodEnvironment;
            UserAgent = s_defaultUserHeader;
            Customer = new CustomerGateway(this);
            Document = new DocumentGateway(this);
            Invoice = new InvoiceGateway(this);
            Paylink = new PaylinkGateway(this);
            Transaction = new TransactionGateway(this);
        }

        /// <param name="apiKey"> API key</param>
        public TwikeyClient(string apikey) : this(apikey, false) { }

        public TwikeyClient WithUserAgent(string userAgent)
        {
            UserAgent = userAgent;
            //s_client.DefaultRequestHeaders.Add("User-Agent",userAgent);
            return this;
        }

        public TwikeyClient WithPrivateKey(string privateKey)
        {
            _privateKey = privateKey;
            return this;
        }

        protected internal async Task<string> GetSessionToken()
        {
            if ((JMethods.CurrentTimeMillis() - _lastLogin) > s_maxSessionAge)
            {
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(_endpoint);
                request.Method = HttpMethod.Post;
                request.Headers.Add("User-Agent", UserAgent);

                Dictionary<string, string> parameters = new Dictionary<string, string>() { { "apiToken", _apiKey } };
                if (_privateKey != null)
                {
                    long otp = GenerateOtp(s_saltOwn, _privateKey);
                    parameters.Add("otp", otp.ToString());
                }
                request.Content = new FormUrlEncodedContent(parameters);

                HttpResponseMessage response = await s_client.SendAsync(request);
                try
                {
                    _sessionToken = response.Headers.GetValues("Authorization").First<string>();
                    _lastLogin = JMethods.CurrentTimeMillis();
                }
                catch (Exception e)
                {
                    _lastLogin = 0L;
                    throw new UnauthenticatedException(e);
                }
            }
            return _sessionToken;
        }

        public Uri GetUrl(string path)
        {
            return new Uri(String.Format("{0}{1}", _endpoint, path));
        }

        public class UserException : Exception
        {
            public UserException(String apiError) : base(apiError) { }
            public UserException(String apiError, Exception e) : base(apiError, e) { }
        }

        public class UnauthenticatedException : UserException
        {
            public UnauthenticatedException(Exception e) : base("Not authenticated", e) { }
        }

        /// <param name="signatureHeader">Request.Headers["X-SIGNATURE"].First<string>()</param>
        /// <param name="queryString">Request.QueryString.Value with format "msg=dummytest&type=event"</param>
        /// <returns>True for valid signatures</returns>
        public bool VerifyWebHookSignature(string signatureHeader, string queryString)
        {
            byte[] providedSignature = JMethods.ParseHexBinary(signatureHeader);

            using (HMAC hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_apiKey)))
            {
                byte[] calculated = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                bool equal = true;
                for (int i = 0; i < calculated.Length; i++)
                {
                    equal = equal && (providedSignature[i] == calculated[i]);
                }
                return equal;
            }

        }

        /// <param name="websitekey">Provided in Settings - Website</param>
        /// <param name="document">Mandatenumber or other</param>
        /// <param name="status">Outcome of the request</param>
        /// <param name="token">If provided in the initial request</param>
        /// <param name="signature">Given in the exit url</param>
        /// <returns>Whether or not the signature is valid</returns>
        public static bool VerifyExiturlSignature(string websitekey, string document, string status, string token, string signature)
        {
            byte[] providedSignature = JMethods.ParseHexBinary(signature);

            using (HMAC hmac = new HMACSHA256(Encoding.UTF8.GetBytes(websitekey)))
            {
                string payload = String.Format("{0}/{1}", document, status);
                if (token != null)
                    payload = String.Format("{0}/{1}", payload, token);

                byte[] calculated = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                bool equal = true;
                for (int i = 0; i < calculated.Length; i++)
                {
                    equal = equal && (providedSignature[i] == calculated[i]);
                }
                return equal;
            }
        }

        /// <param name="websitekey">Provided in Settings - Website</param>
        /// <param name="document">Mandatenumber or other</param>
        /// <param name="encryptedAccount">encrypted account info</param>
        /// <returns>new String[]{iban,bic}</returns>
        public static string[] DecryptAccountInformation(string websitekey, string document, string encryptedAccount)
        {
            string key = document + websitekey;
            try
            {
                using (HashAlgorithm md5 = MD5.Create())
                {
                    byte[] keyBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
                    using (Aes cipher = Aes.Create())
                    {
                        cipher.Key = keyBytes;
                        cipher.Mode = CipherMode.CBC;
                        cipher.IV = keyBytes;
                        cipher.Padding = PaddingMode.PKCS7; // No PKCS5 but PKCS7 uses the same padding as 5

                        ICryptoTransform decryptor = cipher.CreateDecryptor();
                        using (MemoryStream memoryStream = new MemoryStream(JMethods.ParseHexBinary(encryptedAccount)))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                                {
                                    return streamReader.ReadToEnd().Split('/');
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new SystemException("Exception decrypting : " + encryptedAccount, e);
            }
        }


        public static long GenerateOtp(string salt, string privateKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException("Invalid key");

            long counter = (long)Math.Floor((double)JMethods.CurrentTimeMillis() / 30000);
            byte[] key = JMethods.ParseHexBinary(privateKey);

            if (salt != null)
            {
                byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
                byte[] key2 = new byte[saltBytes.Length + key.Length];
                Array.Copy(saltBytes, 0, key2, 0, saltBytes.Length);
                Array.Copy(key, 0, key2, saltBytes.Length, key.Length);
                key = key2;
            }


            using (HMAC hmac = new HMACSHA256(key))
            {
                byte[] counterAsBytes = new byte[8];
                for (int i = 7; i >= 0; --i)
                {
                    counterAsBytes[i] = (byte)(counter & 255);
                    counter = counter >> 8;
                }

                byte[] hash = hmac.ComputeHash(counterAsBytes);
                int offset = hash[19] & 0xf;
                long v = (hash[offset] & 0x7f) << 24 |
                         (hash[offset + 1] & 0xff) << 16 |
                         (hash[offset + 2] & 0xff) << 8 |
                         (hash[offset + 3] & 0xff);
                return v % 100000000;
            }
        }

        public HttpResponseMessage Send(HttpRequestMessage request)
        {
            return SendAsync(request).Result;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await s_client.SendAsync(request);
        }
    }

    public static class JMethods
    {
        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public static byte[] ParseHexBinary(string hexStr)
        {
            if (hexStr == null)
                throw new ArgumentNullException("Invalid hexStr");

            byte[] bytes = new byte[hexStr.Length / 2];

            for (int i = 0; i < hexStr.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexStr.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Twikey;
using Twikey.Model;
using Newtonsoft.Json;

namespace TwikeyAPITests
{
    [TestClass]
    public class TwikeyAPITest
    {
        [TestMethod]
        public void VerifySignatureAndDecryptAccountInfo()
        {
            // exiturl defined in template http://example.com?mandatenumber={{mandateNumber}}&status={{status}}&signature={{s}}&account={{account}}
            // outcome http://example.com?mandatenumber=MYDOC&status=ok&signature=8C56F94905BBC9E091CB6C4CEF4182F7E87BD94312D1DD16A61BF7C27C18F569&account=2D4727E936B5353CA89B908309686D74863521CAB32D76E8C2BDD338D3D44BBA

            // string outcome = "http://example.com?mandatenumber=MYDOC&status=ok&" +
            //        "signature=8C56F94905BBC9E091CB6C4CEF4182F7E87BD94312D1DD16A61BF7C27C18F569&" +
            //        "account=2D4727E936B5353CA89B908309686D74863521CAB32D76E8C2BDD338D3D44BBA";

            string websiteKey = "BE04823F732EDB2B7F82252DDAF6DE787D647B43A66AE97B32773F77CCF12765";
            string doc = "MYDOC";
            string status = "ok";

            string signatureInOutcome = "8C56F94905BBC9E091CB6C4CEF4182F7E87BD94312D1DD16A61BF7C27C18F569";
            string encryptedAccountInOutcome = "2D4727E936B5353CA89B908309686D74863521CAB32D76E8C2BDD338D3D44BBA";
            Assert.IsTrue(TwikeyClient.VerifyExiturlSignature(websiteKey, doc, status, null, signatureInOutcome));
            string[] ibanAndBic = TwikeyClient.DecryptAccountInformation(websiteKey, doc, encryptedAccountInOutcome);
            Assert.AreEqual("BE08001166979213", ibanAndBic[0]);
            Assert.AreEqual("GEBABEBB", ibanAndBic[1]);
        }
    }
}

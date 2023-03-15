using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using testDotNetApp.Controllers;
using testDotNetApp.Models;
using XSystem.Security.Cryptography;

namespace testDotNetApp.Helpers
{
    public class HelpersConfig
    {
        public bool IsPartnerAllowed(string partnerKey, string partnerPassword)
        {
            // Check if partner key and password are correct
            if ((partnerKey == "FAKEGOOGLE" && partnerPassword == "FAKEPASSWORD1234") ||
                (partnerKey == "FAKEPEOPLE" && partnerPassword == "FAKEPASSWORD4578"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GenerateSig(TrxMessage trxMessage)
        {
            var sortedParams = trxMessage.GetType().GetProperties()
                                    .Where(p => p.Name != "Timestamp" && p.Name != "Sig")
                                    .OrderBy(p => p.Name)
                                    .Select(p => $"{p.Name}={p.GetValue(trxMessage)}");

            var paramString = string.Join("&", sortedParams);
            var timestamp = DateTime.Parse(DateTime.Now.ToString()).ToUniversalTime().ToString();
            var sigTimestamp = $"{timestamp.Substring(0, 8)}{timestamp.Substring(9, 6)}";
            var concatenatedString = $"{paramString}&sigtimestamp={sigTimestamp}";
            var bytesToEncode = Encoding.UTF8.GetBytes(concatenatedString);
            var sha256 = new SHA256Managed();
            var hash = sha256.ComputeHash(bytesToEncode);
            var signature = Convert.ToBase64String(hash);

            return signature;
        }
    }
}

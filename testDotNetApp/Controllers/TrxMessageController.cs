using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using testDotNetApp.Models;

namespace testDotNetApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class TrxMessageController : ControllerBase
    {
        private readonly ILogger<TrxMessageController> _logger;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TrxMessageController));

        public TrxMessageController(ILogger<TrxMessageController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("submittrxmessage")]
        public IActionResult SubmitTrxMessage([FromBody] TrxMessage trxMessage)
        {
            // log4net saved the request body
            _log.Debug($"Request body: {JsonConvert.SerializeObject(trxMessage)}");

            // Check if the partner is allowed and the password is correct
            if (!IsPartnerAllowed(trxMessage.PartnerKey, trxMessage.PartnerPassword))
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "Access Denied!" }}");
                return BadRequest(new { result = 0, resultmessage = "Access Denied!" });
            }

            // Check if the mandatory parameter is not provided
            if (string.IsNullOrWhiteSpace(trxMessage.PartnerKey))
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "PartnerKey is Required." }}");
                return BadRequest(new { result = 0, resultmessage = "PartnerKey is Required." });
            }
            if (string.IsNullOrWhiteSpace(trxMessage.PartnerRefNo))
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "PartnerRefNo is Required." }}");
                return BadRequest(new { result = 0, resultmessage = "PartnerRefNo is Required." });
            }
            if (string.IsNullOrWhiteSpace(trxMessage.Sig))
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "Sig is Required." }}");
                return BadRequest(new { result = 0, resultmessage = "Sig is Required." });
            }
            if (string.IsNullOrWhiteSpace(trxMessage.PartnerPassword))
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "PartnerPassword is Required." }}");
                return BadRequest(new { result = 0, resultmessage = "PartnerPassword is Required." });
            }
            if (trxMessage.TotalAmount == null)
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "TotalAmount is Required." }}");
                return BadRequest(new { result = 0, resultmessage = "TotalAmount is Required." });
            }

            // Check if the total amount matches the sum of all item prices
            if (trxMessage.Items != null && trxMessage.Items.Any())
            {
                long sum = 0;
                foreach (var item in trxMessage.Items)
                {
                    // Check if the mandatory parameter is not provided
                    if (item.Qty == null)
                    {
                        _log.Debug($"Response body: {new { result = 0, resultmessage = "Qty is Required." }}");
                        return BadRequest(new { result = 0, resultmessage = "Qty is Required." });
                    }
                    if (item.UnitPrice == null)
                    {
                        _log.Debug($"Response body: {new { result = 0, resultmessage = "UnitPrice is Required." }}");
                        return BadRequest(new { result = 0, resultmessage = "UnitPrice is Required." });
                    }

                    // Check if the item price is valid
                    if (item.Qty <= 0 || item.Qty > 5 || item.UnitPrice <= 0)
                    {
                        _log.Debug($"Response body: {new { result = 0, resultmessage = "Invalid Total Amount." }}");
                        return BadRequest(new { result = 0, resultmessage = "Invalid Total Amount." });
                    }

                    sum += item.Qty.Value * item.UnitPrice.Value;
                }
                if (trxMessage.TotalAmount != sum)
                {
                    _log.Debug($"Response body: {new { result = 0, resultmessage = "Invalid Total Amount." }}");
                    return BadRequest(new { result = 0, resultmessage = "Invalid Total Amount." });
                }
            }

            // Check if the timestamp is within the valid range
            var serverTime = DateTime.UtcNow;
            var requestTime = DateTime.Parse(DateTime.Now.ToString()).ToUniversalTime();
            if (trxMessage.Timestamp != null)
            {
                requestTime = DateTime.Parse(trxMessage.Timestamp).ToUniversalTime();
            }
            if (requestTime < serverTime.AddMinutes(-5) || requestTime > serverTime.AddMinutes(5))
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "Expired." }}");
                return BadRequest(new { result = 0, resultmessage = "Expired." });
            }

            _log.Debug($"Response body: {new { result = 1, resultmessage = "Request data is valid." }}");
            return Ok(new { result = 1, resultmessage = "Request data is valid." });
        }

        private bool IsPartnerAllowed(string partnerKey, string partnerPassword)
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
    }
}

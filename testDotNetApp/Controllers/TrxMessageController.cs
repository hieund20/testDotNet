using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using testDotNetApp.Models;
using testDotNetApp.Helpers;
using System.Globalization;

namespace testDotNetApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class TrxMessageController : ControllerBase
    {
        private readonly ILogger<TrxMessageController> _logger;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TrxMessageController));
        HelpersConfig helpers = new HelpersConfig();

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
            if (!helpers.IsPartnerAllowed(trxMessage.PartnerKey, trxMessage.PartnerPassword))
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "Access Denied!" }}");
                return BadRequest(new { result = 0, resultmessage = "Access Denied!" });
            }

            // Check if the mandatory parameter is not provided
            foreach (var prop in trxMessage.GetType().GetProperties())
            {
                var propName = prop.Name;
                var propValue = prop.GetValue(trxMessage, null);

                if (propName == "Timestamp") continue;
                if (propValue == null)
                {
                    _log.Debug($"Response body: {new { result = 0, resultmessage = $"{propName} is Required." }}");
                    return BadRequest(new { result = 0, resultmessage = $"{propName} is Required." });
                }  
            };

            // Check if sig is invalid
            if (helpers.GenerateSig(trxMessage) != trxMessage.Sig)
            {
                _log.Debug($"Response body: {new { result = 0, resultmessage = "Access Denied!, Sig is invalid" }}");
                return BadRequest(new { result = 0, resultmessage = "Access Denied!, Sig is invalid" });
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
                DateTime timestamp;
                if (!DateTime.TryParseExact(trxMessage.Timestamp, "dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
                {
                    _log.Debug($"Response body: {new { result = 0, resultmessage = "Invalid Timestamp." }}");
                    return Ok(new { result = 0, resultmessage = "Invalid Timestamp." });
                }
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
    }
}

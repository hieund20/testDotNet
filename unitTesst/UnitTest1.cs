using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using testDotNetApp.Controllers;
using testDotNetApp.Models;

namespace unitTesst
{
    public class Tests
    {

        private TrxMessageController _controller;
        private Mock<ILogger<TrxMessageController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<TrxMessageController>>();
            _controller = new TrxMessageController(_loggerMock.Object);
        }

        [Test]
        public void SubmitTrxMessage_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var trxMessage = new TrxMessage
            {
                PartnerKey = "FAKEGOOGLE",
                PartnerPassword = "FAKEPASSWORD1234",
                PartnerRefNo = "FG-00001",
                Sig = "24XYSmvKGH9I9Y5FLvSsId2MPtjkvog7U5JLhE3m30A=",
                TotalAmount = 8,
                Items = new[]
                {
                    new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen 1", Qty = 2, UnitPrice = 2 },
                    new ItemDetail { PartnerItemRef = "i-00002", Name = "Pen 2", Qty = 2, UnitPrice = 2 },
                }
            };

            // Act
            var result = _controller.SubmitTrxMessage(trxMessage);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(1, okResult.Value.GetType().GetProperty("result").GetValue(okResult.Value));
            Assert.AreEqual("Request data is valid.", okResult.Value.GetType().GetProperty("resultmessage").GetValue(okResult.Value));
        }

        [Test]
        public void SubmitTrxMessage_WithInvalidPartnerKey_ReturnsBadRequest()
        {
            // Arrange
            var trxMessage = new TrxMessage
            {
                PartnerKey = "INVALIDKEY",
                PartnerPassword = "FAKEPASSWORD1234",
                PartnerRefNo = "FG-00001",
                Sig = "24XYSmvKGH9I9Y5FLvSsId2MPtjkvog7U5JLhE3m30A=",
                TotalAmount = 8,
                Items = new[]
                {
                    new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen 1", Qty = 2, UnitPrice = 2 },
                    new ItemDetail { PartnerItemRef = "i-00002", Name = "Pen 2", Qty = 2, UnitPrice = 2 },
                }
            };

            // Act
            var result = _controller.SubmitTrxMessage(trxMessage);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual(0, badRequestResult.Value.GetType().GetProperty("result").GetValue(badRequestResult.Value));
            Assert.AreEqual("Access Denied!", badRequestResult.Value.GetType().GetProperty("resultmessage").GetValue(badRequestResult.Value));
        }

        [Test]
        public void SubmitTrxMessage_WithInvalidPartnerPassword_ReturnsBadRequest()
        {
            // Arrange
            var trxMessage = new TrxMessage
            {
                PartnerKey = "FAKEGOOGLE",
                PartnerPassword = "INVALIDKEY",
                PartnerRefNo = "FG-00001",
                Sig = "24XYSmvKGH9I9Y5FLvSsId2MPtjkvog7U5JLhE3m30A=",
                TotalAmount = 8,
                Items = new[]
                {
                    new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen 1", Qty = 2, UnitPrice = 2 },
                    new ItemDetail { PartnerItemRef = "i-00002", Name = "Pen 2", Qty = 2, UnitPrice = 2 },
                }
            };

            // Act
            var result = _controller.SubmitTrxMessage(trxMessage);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual(0, badRequestResult.Value.GetType().GetProperty("result").GetValue(badRequestResult.Value));
            Assert.AreEqual("Access Denied!", badRequestResult.Value.GetType().GetProperty("resultmessage").GetValue(badRequestResult.Value));
        }

        [Test]
        public void SubmitTrxMessage_WithInvalidTotalAmount_ReturnsBadRequest()
        {
            // Arrange
            var trxMessage = new TrxMessage
            {
                PartnerKey = "FAKEGOOGLE",
                PartnerPassword = "FAKEPASSWORD1234",
                PartnerRefNo = "FG-00001",
                Sig = "24XYSmvKGH9I9Y5FLvSsId2MPtjkvog7U5JLhE3m30A=",
                TotalAmount = 10,
                Items = new[]
                {
                    new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen 1", Qty = 2, UnitPrice = 2 },
                    new ItemDetail { PartnerItemRef = "i-00002", Name = "Pen 2", Qty = 2, UnitPrice = 2 },
                }
            };

            // Act
            var result = _controller.SubmitTrxMessage(trxMessage);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual(0, badRequestResult.Value.GetType().GetProperty("result").GetValue(badRequestResult.Value));
            Assert.AreEqual("Invalid Total Amount.", badRequestResult.Value.GetType().GetProperty("resultmessage").GetValue(badRequestResult.Value));
        }

        [Test]
        public void SubmitTrxMessage_WithExpired_ReturnsBadRequest()
        {
            // Arrange
            var trxMessage = new TrxMessage
            {
                PartnerKey = "FAKEGOOGLE",
                PartnerPassword = "FAKEPASSWORD1234",
                PartnerRefNo = "FG-00001",
                Sig = "24XYSmvKGH9I9Y5FLvSsId2MPtjkvog7U5JLhE3m30A=",
                Timestamp = "2013-11-22T02:11:22.0000000Z",
                TotalAmount = 8,
                Items = new[]
                {
                    new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen 1", Qty = 2, UnitPrice = 2 },
                    new ItemDetail { PartnerItemRef = "i-00002", Name = "Pen 2", Qty = 2, UnitPrice = 2 },
                }
            };

            // Act
            var result = _controller.SubmitTrxMessage(trxMessage);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual(0, badRequestResult.Value.GetType().GetProperty("result").GetValue(badRequestResult.Value));
            Assert.AreEqual("Expired.", badRequestResult.Value.GetType().GetProperty("resultmessage").GetValue(badRequestResult.Value));
        }

        [Test]
        public void SubmitTrxMessage_WithSigInvalid_ReturnsBadRequest()
        {
            // Arrange
            var trxMessage = new TrxMessage
            {
                PartnerKey = "FAKEGOOGLE",
                PartnerPassword = "FAKEPASSWORD1234",
                PartnerRefNo = "FG-00001",
                Sig = "24XYSmvKGH9I9Y5FLvSsId2MPtjkvog7U5JLhE3m30A=",
                TotalAmount = 8,
                Items = new[]
                {
                    new ItemDetail { PartnerItemRef = "i-00001", Name = "Pen 1", Qty = 2, UnitPrice = 2 },
                    new ItemDetail { PartnerItemRef = "i-00002", Name = "Pen 2", Qty = 2, UnitPrice = 2 },
                }
            };

            // Act
            var result = _controller.SubmitTrxMessage(trxMessage);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual(0, badRequestResult.Value.GetType().GetProperty("result").GetValue(badRequestResult.Value));
            Assert.AreEqual("Access Denied!, Sig is invalid.", badRequestResult.Value.GetType().GetProperty("resultmessage").GetValue(badRequestResult.Value));
        }
    }
}
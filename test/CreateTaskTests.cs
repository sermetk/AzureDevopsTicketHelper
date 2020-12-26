using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace DevopsTicketHelper.Tests
{
    public class CreateTaskTests
    {
        private readonly Mock<Functions> mockFunctions;
        private readonly Mock<ILogger<Functions>> mockLogger;
        private readonly Mock<IHttpClientFactory> mockHttpClientFactory;

        public CreateTaskTests()
        {
            mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockLogger = new Mock<ILogger<Functions>>();
            mockFunctions = new Mock<Functions>(MockBehavior.Strict, new object[] { mockHttpClientFactory.Object, mockLogger.Object });
        }

        [Fact]
        public async Task Should_Throw_Http_Exception()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });
            var client = new HttpClient(mockHttpMessageHandler.Object);
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            var functions = mockFunctions.Object;
            await Assert.ThrowsAsync<HttpRequestException>(async ()  =>
            {
                await functions.CreateTestItemsFromBuildId(1);
            });
        }
    }
}

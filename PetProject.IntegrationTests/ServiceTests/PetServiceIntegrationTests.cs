using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PetProject.BusinessLayer.Configurations;
using PetProject.BusinessLayer.Helpers;
using PetProject.BusinessLayer.Services;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PetProject.IntegrationTests
{
    [TestClass]
    public class PetServiceIntegrationTests
    {
        private static PetService _petService;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
           
        }

        [ClassCleanup]
        public static void TearDown()
        {
            _petService = null;            
        }


        [TestMethod]
        public async Task TestMethod_GetPetList_GeneralHealthCheck()
        {
            _petService = CreatePetService();
            var petListResponse = await _petService.GetPetOwnersAsync();

            Assert.IsNotNull(petListResponse, "Response is null from PetList API");
            Assert.IsNotNull(petListResponse.Result, "Result is null from PetList API");
            Assert.IsTrue(petListResponse.Success, "Response Success is false");
            Assert.IsTrue(petListResponse.ErrorMessage.IsNullOrEmpty(), "Response Error Message is not null");
        }

        [TestMethod]
        public async Task TestMethod_GetPetList_ResultCheck()
        {
            _petService = CreatePetService();
            var petListResponse = await _petService.GetPetOwnersAsync();

            Assert.IsNotNull(petListResponse, "Response is null from PetList API");
            Assert.IsNotNull(petListResponse.Result, "Result is null from PetList API");

            Assert.IsTrue(petListResponse.Result.Count() > 0, "Result has no registered pets");
            var firstPetOwner = petListResponse.Result.FirstOrDefault();

            Assert.IsNotNull(firstPetOwner, "PetOwner object is null in the result");
            Assert.IsFalse(firstPetOwner.Name.IsNullOrEmpty(), "PetOwner Name is empty");
            Assert.IsFalse(firstPetOwner.Gender.IsNullOrEmpty(), "PetOwner gender is empty");
            Assert.IsTrue(firstPetOwner.Age > 0, "PetOwner age is not provided");
            Assert.IsTrue(firstPetOwner.Age > 0, "PetOwner age is not provided");
            Assert.IsTrue(petListResponse.Result.Select(x => x.Pets.Any()).Any(), "There are no pets in the petowners list");

            //TODO: Loop through all objects to do thorough check
        }


        #region Private Methods
        private static PetService CreatePetService()
        {
            //IOptions setup
            var _repoSettings = new Mock<IOptions<PetRepositorySetting>>();
            _repoSettings.Setup(x => x.Value).Returns(new PetRepositorySetting() { Url = "http://agl-developer-test.azurewebsites.net/people.json" });

            //HttpClient setup
            //var mockMessageHandler = new Mock<HttpMessageHandler>();
            //mockMessageHandler.Protected()
            //    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            //    .ReturnsAsync(new HttpResponseMessage
            //    {
            //        StatusCode = HttpStatusCode.OK,
            //        Content = new StringContent(TestHelper.GetPetApiSnapshot())
            //    });

            var mockHttpClient = new HttpClient();
            var typedClient = new Mock<TypedHttpClient>(MockBehavior.Strict, new[] { mockHttpClient });
            typedClient.SetupGet(t => t.Client).Returns(mockHttpClient);

            //Logger setup
            var mockLogger = new Mock<ILogger<PetService>>();
            ILogger<PetService> logger = mockLogger.Object;

            return new PetService(_repoSettings.Object, typedClient.Object, mockLogger.Object);
        }
        #endregion
    }
}

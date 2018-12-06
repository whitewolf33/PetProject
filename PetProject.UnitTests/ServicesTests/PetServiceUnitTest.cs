using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using PetProject.BusinessLayer.Configurations;
using PetProject.BusinessLayer.Helpers;
using PetProject.BusinessLayer.Models;
using PetProject.BusinessLayer.Services;
using PetProject.UnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PetProject.UnitTests
{
    [TestClass]
    public class PetServiceUnitTest
    {
        private static PetService _petService;
        private static List<PetOwner> _mockPetOwnersList = new List<PetOwner>();

        #region Test Setup
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _petService = CreateMockPetService();
            SetupMockData();
        }

        [ClassCleanup]
        public static void TearDown()
        {
            _petService = null;
            _mockPetOwnersList = null;
        } 
        #endregion

        #region General Tests
        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(_petService, "PetService is null");
        } 
        #endregion

        #region GetPetsList Tests
        [TestMethod]
        public async Task TestMethod_GetPetsList()
        {            
            var expected = 6;
            var actual = await _petService.GetPetOwnersAsync();

            Assert.IsNotNull(actual, "GetPetListTest Snapshot mocktest returned null");
            Assert.IsTrue(actual.Success, "GetPetListTest Response did not return Success");
            Assert.IsTrue(actual.ErrorMessage.IsNullOrEmpty(), "GetPetListTest Response Error Message is not empty");
            Assert.IsNotNull(actual.Result, "GetPetListTest Snapshot mocktest returned null results");
            Assert.AreEqual(expected, actual.Result.Count, "GetPetListTest SnapShot Mockdata returning unexpected results");
        } 
        #endregion

        #region GroupPetsByOwnersGender Tests
        [TestMethod]
        public async Task TestGetPetsByOwnerGender_NullList()
        {
            var actual = await _petService.GetPetsByOwnerGender(null, PetType.Dog);
            Assert.IsNull(actual, "Unexpected results for null input");
        }

        [TestMethod]
        public async Task TestGetPetsByOwnerGender_EmptyList()
        {
            var resultList = await _petService.GetPetsByOwnerGender(new List<PetOwner>(), PetType.Dog);
            Assert.IsNotNull(resultList, "Unexpected results");
            Assert.IsTrue(resultList.Count == 0, "Returned Pet list is not empty");
        }

        [TestMethod]
        public async Task TestGetPetsByOwnerGender_NoCats()
        {
            var noCatsList = new List<PetOwner>() {new PetOwner
            {
                Gender = "Female",
                Pets = new List<Pet>() { new Pet { Name = "Fish1", Type = PetType.Fish }, new Pet { Name = "Dog1", Type = PetType.Dog } }
            } };
            var expected = 0;
            var actual = await _petService.GetPetsByOwnerGender(noCatsList, PetType.Cat);
            Assert.AreEqual(expected, actual.Count, "Unexpected result returned for NoCats condition");
        }

        [TestMethod]
        public async Task TestGetPetsByOwnerGender_CatsAndMaleOwnersOnly()
        {
            var _maleOwnersOnlyList = _mockPetOwnersList
                                           .Where(x => x.Gender.Trim().Equals("male", StringComparison.CurrentCultureIgnoreCase));
            var expected = 3;
            var actual = await _petService.GetPetsByOwnerGender(_maleOwnersOnlyList.ToList(), PetType.Cat);
            Assert.AreEqual(expected, actual.Count, "Unexpected Cats count for male owners returned");
        }

        [TestMethod]
        public async Task TestGetPetsByOwnerGender_ForCatsAndFemaleOwnersOnly()
        {
            var _femaleOwnersOnlyList = _mockPetOwnersList
                                            .Where(x => x.Gender.Trim().Equals("female", StringComparison.CurrentCultureIgnoreCase));
            var expected = 2; //based on mockdata
            var actual = await _petService.GetPetsByOwnerGender(_femaleOwnersOnlyList.ToList(), PetType.Cat);
            Assert.AreEqual(expected, actual.Count, "Unexpected Cats count for female owners returned");
        }


        [TestMethod]
        public async Task TestGetPetsByOwnerGender_NoDogs()
        {
            var list = new List<PetOwner>() {new PetOwner
            {
                Gender = "Female",
                Pets = new List<Pet>() { new Pet { Name = "Fish1", Type = PetType.Fish } }
            } };

            var expected = 0;
            var actual = await _petService.GetPetsByOwnerGender(list, PetType.Dog);
            Assert.AreEqual(expected, actual.Count, "Unexpected results returned for No dogs condition");
        }

        [TestMethod]
        public async Task TestGetPetsByOwnerGender_DogsAndMaleOwnersOnly()
        {
            //TODO:
        }

        [TestMethod]
        public async Task TestGetPetsByOwnerGender_DogsAndFemaleOwnersOnly()
        {
            //TODO:
        }

#endregion
       
        #region Private Methods
        private static PetService CreateMockPetService()
        {
            //IOptions setup
            var _repoSettings = new Mock<IOptions<PetRepositorySetting>>();
            _repoSettings.Setup(x => x.Value).Returns(new PetRepositorySetting() { Url = "https://foo.com/api/peep.json" });

            //HttpClient setup
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            mockMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(TestHelper.GetPetApiSnapshot())
                });

            var mockHttpClient = new HttpClient(mockMessageHandler.Object);
            var typedClient = new Mock<TypedHttpClient>(MockBehavior.Strict, new[] { mockHttpClient });
            typedClient.SetupGet(t => t.Client).Returns(mockHttpClient);

            //Logger setup
            var mockLogger = new Mock<ILogger<PetService>>();
            ILogger<PetService> logger = mockLogger.Object;

            return new PetService(_repoSettings.Object, typedClient.Object, mockLogger.Object);
        }

        private static void SetupMockData()
        {
            _mockPetOwnersList.Add(new PetOwner { Gender = "Male", Pets = new List<Pet>() { new Pet { Name = "Cat1", Type = PetType.Cat } } });
            _mockPetOwnersList.Add(new PetOwner
            {
                Gender = "Female",
                Pets = new List<Pet>()
                                                            {   new Pet { Name = "Cat2", Type = PetType.Cat },
                                                                new Pet { Name = "Cat3", Type = PetType.Cat },
                                                                new Pet { Name = "Dog1", Type = PetType.Dog }
                                                            }
            });
            _mockPetOwnersList.Add(new PetOwner { Gender = "Male", Pets = null });
            _mockPetOwnersList.Add(new PetOwner
            {
                Gender = "Male",
                Pets = new List<Pet>()
                                                            {   new Pet { Name = "Cat4", Type = PetType.Cat },
                                                                new Pet { Name = "Cat5", Type = PetType.Cat },
                                                                new Pet { Name = "Dog2", Type = PetType.Dog }
                                                            }
            });
            _mockPetOwnersList.Add(new PetOwner { Gender = "Female", Pets = new List<Pet>() { new Pet { Name = "Fish1", Type = PetType.Fish } } });
        } 
        #endregion
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PetProject.BusinessLayer.Configurations;
using PetProject.BusinessLayer.Helpers;
using PetProject.BusinessLayer.Interfaces;
using PetProject.BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PetProject.BusinessLayer.Services
{
    public class PetService : BaseService, IPetService
    {
        private PetRepositorySetting _repoConfig;
        private HttpClient _httpClient;
        public PetService(IOptions<PetRepositorySetting> petRepoConfig, TypedHttpClient typedClient, ILogger logger) 
                            : base(logger)
        {
            _repoConfig = petRepoConfig.Value;
            _logger = logger;
            _httpClient = typedClient.Client;
        }
        /// <summary>
        /// Async Method to consume the PetAPI 
        /// returns a list of PetOwners with their registered pets
        /// </summary>
        /// <returns>List<PetOwners></PetOwners></returns>
        public async Task<StandardResponse<List<PetOwner>>> GetPetOwnersAsync()
        {
            var stdResponse = new StandardResponse<List<PetOwner>>();
            try
            {
                var baseUrl = _repoConfig.Url;
                _logger.LogDebug($"Path for the WebAPI is {baseUrl}");
                //Set the baseUrl here to allow for changes in the middle of execution
                _httpClient.BaseAddress = new Uri(baseUrl);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await _httpClient.GetAsync(baseUrl);

                _logger.LogDebug($"Http Response Code is {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (!result.IsNullOrEmpty())
                    {
                        //NOTE: Talk to business about considering caching of the results using Polly.Cache
                        var petOwnersList = JsonConvert.DeserializeObject<List<PetOwner>>(result);
                        _logger.LogDebug($"Http Request resulted with {petOwnersList.Count} owners");

                        stdResponse.Success = true;
                        stdResponse.Result = petOwnersList;
                    }
                }
                else
                {
                    throw new Exception($"Error occured with response code - {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                stdResponse = StandardResponse<List<PetOwner>>.GenericError(ex.Message);
            }
            return stdResponse;
        }
        /// <summary>
        /// Grouping of the pets by the gender of their owner        
        /// </summary>
        /// <param name="petOwnersList">list of petowners with their pets</param>
        /// <param name="petType"> Ability to filter by a chosen PetType</param>
        /// <returns>Returns an IGroupable to print</returns>
        public async Task<List<IGroupable>> GetPetsByOwnerGender(List<PetOwner> petOwnersList, PetType petType)
        {
            if (petOwnersList == null)
                return null;

            try
            {
                var petNamesByOwnerGendersList = new List<IGroupable>();

                //Iterate through PetOwnersList and get Pets of desiredType 
                // and store in an anonymous class of type (Gender, PetName)
                foreach (var petOwner in petOwnersList)
                {
                    if (petOwner != null && petOwner.Pets != null)
                    {
                        foreach (var pet in petOwner.Pets)
                        {
                            if (pet != null)
                            {
                                if (pet.Type == petType)
                                {
                                    petNamesByOwnerGendersList.Add(new GroupedPetNames(petOwner.Gender, pet.Name));
                                }
                            }
                        }
                    }
                }

                return await Task.FromResult(petNamesByOwnerGendersList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }
    }
}


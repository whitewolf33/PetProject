using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace PetProject.BusinessLayer.Services
{
    public abstract class BaseRestService : BaseService
    {
        protected readonly HttpClient _httpClient;
        public BaseRestService(HttpClient httpClient, ILogger logger) : base(logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
    }
}

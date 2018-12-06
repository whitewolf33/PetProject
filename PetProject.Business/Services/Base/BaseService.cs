using Microsoft.Extensions.Logging;

namespace PetProject.BusinessLayer.Services
{
    public abstract class BaseService
    {
        protected ILogger _logger;
        public BaseService(ILogger logger)
        {
            _logger = logger;
        }
    }
}

using System.Net.Http;

namespace PetProject.BusinessLayer.Helpers
{
    /// <summary>
    /// Class used for DI HttpClient to use in the services required
    /// </summary>
    public class TypedHttpClient
    {        
        public virtual HttpClient Client { get; private set; }       

        public TypedHttpClient(HttpClient client)
        {
            Client = client;            
        }        
    }
}


using Newtonsoft.Json;

namespace PetProject.BusinessLayer.Models
{
    public abstract class Base
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
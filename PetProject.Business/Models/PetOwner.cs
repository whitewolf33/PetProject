using Newtonsoft.Json;
using System.Collections.Generic;

namespace PetProject.BusinessLayer.Models
{
    public class PetOwner : Base
    {
        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("age")]
        public long Age { get; set; }

        [JsonProperty("pets")]
        public List<Pet> Pets { get; set; }
    }
}

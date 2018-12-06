using PetProject.BusinessLayer.Interfaces;

namespace PetProject.BusinessLayer.Models
{
    public class GroupedPetNames : IGroupable
    {
        public GroupedPetNames(string key, string name)
        {
            Key = key;
            Name = name;
        }

        public string Key { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
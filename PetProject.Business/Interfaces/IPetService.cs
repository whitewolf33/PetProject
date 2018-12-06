using PetProject.BusinessLayer.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PetProject.BusinessLayer.Interfaces
{
    public interface IPetService
    {
        Task<StandardResponse<List<PetOwner>>> GetPetOwnersAsync();

        Task<List<IGroupable>> GetPetsByOwnerGender(List<PetOwner> petOwnersList, PetType petType);
    }
}
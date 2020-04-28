using System.Threading.Tasks;
using AdvertApi.Models;

namespace AdvertApi.Services
{
    public class DynamoDbAdvertStorageService : IAdvertStorageService
    {
        public Task<string> Add(AdvertModel model)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Confirm(ConfirmAdvertModel model)
        {
            throw new System.NotImplementedException();
        }
    }
}

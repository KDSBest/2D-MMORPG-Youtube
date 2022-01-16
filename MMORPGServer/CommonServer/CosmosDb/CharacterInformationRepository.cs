using Common.Protocol.Character;
using Microsoft.Azure.Cosmos;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb
{
	public class CharacterInformationRepository : CosmosDbRepository<CharacterInformation>
    {
        public CharacterInformationRepository() : base(CosmosClientSinglton.Instance.CharacterContainer.Value)
        {

        }

        public async Task<bool> HasPlayerACharacterAsync(string owner)
        {
            return await GetCharacterByOwnerAsync(owner) != null;
        }

        public Task<CharacterInformation> GetCharacterByOwnerAsync(string owner)
        {
            return Task.Run(() =>
            {
                try
                {
                    CharacterInformation result = Container.GetItemLinqQueryable<CharacterInformation>(true).Where(x => x.Owner == owner).AsEnumerable().FirstOrDefault();
                    return result;
                }
                catch (CosmosException de)
                {
                    if (de.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                }

                return null;
            });
        }
    }
}

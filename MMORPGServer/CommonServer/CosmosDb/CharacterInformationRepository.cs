using Common.Protocol.Character;
using Microsoft.Azure.Cosmos;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CommonServer.CosmosDb
{
	public class CharacterInformationRepository : CosmosDbRepository<CharacterInformation>
    {
        public CharacterInformationRepository() : base(CosmosClientSinglton.Instance.CharacterContainer)
        {

        }

		public Task<CharacterInformation> GetByNameAsync(string charName)
		{
            return Task.Run(() =>
            {
                try
                {
                    CharacterInformation result = Container.GetItemLinqQueryable<CharacterInformation>(true).Where(x => x.Name == charName).AsEnumerable().FirstOrDefault();
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

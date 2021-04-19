using CommonServer.CosmosDb.Model;

namespace CommonServer.CosmosDb
{
    public class UserInformationRepository : CosmosDbRepository<UserInformation>
    {
        public UserInformationRepository() : base(CosmosClientSinglton.Instance.LoginContainer)
        {

        }
    }
}

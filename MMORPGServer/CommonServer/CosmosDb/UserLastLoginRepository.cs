using CommonServer.CosmosDb.Model;

namespace CommonServer.CosmosDb
{
	public class UserLastLoginRepository : CosmosDbRepository<UserLastLogin>
    {
        public UserLastLoginRepository() : base(CosmosClientSinglton.Instance.UserLastLoginContainer.Value)
        {

        }
    }
}

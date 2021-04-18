using Common.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class CryptoProviderTest
	{
		[TestMethod]
		public void CryptoProviderFullIntegrationTest()
		{
			string testData = "Hello World!";
			CryptoProvider server = new CryptoProvider();
			string pubKey = server.GetPublicCspBlob();

			CryptoProvider client = new CryptoProvider(pubKey);
			string aesParameter = client.EncryptAesParameter();

			server.DecryptAesParameter(aesParameter);

			string encryptedData = client.Encrypt(testData);

			Assert.AreEqual(testData, server.Decrypt(encryptedData));
			Assert.AreEqual(encryptedData, server.Encrypt(testData));
			Assert.AreEqual(testData, client.Decrypt(encryptedData));
		}
	}
}

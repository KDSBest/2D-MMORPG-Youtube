using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Crypto
{
	public class CryptoProvider
	{
		private CspParameters cspp = new CspParameters();
		private RSACryptoServiceProvider rsa;
		private Aes aes;
		private static readonly char splitCharacter = ':';

		public CryptoProvider() : this(string.Empty)
		{

		}

		public CryptoProvider(string cspBlobBase64)
		{
			rsa = new RSACryptoServiceProvider(cspp);
			aes = Aes.Create();

			if (!string.IsNullOrEmpty(cspBlobBase64))
			{
				byte[] cspBlob = Convert.FromBase64String(cspBlobBase64);
				rsa.ImportCspBlob(cspBlob);
			}
		}

		public string GetPublicCspBlob()
		{
			byte[] cspBlob = rsa.ExportCspBlob(false);
			return Convert.ToBase64String(cspBlob);
		}

		public string GetPrivateCspBlob()
		{
			byte[] cspBlob = rsa.ExportCspBlob(true);
			return Convert.ToBase64String(cspBlob);
		}

		public string EncryptAesParameter()
		{
			string ivEncrypted = Convert.ToBase64String(rsa.Encrypt(aes.IV, false));
			string keyEncrypted = Convert.ToBase64String(rsa.Encrypt(aes.Key, false));

			return $"{ivEncrypted}{splitCharacter}{keyEncrypted}";
		}

		public void DecryptAesParameter(string aesParameter)
		{
			var parameter = aesParameter.Split(splitCharacter);
			if(parameter.Length != 2)
			{
				throw new ArgumentException($"Aes Parameter need to split with '{splitCharacter}' into 2 parameter.");
			}

			byte[] ivEncrypted = Convert.FromBase64String(parameter[0]);
			byte[] keyEncrypted = Convert.FromBase64String(parameter[1]);

			aes.IV = rsa.Decrypt(ivEncrypted, false);
			aes.Key = rsa.Decrypt(keyEncrypted, false);
		}

		public string Encrypt(string value)
		{
			byte[] bytes = UTF8Encoding.UTF8.GetBytes(value);
			using (ICryptoTransform transform = aes.CreateEncryptor())
			{
				using(var memStream = new MemoryStream())
				{
					using(var cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write))
					{
						cryptoStream.Write(bytes, 0, bytes.Length);
					}

					return Convert.ToBase64String(memStream.ToArray());
				}
			}
		}

		public string Decrypt(string value)
		{
			byte[] bytes = Convert.FromBase64String(value);
			using (ICryptoTransform transform = aes.CreateDecryptor())
			{
				using (var memStream = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write))
					{
						cryptoStream.Write(bytes, 0, bytes.Length);
					}

					return UTF8Encoding.UTF8.GetString(memStream.ToArray());
				}
			}
		}
	}
}

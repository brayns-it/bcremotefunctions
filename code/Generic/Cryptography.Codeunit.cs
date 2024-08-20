using System.Text;
using Org.BouncyCastle.Cms;

namespace BCRemoteFunctions
{
    public class Cryptography : Codeunit
    {
        [ApiMethod(Method = RequestMethod.Post, Route = "cryptography/GetPkcs7Message")]
        public string GetPkcs7Message(string envelope)
        {
            byte[] binEnvelope = Convert.FromBase64String(envelope);

            // attempt DER
            try
            {
                binEnvelope = Convert.FromBase64String(Encoding.UTF8.GetString(binEnvelope));
            }
            catch
            {
            }

            var cms = new CmsSignedData(binEnvelope);
            CmsProcessableByteArray content = (CmsProcessableByteArray)cms.SignedContent;
            MemoryStream ms = new();
            Stream cs = content.GetInputStream();
            cs.CopyTo(ms);
            byte[] binContent = ms.ToArray();
            ms.Close();
            cs.Close();

            return Convert.ToBase64String(binContent);
        }
    }
}

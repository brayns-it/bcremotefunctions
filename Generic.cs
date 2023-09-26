using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Cms;
using System.Text;

namespace BCRemoteFunctions
{
    public static class Generic
    {
        public static async Task<JObject> Execute(string function, JObject request)
        {
            switch (function)
            {
                case "GetPkcs7Message":
                    return await GetPkcs7Message(request);
                default:
                    throw new Exception("Invalid function " + function);
            }
        }

        public static async Task<JObject> GetPkcs7Message(JObject request)
        {
            string envelope = request["envelope"]!.ToString();
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
            await cs.CopyToAsync(ms);
            byte[] binContent = ms.ToArray();
            ms.Close();
            cs.Close();

            JObject res = new();
            res["content"] = Convert.ToBase64String(binContent);
            return res;
        }
    }
}

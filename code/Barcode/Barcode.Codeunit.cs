using System.Text;
using Org.BouncyCastle.Cms;

namespace BCRemoteFunctions
{
    public class Barcode : Codeunit
    {
        [ApiMethod(Method = RequestMethod.Post, Route = "barcode/GetQrCode")]
        public string GetQrCode(string text, int pixelsPerModule)
        {
            var qrData = QRCoder.QRCodeGenerator.GenerateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.Base64QRCode(qrData);
            return qrCode.GetGraphic(pixelsPerModule);
        }
    }
}

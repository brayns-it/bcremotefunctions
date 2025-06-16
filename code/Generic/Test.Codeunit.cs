using System.Text;
using Org.BouncyCastle.Cms;

namespace BCRemoteFunctions
{
    public class Test : Codeunit
    {
        [ApiMethod(Method = RequestMethod.Post, Route = "test/TestConnection")]
        public bool TestConnection()
        {
            return true;
        }
    }
}

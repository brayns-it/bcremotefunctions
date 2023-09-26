namespace BCRemoteFunctions
{
    public class ErrorResponse
    {
        public class ErrorMessage
        {
            public string message = "";
        }

        public ErrorMessage error = new();

        public ErrorResponse(Exception ex)
        {
            error.message = ex.Message;
        }
    }

    public class Tokens
    {
        public class Token
        {
            public string token = "";
            public bool genericEnabled = false;
        }

        public List<Token> tokens = new();
    }
}

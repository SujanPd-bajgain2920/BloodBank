namespace bloodconnect.Model
{
    public class Response
    {
        public int StatusCode { get; set; }

        public string StatusMessage { get; set; }
        public bool Success { get; internal set; }
        public Profile Data { get; internal set; }

        
    }

    internal class Response<T>
    {
        public Profile Data { get; internal set; }

        public static explicit operator Response<T>(Response v)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Ranger.Services.Subscriptions
{
    public class ChargeBeeException : System.Exception
    {
        public ChargeBeeException() { }
        public ChargeBeeException(string message) : base(message) { }
        public ChargeBeeException(string message, System.Exception inner) : base(message, inner) { }
    }
}
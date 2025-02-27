namespace Helpers
{
    [Serializable]
    internal class InvalidGeolocationException : Exception
    {
        public InvalidGeolocationException()
        {
        }

        public InvalidGeolocationException(string? message) : base(message)
        {
        }

        public InvalidGeolocationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
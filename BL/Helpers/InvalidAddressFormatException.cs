
namespace Helpers
{
    [Serializable]
    internal class InvalidAddressFormatException : Exception
    {
        public InvalidAddressFormatException()
        {
        }

        public InvalidAddressFormatException(string? message) : base(message)
        {
        }

        public InvalidAddressFormatException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
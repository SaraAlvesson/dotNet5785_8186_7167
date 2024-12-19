
namespace BO;

internal class Exceptions
{
    [Serializable] // Indicates that this exception can be serialized, useful for remoting or saving state.
    public class BlNullPropertyException : Exception
    {
        public BlNullPropertyException(string? message) : base(message) { }
    }

    public class BlDoesNotExistException : Exception
    {
        public BlDoesNotExistException(string? message) : base(message) { }
        public BlDoesNotExistException(string message, Exception innerException): base(message, innerException) { }
    }

     /// <summary>
    /// Exception thrown when an attempt is made to add an entity that already exists in the DAL.
    /// </summary>
    public class BLAlreadyExistException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the DalAlreadyExistException class with a specific error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public BLAlreadyExistException(string? message) : base(message) { } // Passes the message to the base Exception class.
        public BLAlreadyExistException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when an entity cannot be deleted from the DAL due to constraints or other reasons.
    /// </summary>
    public class BLDeletionImpossible : Exception
    {
        /// <summary>
        /// Initializes a new instance of the DalDeletionImpossible class with a specific error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public BLDeletionImpossible(string? message) : base(message) { } // Passes the message to the base Exception class.
        public BLDeletionImpossible(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BLXMLFileLoadCreateException : Exception
    {

        public BLXMLFileLoadCreateException(string? message) : base(message) { }
        public BLXMLFileLoadCreateException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class AdressDoesNotExistException : Exception
    {

        public AdressDoesNotExistException(string? message) : base(message) { }
        public AdressDoesNotExistException(string message, Exception innerException) : base(message, innerException) { }
    }

  

}

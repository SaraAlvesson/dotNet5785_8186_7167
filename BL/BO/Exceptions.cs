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
        public BlDoesNotExistException(string message, Exception innerException) : base(message, innerException) { }
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

    public class BlEmailNotCorrect : Exception
    {

        public BlEmailNotCorrect(string? message) : base(message) { }
        public BlEmailNotCorrect(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BlPhoneNumberNotCorrect : Exception
    {

        public BlPhoneNumberNotCorrect(string? message) : base(message) { }
        public BlPhoneNumberNotCorrect(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BlMaxDistanceNotCorrect : Exception
    {

        public BlMaxDistanceNotCorrect(string? message) : base(message) { }
        public BlMaxDistanceNotCorrect(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BlIdNotValid : Exception
    {

        public BlIdNotValid(string? message) : base(message) { }
        public BlIdNotValid(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BlPasswordNotValid : Exception
    {

        public BlPasswordNotValid(string? message) : base(message) { }
        public BlPasswordNotValid(string message, Exception innerException) : base(message, innerException) { }
    }

    public class BlUnauthorizedAccessException : Exception
    {

        public BlUnauthorizedAccessException(string? message) : base(message) { }
        public BlUnauthorizedAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BlCantBeErased : Exception
    {

        public BlCantBeErased(string? message) : base(message) { }
        public BlCantBeErased(string message, Exception innerException) : base(message, innerException) { }
    }
    public class AddressDoesNotExistException : Exception
    {

        public AddressDoesNotExistException(string? message) : base(message) { }
        public AddressDoesNotExistException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BlInvalidOperationException : Exception
    {

        public BlInvalidOperationException(string? message) : base(message) { }
        public BlInvalidOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class CannotUpdateCallException : Exception
    {

        public CannotUpdateCallException(string? message) : base(message) { }
        public CannotUpdateCallException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class CannotUpdateVolunteerException : Exception
    {

        public CannotUpdateVolunteerException(string? message) : base(message) { }
        public CannotUpdateVolunteerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class BlInvalidLocationException : Exception
    {

        public BlInvalidLocationException(string? message) : base(message) { }
        public BlInvalidLocationException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BlBadVolunteerLogicException : Exception
    {

        public BlBadVolunteerLogicException(string? message) : base(message) { }
        public BlBadVolunteerLogicException(string message, Exception innerException) : base(message, innerException) { }
    }
    

    [Serializable]
    public class BLTemporaryNotAvailableException : Exception
    {
        public BLTemporaryNotAvailableException()
        {
        }

        public BLTemporaryNotAvailableException(string? message) : base(message)
        {
        }

        public BLTemporaryNotAvailableException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
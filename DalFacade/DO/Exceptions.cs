
namespace DO;

/// <summary>
/// Exception thrown when a requested entity does not exist in the DAL (Data Access Layer).
/// </summary>
[Serializable] // Indicates that this exception can be serialized, useful for remoting or saving state.
public class DalDoesNotExistException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DalDoesNotExistException class with a specific error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DalDoesNotExistException(string? message) : base(message) { } // Passes the message to the base Exception class.
}

/// <summary>
/// Exception thrown when an attempt is made to add an entity that already exists in the DAL.
/// </summary>
public class DalAlreadyExistException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DalAlreadyExistException class with a specific error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DalAlreadyExistException(string? message) : base(message) { } // Passes the message to the base Exception class.
}

/// <summary>
/// Exception thrown when an entity cannot be deleted from the DAL due to constraints or other reasons.
/// </summary>
public class DalDeletionImpossible : Exception
{
    /// <summary>
    /// Initializes a new instance of the DalDeletionImpossible class with a specific error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DalDeletionImpossible(string? message) : base(message) { } // Passes the message to the base Exception class.
}
public class DalXMLFileLoadCreateException:Exception
{

    public DalXMLFileLoadCreateException(string? message) : base(message) { } 
}
public class InvalidCallFormatException : Exception
{
    public InvalidCallFormatException(string message) : base(message) { }
}

public class InvalidCallLogicException : Exception
{
    public InvalidCallLogicException(string message) : base(message) { }
}

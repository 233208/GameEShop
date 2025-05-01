namespace User.Domain.Exceptions.Login;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Incorect password or login") { }
}
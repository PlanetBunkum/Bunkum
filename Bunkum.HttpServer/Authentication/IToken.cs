namespace Bunkum.HttpServer.Authentication;

#nullable disable

public interface IToken<out TUser> where TUser : IUser
{
    public TUser User { get; }
}
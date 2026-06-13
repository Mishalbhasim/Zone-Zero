using System;

public interface IAuthService
{
    bool IsLoggedIn { get; }
    string PlayerId { get; }
    string PlayerName { get; }

    void Login(string email, string password,
                  Action onSuccess, Action<string> onFail);

    void Register(string username, string email, string password,
                  Action onSuccess, Action<string> onFail);

    void Logout();
}
public interface IUserService
{
    Task<bool> RegisterUser(RegisterViewModel model);
    Task<(string Token, List<string> Roles)?> LoginUser(LoginViewModel model);
    Task<UserDto?> GetUser(int userId);
    Task<UserDto?> GetUserByToken(string recommendationToken);
    Task<bool> UpdateUser(int userId, UserDto updatedUser);
}

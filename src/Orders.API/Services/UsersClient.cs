using System.Net;

namespace Orders.API.Services;

public class UsersClient(HttpClient http)
{
    public async Task<bool> ExistsAsync(Guid usuarioId)
    {
        var response = await http.GetAsync($"/api/users/{usuarioId}");

        if (response.StatusCode == HttpStatusCode.NotFound) return false;
        response.EnsureSuccessStatusCode();

        return true;
    }
}

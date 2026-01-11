using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}

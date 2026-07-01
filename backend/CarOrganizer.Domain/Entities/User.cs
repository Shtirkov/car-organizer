using Microsoft.AspNetCore.Identity;

namespace CarOrganizer.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }
}
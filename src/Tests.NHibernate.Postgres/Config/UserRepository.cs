using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public class UserRepository : Repository
{
    public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().GetByIdAsync(id, cancellationToken);
    }
    
    public async Task<User> GetByIdOrDefaultAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().GetByIdOrDefaultAsync(id, cancellationToken);
    }
    
    public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().SingleAsync(new ByUsernameSpecification(username), cancellationToken);
    }
    
    public async Task<User?> GetByEmailAddressOrDefaultAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().SingleOrDefaultAsync(new EqualsFilter("emailAddress", emailAddress), cancellationToken);
    }
    
    public async Task<User> GetFirstActiveAsync(CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().FirstAsync(new EqualsFilter("isActive", true), cancellationToken);
    }
    
    public async Task<User?> GetFirstByNameOrDefaultAsync(string username, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().FirstOrDefaultAsync(new EqualsFilter("username", username), cancellationToken);
    }
    
    public async Task<IReadOnlyCollection<User>> GetMultipleByNameAsync(ICollection<string> usernames, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().ManyAsync(new InFilter("username", usernames.ToArray<object>()), cancellationToken);
    }
    
    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().ManyAsync(cancellationToken: cancellationToken);
    }
    
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var repeatedUsernames = await UnitOfWork.RawProjection<dynamic?>("select name from Users where name = :p0;", new { p0 = user.Username}, cancellationToken);
        if (repeatedUsernames.Any())
        {
            throw new ArgumentException("User already exists.");
        }
        await UnitOfWork.AddAsync(user, cancellationToken);
    }
}

public class ByUsernameSpecification : Specification
{
    public ByUsernameSpecification(string username)
    {
        this.AddFilter(new EqualsFilter("username", username));
        this.AddFilter(new EqualsFilter("isActive", true));
    }
}
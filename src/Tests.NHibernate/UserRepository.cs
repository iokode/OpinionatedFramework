using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.Tests.NHibernate;

public class UserRepository : Repository
{
    public async Task<User> GetByUsername(string username, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().SingleAsync(new ByUsernameSpec(username).ToFilter(), cancellationToken);
    }
    
    public async Task<User?> GetByEmailAddress(string emailAddress, CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().SingleOrDefaultAsync(new EqualsFilter("emailAddress", emailAddress), cancellationToken);
    }
    
    public async Task<IEnumerable<User>> GetMultipleByIsActive(CancellationToken cancellationToken = default)
    {
        return await GetEntitySet<User>().ManyAsync(new EqualsFilter("isActive", true), cancellationToken);
    }
    
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await UnitOfWork.AddAsync(user, cancellationToken);
    }
}

public class ByUsernameSpec : Spec
{
    public ByUsernameSpec(string username)
    {
        this.AddFilter(new EqualsFilter("username", username));
        this.AddFilter(new EqualsFilter("isActive", true));
    }
}
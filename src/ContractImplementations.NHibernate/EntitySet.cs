using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder.Exceptions;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using NHibernate;
using NHibernate.Criterion;
using NHNonUniqueResultException = NHibernate.NonUniqueResultException;
using NonUniqueResultException = IOKode.OpinionatedFramework.Persistence.QueryBuilder.Exceptions.NonUniqueResultException;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public class EntitySet<T> : IEntitySet<T> where T : Entity
{
    private readonly ISession _session;

    public EntitySet(ISession session)
    {
        _session = session;
    }

    public async Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _session.LoadAsync<T>(id, cancellationToken);
        }
        catch (ObjectNotFoundException ex)
        {
            throw new EntityNotFoundException(id, ex);
        }
    }
    
    public async Task<T> GetByIdOrDefaultAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _session.GetAsync<T>(id, cancellationToken);
    }

    public async Task<T> SingleAsync(Filter? filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);

        try
        {
            var result = await criteria.UniqueResultAsync<T>(cancellationToken);
            if (result == null)
            {
                throw new EmptyResultException();
            }

            return result;
        }
        catch(NHNonUniqueResultException ex)
        {
            throw new NonUniqueResultException(ex);
        }
    }

    public async Task<T?> SingleOrDefaultAsync(Filter? filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);

        try
        {
            return await criteria.UniqueResultAsync<T>(cancellationToken);
        }
        catch(NHNonUniqueResultException ex)
        {
            throw new NonUniqueResultException(ex);
        }
    }

    public async Task<T> FirstAsync(Filter? filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);
        criteria.SetMaxResults(1);

        var list = await criteria.ListAsync<T>(cancellationToken);
        if (list.Count == 0)
        {
            throw new EmptyResultException();
        }

        return list[0];
    }

    public async Task<T?> FirstOrDefaultAsync(Filter? filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);
        criteria.SetMaxResults(1);

        var list = await criteria.ListAsync<T>(cancellationToken);
        return list.Count == 0 ? null : list[0];
    }

    public async Task<IReadOnlyCollection<T>> ManyAsync(Filter? filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);

        var list = await criteria.ListAsync<T>(cancellationToken);
        return (IReadOnlyCollection<T>)list;
    }

    private void ApplyFilter(ICriteria criteria, Filter? filter)
    {
        if (filter == null)
        {
            return;
        }

        var criterion = BuildCriterion(filter);
        criteria.Add(criterion);
    }

    private ICriterion BuildCriterion(Filter filter)
    {
        return filter switch
        {
            EqualsFilter eq => Restrictions.Eq(eq.FieldName, eq.Value),
            LikeFilter like => Restrictions.Like(like.FieldName, like.Pattern, MatchMode.Anywhere),
            InFilter inFilter => Restrictions.In(inFilter.FieldName, inFilter.Values),
            BetweenFilter betweenFilter => Restrictions.Between(betweenFilter.FieldName, betweenFilter.Low, betweenFilter.High),
            GreaterThanFilter greaterThanFilter => Restrictions.Gt(greaterThanFilter.FieldName, greaterThanFilter.Value),
            LessThanFilter lessThanFilter => Restrictions.Lt(lessThanFilter.FieldName, lessThanFilter.Value),
            AndFilter andFilter => BuildJunction(andFilter.Filters, isAnd: true),
            OrFilter orFilter => BuildJunction(orFilter.Filters, isAnd: false),
            NotEqualsFilter notEqualsFilter => Restrictions.Not(Restrictions.Eq(notEqualsFilter.FieldName, notEqualsFilter.Value)),
            _ => throw new NotSupportedException($"Filter type '{filter.GetType().Name}' is not supported.")
        };
    }

    private Junction BuildJunction(Filter[] conditions, bool isAnd)
    {
        Junction junction = isAnd ? Restrictions.Conjunction() : Restrictions.Disjunction();
        foreach (var cond in conditions)
        {
            junction.Add(BuildCriterion(cond));
        }

        return junction;
    }
}
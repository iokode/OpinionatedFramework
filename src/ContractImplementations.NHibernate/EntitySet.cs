using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using NHibernate;
using NHibernate.Criterion;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public class EntitySet<T> : IEntitySet<T> where T : Entity
{
    private readonly ISession _session;

    public EntitySet(ISession session)
    {
        _session = session;
    }

    public async Task<T> SingleAsync(Filter filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);

        var result = await criteria.UniqueResultAsync<T>(cancellationToken);
        if (result == null)
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        // If multiple results were present, NHibernate would throw NonUniqueResultException here.
        return result;
    }

    public async Task<T?> SingleOrDefaultAsync(Filter filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);

        // Returns null if no results, throws if multiple results are found
        return await criteria.UniqueResultAsync<T>(cancellationToken);
    }

    public async Task<T> FirstAsync(Filter filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);
        criteria.SetMaxResults(1);

        var list = await criteria.ListAsync<T>(cancellationToken);
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        return list[0];
    }

    public async Task<T?> FirstOrDefaultAsync(Filter filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);
        criteria.SetMaxResults(1);

        var list = await criteria.ListAsync<T>(cancellationToken);
        return list.Count == 0 ? null : list[0];
    }

    public async Task<IReadOnlyCollection<T>> ManyAsync(Filter filter, CancellationToken cancellationToken = default)
    {
        var criteria = _session.CreateCriteria<T>();
        ApplyFilter(criteria, filter);

        var list = await criteria.ListAsync<T>(cancellationToken);
        return (IReadOnlyCollection<T>)list;
    }

    private void ApplyFilter(ICriteria criteria, Filter filter)
    {
        var criterion = BuildCriterion(filter);
        criteria.Add(criterion);
    }

    private ICriterion BuildCriterion(Filter filter)
    {
        return filter switch
        {
            EqualsFilter eq => Restrictions.Eq(eq.FieldName, eq.Value),
            LikeFilter like => Restrictions.Like(like.FieldName, like.Pattern, MatchMode.Anywhere),
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
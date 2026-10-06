using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;
using NHibernate.Criterion;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.UnitOfWork;

internal sealed class FilterHqlBuilder
{
    private readonly List<(string Name, object Value)> parameters = new();

    public string? Alias
    {
        get;
        init
        {
            if (value is not null && !IsValidIdentifier(value))
            {
                throw new ArgumentException($"'{value}' is not a valid HQL alias.", nameof(value));
            }
            field = value;
        }
    }

    public IReadOnlyCollection<(string Name, object Value)> Parameters => this.parameters;

    public string Build(Filter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return BuildPredicate(filter);
    }

    private string BuildPredicate(Filter filter)
    {
        return filter switch
        {
            EqualsFilter equals => BuildEquality(equals.FieldName, equals.Value, isEqual: true),
            NotEqualsFilter notEquals => BuildEquality(notEquals.FieldName, notEquals.Value, isEqual: false),
            LikeFilter like => $"{BuildFieldReference(like.FieldName)} like {AddParameter(MatchMode.Anywhere.ToMatchString(like.Pattern))}",
            InFilter inFilter => BuildIn(inFilter),
            BetweenFilter between => $"{BuildFieldReference(between.FieldName)} between {AddParameter(between.Low)} and {AddParameter(between.High)}",
            GreaterThanFilter greaterThan => $"{BuildFieldReference(greaterThan.FieldName)} > {AddParameter(greaterThan.Value)}",
            LessThanFilter lessThan => $"{BuildFieldReference(lessThan.FieldName)} < {AddParameter(lessThan.Value)}",
            AndFilter andFilter => BuildJunction(andFilter.Filters, "and", "1 = 1"),
            OrFilter orFilter => BuildJunction(orFilter.Filters, "or", "1 = 0"),
            NotFilter notFilter => $"not ({BuildPredicate(notFilter.Filter)})",
            _ => throw new NotSupportedException($"Filter type '{filter.GetType().Name}' is not supported.")
        };
    }

    private string BuildEquality(string fieldName, object? value, bool isEqual)
    {
        string fieldReference = BuildFieldReference(fieldName);
        if (value is null)
        {
            return $"{fieldReference} is {(isEqual ? string.Empty : "not ")}null";
        }

        return $"{fieldReference} {(isEqual ? "=" : "<>")} {AddParameter(value)}";
    }

    private string BuildIn(InFilter filter)
    {
        if (filter.Values.Length == 0)
        {
            return "1 = 0";
        }

        var parameterNames = new string[filter.Values.Length];
        for (int i = 0; i < filter.Values.Length; i++)
        {
            parameterNames[i] = AddParameter(filter.Values[i]);
        }

        return $"{BuildFieldReference(filter.FieldName)} in ({string.Join(", ", parameterNames)})";
    }

    private string BuildJunction(Filter[] filters, string operation, string emptyPredicate)
    {
        if (filters.Length == 0)
        {
            return emptyPredicate;
        }

        var predicates = new string[filters.Length];
        for (int i = 0; i < filters.Length; i++)
        {
            predicates[i] = BuildPredicate(filters[i]);
        }

        return $"({string.Join($" {operation} ", predicates)})";
    }

    private string AddParameter(object value)
    {
        string name = $"filter{this.parameters.Count}";
        this.parameters.Add((name, value));
        return $":{name}";
    }

    private string BuildFieldReference(string fieldName)
    {
        if (!IsValidFieldPath(fieldName))
        {
            throw new ArgumentException($"'{fieldName}' is not a valid entity field path.", nameof(fieldName));
        }

        return this.Alias is null ? fieldName : $"{this.Alias}.{fieldName}";
    }

    private static bool IsValidFieldPath(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        bool isSegmentStart = true;
        foreach (char character in fieldName)
        {
            if (character == '.')
            {
                if (isSegmentStart)
                {
                    return false;
                }

                isSegmentStart = true;
                continue;
            }

            if (isSegmentStart)
            {
                if (character != '_' && !char.IsLetter(character))
                {
                    return false;
                }

                isSegmentStart = false;
                continue;
            }

            if (character != '_' && !char.IsLetterOrDigit(character))
            {
                return false;
            }
        }

        return !isSegmentStart;
    }

    private static bool IsValidIdentifier(string value)
    {
        if (value.Length == 0 || value[0] != '_' && !char.IsLetter(value[0]))
        {
            return false;
        }

        for (int i = 1; i < value.Length; i++)
        {
            if (value[i] != '_' && !char.IsLetterOrDigit(value[i]))
            {
                return false;
            }
        }

        return true;
    }
}
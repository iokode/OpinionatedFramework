using System;
using System.Collections;
using System.Reflection;
using Humanizer;
using NHibernate.Transform;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

public class SnakeCaseAliasToBeanResultTransformer<T> : IResultTransformer
{
    public object TransformTuple(object[] tuple, string[] aliases)
    {
        var result = Activator.CreateInstance(typeof(T));
        if (result == null)
        {
            throw new InvalidOperationException($"Could not create an instance of {typeof(T).FullName}");
        }

        for (int i = 0; i < aliases.Length; i++)
        {
            string alias = aliases[i];
            string propertyName = alias.Pascalize();

            var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property != null && property.CanWrite)
            {
                property.SetValue(result, tuple[i]);
            }
        }

        return result;
    }

    public IList TransformList(IList collection)
    {
        return collection;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;

internal static class TypeNameHelper
{
    private const char DefaultNestedTypeDelimiter = '+';
#nullable disable
    private static readonly Dictionary<Type, string> _builtInTypeNames = new Dictionary<Type, string>()
    {
        {
            typeof(void),
            "void"
        },
        {
            typeof(bool),
            "bool"
        },
        {
            typeof(byte),
            "byte"
        },
        {
            typeof(char),
            "char"
        },
        {
            typeof(Decimal),
            "decimal"
        },
        {
            typeof(double),
            "double"
        },
        {
            typeof(float),
            "float"
        },
        {
            typeof(int),
            "int"
        },
        {
            typeof(long),
            "long"
        },
        {
            typeof(object),
            "object"
        },
        {
            typeof(sbyte),
            "sbyte"
        },
        {
            typeof(short),
            "short"
        },
        {
            typeof(string),
            "string"
        },
        {
            typeof(uint),
            "uint"
        },
        {
            typeof(ulong),
            "ulong"
        },
        {
            typeof(ushort),
            "ushort"
        }
    };

#nullable enable
    [return: NotNullIfNotNull("item")]
    public static string? GetTypeDisplayName(object? item, bool fullName = true)
    {
        return item != null
            ? TypeNameHelper.GetTypeDisplayName(item.GetType(), fullName, false, true, '+')
            : null;
    }

    /// <summary>Pretty print a type name.</summary>
    /// <param name="type">The <see cref="T:System.Type" />.</param>
    /// <param name="fullName"><c>true</c> to print a fully qualified name.</param>
    /// <param name="includeGenericParameterNames"><c>true</c> to include generic parameter names.</param>
    /// <param name="includeGenericParameters"><c>true</c> to include generic parameters.</param>
    /// <param name="nestedTypeDelimiter">Character to use as a delimiter in nested type names</param>
    /// <returns>The pretty printed type name.</returns>
    public static string GetTypeDisplayName(
        Type type,
        bool fullName = true,
        bool includeGenericParameterNames = false,
        bool includeGenericParameters = true,
        char nestedTypeDelimiter = '+')
    {
        StringBuilder builder = new StringBuilder();
        TypeNameHelper.ProcessType(builder, type,
            new TypeNameHelper.DisplayNameOptions(fullName, includeGenericParameterNames, includeGenericParameters,
                nestedTypeDelimiter));
        return builder.ToString();
    }

#nullable disable
    private static void ProcessType(
        StringBuilder builder,
        Type type,
        in TypeNameHelper.DisplayNameOptions options)
    {
        if (type.IsGenericType)
        {
            Type[] genericArguments = type.GetGenericArguments();
            TypeNameHelper.ProcessGenericType(builder, type, genericArguments, genericArguments.Length, in options);
        }
        else if (type.IsArray)
        {
            TypeNameHelper.ProcessArrayType(builder, type, in options);
        }
        else
        {
            string str1;
            if (TypeNameHelper._builtInTypeNames.TryGetValue(type, out str1))
                builder.Append(str1);
            else if (type.IsGenericParameter)
            {
                if (!options.IncludeGenericParameterNames)
                    return;
                builder.Append(type.Name);
            }
            else
            {
                string str2 = options.FullName ? type.FullName : type.Name;
                builder.Append(str2);
                if (options.NestedTypeDelimiter == '+')
                    return;
                builder.Replace('+', options.NestedTypeDelimiter, builder.Length - str2.Length, str2.Length);
            }
        }
    }

    private static void ProcessArrayType(
        StringBuilder builder,
        Type type,
        in TypeNameHelper.DisplayNameOptions options)
    {
        Type type1 = type;
        while (type1.IsArray)
            type1 = type1.GetElementType();
        TypeNameHelper.ProcessType(builder, type1, in options);
        for (; type.IsArray; type = type.GetElementType())
        {
            builder.Append('[');
            builder.Append(',', type.GetArrayRank() - 1);
            builder.Append(']');
        }
    }

    private static void ProcessGenericType(
        StringBuilder builder,
        Type type,
        Type[] genericArguments,
        int length,
        in TypeNameHelper.DisplayNameOptions options)
    {
        int length1 = 0;
        if (type.IsNested)
            length1 = type.DeclaringType.GetGenericArguments().Length;
        if (options.FullName)
        {
            if (type.IsNested)
            {
                TypeNameHelper.ProcessGenericType(builder, type.DeclaringType, genericArguments, length1, in options);
                builder.Append(options.NestedTypeDelimiter);
            }
            else if (!string.IsNullOrEmpty(type.Namespace))
            {
                builder.Append(type.Namespace);
                builder.Append('.');
            }
        }

        int count = type.Name.IndexOf('`');
        if (count <= 0)
        {
            builder.Append(type.Name);
        }
        else
        {
            builder.Append(type.Name, 0, count);
            if (!options.IncludeGenericParameters)
                return;
            builder.Append('<');
            for (int index = length1; index < length; ++index)
            {
                TypeNameHelper.ProcessType(builder, genericArguments[index], in options);
                if (index + 1 != length)
                {
                    builder.Append(',');
                    if (options.IncludeGenericParameterNames || !genericArguments[index + 1].IsGenericParameter)
                        builder.Append(' ');
                }
            }

            builder.Append('>');
        }
    }

    private readonly struct DisplayNameOptions
    {
        public DisplayNameOptions(
            bool fullName,
            bool includeGenericParameterNames,
            bool includeGenericParameters,
            char nestedTypeDelimiter)
        {
            this.FullName = fullName;
            this.IncludeGenericParameters = includeGenericParameters;
            this.IncludeGenericParameterNames = includeGenericParameterNames;
            this.NestedTypeDelimiter = nestedTypeDelimiter;
        }

        public bool FullName { get; }

        public bool IncludeGenericParameters { get; }

        public bool IncludeGenericParameterNames { get; }

        public char NestedTypeDelimiter { get; }
    }
}
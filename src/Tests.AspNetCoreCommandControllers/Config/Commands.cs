using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.Tests.AspNetCoreCommandControllers;

internal class TestCommand : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.CompletedTask;
    }
}

internal abstract class TestCommand<TResult> : Command<TResult>
{
    protected override Task<TResult> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(default(TResult)!);
    }
}

internal class Test2Command(int id, Code code) : TestCommand<Code>
{
    protected override Task<Code> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        code.Id = id;
        return Task.FromResult(code);
    }
}

internal class Code(string iso)
{
    public int Id { get; set; }
    public string Iso => iso;
}

internal class CodeConverter : JsonConverter<Code>
{
    public override Code? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, options);
        if (dictionary == null)
        {
            return null;
        }

        var id = dictionary.TryGetValue("id", out var idElement) ? idElement.GetInt32() : 0;
        var iso = dictionary.TryGetValue("iso", out var isoElement) ? isoElement.GetString() : string.Empty;

        var code = new Code(iso ?? string.Empty) {Id = id};
        return code;
    }

    public override void Write(Utf8JsonWriter writer, Code value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(Code.Id));
        writer.WriteNumberValue(value.Id);
        writer.WritePropertyName(nameof(Code.Iso));
        writer.WriteStringValue(value.Iso);
        writer.WriteEndObject();
    }
}
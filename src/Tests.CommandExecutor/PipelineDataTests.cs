using System.Threading.Tasks;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class PipelineDataTests
{
    [Fact]
    public async Task InvokeCommandsThatSetPipelineDataAndAssertIsNotSharedBetweenCommands()
    {
        // Arrange
        var executor = Helpers.CreateExecutor();

        // Act & Assert
        await executor.InvokeAsync(new AssertGivenNameAndFamilyNameDoestExistInPipelineData(), default);
        await executor.InvokeAsync(new SetGivenAndFamilyNamesInPipelineData("Ivan", "Montilla"), default);
        await executor.InvokeAsync(new AssertIvanMontillaIsNotInPipelineData(), default);
    }

    [Fact]
    public async Task InvokeCommandsThatSetPipelineDataAndAssertIsSharedBetweenInThePipeline()
    {
        // Arrange
        var executor = Helpers.CreateExecutor([new SetIvanMontillaInPipelineDataMiddleware()]);

        // Act & Assert
        await executor.InvokeAsync(new AssertIvanMontillaIsInPipelineData(), default);
    }
}
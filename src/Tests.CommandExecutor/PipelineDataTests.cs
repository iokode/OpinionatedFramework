using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class PipelineDataTests
{
    [Fact]
    public async Task InvokeCommandsThatSetPipelineDataAndAssertIsNotSharedBetweenCommands()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });

        // Act & Assert
        await executor.InvokeAsync(new AssertGivenNameAndFamilyNameDoestExistInPipelineData(), CancellationToken.None);
        await executor.InvokeAsync(new SetGivenAndFamilyNamesInPipelineData("Ivan", "Montilla"), CancellationToken.None);
        await executor.InvokeAsync(new AssertIvanMontillaIsNotInPipelineData(), CancellationToken.None);
    }

    [Fact]
    public async Task InvokeCommandsThatSetPipelineDataAndAssertIsSharedBetweenInThePipeline()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(options =>
        {
            options.AddMiddleware<SetIvanMontillaInPipelineDataMiddleware>();
        });

        // Act & Assert
        await executor.InvokeAsync(new AssertIvanMontillaIsInPipelineData(), CancellationToken.None);
    }
}
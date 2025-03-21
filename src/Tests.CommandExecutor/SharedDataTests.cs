using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class SharedDataTests
{
    [Fact]
    public async Task InvokeCommandsThatSetAndRemovesSharedData()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });
        
        // Act & Assert
        await executor.InvokeAsync(new AssertGivenNameAndFamilyNameDoestExistInSharedData(), CancellationToken.None);
        await executor.InvokeAsync(new SetGivenAndFamilyNamesInSharedData("Ivan", "Montilla"), CancellationToken.None);
        await executor.InvokeAsync(new AssertIvanMontillaInSharedData(), CancellationToken.None);
        await executor.InvokeAsync(new RemoveGivenAndFamilyNameFromSharedData(), CancellationToken.None);
        await executor.InvokeAsync(new AssertGivenNameAndFamilyNameDoestExistInSharedData(), CancellationToken.None);
    }

    [Fact]
    public async Task InvokeCommandsThatSetAndRemovesSharedDataWithResultCommands()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });
        
        // Act & Assert
        await executor.InvokeAsync<ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData, int>(new ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData(), CancellationToken.None);
        await executor.InvokeAsync<ResultingSetGivenAndFamilyNamesInSharedData, int>(new ResultingSetGivenAndFamilyNamesInSharedData("Ivan", "Montilla"), CancellationToken.None);
        await executor.InvokeAsync<ResultingAssertIvanMontillaIsInSharedData, int>(new ResultingAssertIvanMontillaIsInSharedData(), CancellationToken.None);
        await executor.InvokeAsync<ResultingRemoveGivenAndFamilyNameFromSharedData, int>(new ResultingRemoveGivenAndFamilyNameFromSharedData(), CancellationToken.None);
        await executor.InvokeAsync<ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData, int>(new ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData(), CancellationToken.None);
    }
    
    [Fact]
    public async Task InvokeCommandsThatSetAndRemovesSharedDataWithMixCommands()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });
        
        // Act & Assert
        await executor.InvokeAsync<ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData, int>(new ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData(), CancellationToken.None);
        await executor.InvokeAsync(new SetGivenAndFamilyNamesInSharedData("Ivan", "Montilla"), CancellationToken.None);
        await executor.InvokeAsync<ResultingAssertIvanMontillaIsInSharedData, int>(new ResultingAssertIvanMontillaIsInSharedData(), CancellationToken.None);
        await executor.InvokeAsync<ResultingRemoveGivenAndFamilyNameFromSharedData, int>(new ResultingRemoveGivenAndFamilyNameFromSharedData(), CancellationToken.None);
        await executor.InvokeAsync(new AssertGivenNameAndFamilyNameDoestExistInSharedData(), CancellationToken.None);
    }
}
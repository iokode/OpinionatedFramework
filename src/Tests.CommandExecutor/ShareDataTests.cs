using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class ShareDataTests
{
    [Fact]
    public async Task InvokeCommandsThatSetAndRemovesSharedData()
    {
        // Arrange
        var executor = Helpers.CreateExecutor();
        
        // Act & Assert
        await executor.InvokeAsync(new AssertGivenNameAndFamilyNameDoestExistInSharedData(), default);
        await executor.InvokeAsync(new SetGivenAndFamilyNamesInSharedData("Ivan", "Montilla"), default);
        await executor.InvokeAsync(new AssertIvanMontillaInSharedData(), default);
        await executor.InvokeAsync(new RemoveGivenAndFamilyNameFromShareData(), CancellationToken.None);
        await executor.InvokeAsync(new AssertGivenNameAndFamilyNameDoestExistInSharedData(), default);
    }

    [Fact]
    public async Task InvokeCommandsThatSetAndRemovesSharedDataWithResultCommands()
    {
        // Arrange
        var executor = Helpers.CreateExecutor();
        
        // Act & Assert
        await executor.InvokeAsync<ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData, int>(new ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData(), default);
        await executor.InvokeAsync<ResultingSetGivenAndFamilyNamesInSharedData, int>(new ResultingSetGivenAndFamilyNamesInSharedData("Ivan", "Montilla"), default);
        await executor.InvokeAsync<ResultingAssertIvanMontillaInSharedData, int>(new ResultingAssertIvanMontillaInSharedData(), default);
        await executor.InvokeAsync<ResultingRemoveGivenAndFamilyNameFromShareData, int>(new ResultingRemoveGivenAndFamilyNameFromShareData(), CancellationToken.None);
        await executor.InvokeAsync<ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData, int>(new ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData(), default);
    }
    
    [Fact]
    public async Task InvokeCommandsThatSetAndRemovesSharedDataWithMixCommands()
    {
        // Arrange
        var executor = Helpers.CreateExecutor();
        
        // Act & Assert
        await executor.InvokeAsync<ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData, int>(new ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData(), default);
        await executor.InvokeAsync(new SetGivenAndFamilyNamesInSharedData("Ivan", "Montilla"), default);
        await executor.InvokeAsync<ResultingAssertIvanMontillaInSharedData, int>(new ResultingAssertIvanMontillaInSharedData(), default);
        await executor.InvokeAsync<ResultingRemoveGivenAndFamilyNameFromShareData, int>(new ResultingRemoveGivenAndFamilyNameFromShareData(), CancellationToken.None);
        await executor.InvokeAsync(new AssertGivenNameAndFamilyNameDoestExistInSharedData(), default);
    }
}
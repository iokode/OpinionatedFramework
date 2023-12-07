using IOKode.OpinionatedFramework.Ensuring.Ensurers;

namespace IOKode.OpinionatedFramework.Tests.Ensuring.Ensurers;

public class EnumerableEnsurerTests
{
    private readonly IEnumerable<object> _collectionWithThreeItems = new List<object> { 1, 2, 3 };
    private readonly IEnumerable<object> _collectionWithTwoItems = new List<object> { 1, 2 };
    private readonly IEnumerable<object> _collectionWithOneItem = new List<object> { 1 };
    private readonly IEnumerable<object> _emptyCollection = new List<object>();

    [Fact]
    public void NotEmpty_WithNotEmptyCollection()
    {
        bool result = EnumerableEnsurer.NotEmpty(_collectionWithThreeItems);
        Assert.True(result);
    }

    [Fact]
    public void NotEmpty_WithEmptyCollection()
    {
        bool emptyResult = EnumerableEnsurer.NotEmpty(_emptyCollection);
        Assert.False(emptyResult);
    }

    [Fact]
    public void Empty_WithEmptyCollection()
    {
        bool result = EnumerableEnsurer.Empty(_emptyCollection);
        Assert.True(result);
    }

    [Fact]
    public void Empty_WithNotEmptyCollection()
    {
        bool notEmptyResult = EnumerableEnsurer.Empty(_collectionWithOneItem);
        Assert.False(notEmptyResult);
    }

    [Fact]
    public void Count_WithCountEqualToExpected()
    {
        bool result = EnumerableEnsurer.Count(_collectionWithThreeItems, 3);
        Assert.True(result);
    }

    [Fact]
    public void Count_WithCountNotEqualToExpected()
    {
        bool countIsZeroResult = EnumerableEnsurer.Count(_collectionWithThreeItems, 1);
        Assert.False(countIsZeroResult);
    }

    [Fact]
    public void MinCount_WithMinCountEqualToExpected()
    {
        bool result = EnumerableEnsurer.AtLeastNItems(_collectionWithTwoItems, 2);
        Assert.True(result);
    }

    [Fact]
    public void MinCount_WithMinCountNotEqualToExpected()
    {
        bool minCountIsZeroResult = EnumerableEnsurer.AtLeastNItems(_emptyCollection, 1);
        Assert.False(minCountIsZeroResult);
    }

    [Fact]
    public void MaxCount_WithMaxCountEqualToExpected()
    {
        bool result = EnumerableEnsurer.UpToNItems(_collectionWithThreeItems, 4);
        Assert.True(result);
    }

    [Fact]
    public void MaxCount_WithMaxCountNotEqualToExpected()
    {
        bool maxCountIsZeroResult = EnumerableEnsurer.UpToNItems(_emptyCollection, 1);
        Assert.True(maxCountIsZeroResult);
    }

    [Fact]
    public void CountBetween_WithCountInRange()
    {
        bool result = EnumerableEnsurer.CountBetween(_collectionWithTwoItems, 1, 3);
        Assert.True(result);
    }

    [Fact]
    public void CountBetween_WithCountBelowRange()
    {
        bool countBelowRangeResult = EnumerableEnsurer.CountBetween(_collectionWithOneItem, 2, 4);
        Assert.False(countBelowRangeResult);
    }

    [Fact]
    public void CountBetween_WithCountAboveRange()
    {
        bool countAboveRangeResult = EnumerableEnsurer.CountBetween(_collectionWithThreeItems, 1, 2);
        Assert.False(countAboveRangeResult);
    }

    [Fact]
    public void In_WithAllValuesPresent()
    {
        bool result = EnumerableEnsurer.In(_collectionWithThreeItems, 1, 2, 3);
        Assert.True(result);
    }

    [Fact]
    public void In_WithSomeValuesMissing()
    {
        bool result = EnumerableEnsurer.In(_collectionWithTwoItems, 1, 2, 3);
        Assert.False(result);
    }

    [Fact]
    public void All_WithAllItemsSatisfyingPredicate()
    {
        bool result = EnumerableEnsurer.All(_collectionWithThreeItems, x => x is int);
        Assert.True(result);
    }

    [Fact]
    public void All_WithSomeItemsNotSatisfyingPredicate()
    {
        bool result = EnumerableEnsurer.All(_collectionWithThreeItems, x => x is string);
        Assert.False(result);
    }

    [Fact]
    public void Any_WithAtLeastOneItemSatisfyingPredicate()
    {
        bool result = EnumerableEnsurer.Any(_collectionWithThreeItems, x => x is int);
        Assert.True(result);
    }

    [Fact]
    public void Any_WithNoItemsSatisfyingPredicate()
    {
        bool result = EnumerableEnsurer.Any(_collectionWithThreeItems, x => x is string);
        Assert.False(result);
    }
}
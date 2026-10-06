using Xunit;

// The two fixtures of this assembly both bind the broker to the same host port and bootstrap the static
// Container, so their collections cannot run at the same time.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

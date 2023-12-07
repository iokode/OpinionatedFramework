using Xunit;

// The CommandExecutor tests do not run in parallel because they rely on the initialized Container class, 
// which is not thread-safe. Disabling test parallelization ensures that these tests don't interfere 
// with each other by trying to initialize or use the container at the same time.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

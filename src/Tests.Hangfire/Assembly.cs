using Xunit;

// The serializability test bootstraps the framework on its own, and the jobs fixture bootstraps it for its
// whole collection, so the two collections cannot run at the same time over the static Container.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

using Xunit;

// The bootstrapping tests do not run in parallel because they all register drivers into the static Container,
// which they reset between tests. Disabling test parallelization keeps one test class from observing the
// registrations of another.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

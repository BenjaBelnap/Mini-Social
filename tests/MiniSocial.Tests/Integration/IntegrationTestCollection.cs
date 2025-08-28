using Xunit;

namespace MiniSocial.Tests.Integration;

/// <summary>
/// Collection definition to ensure integration tests run sequentially.
/// This prevents database state conflicts between parallel test executions.
/// </summary>
[CollectionDefinition("Integration Tests", DisableParallelization = true)]
public class IntegrationTestCollection
{
    // This class is just a marker for the collection definition
    // The actual tests will be marked with [Collection("Integration Tests")]
}

namespace ZelaFix.Tests.Integration.Fixtures;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "IntegrationTestCollection";
}
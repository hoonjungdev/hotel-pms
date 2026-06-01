namespace HotelPms.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "IntegrationTest";
}

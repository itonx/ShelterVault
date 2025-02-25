namespace ShelterVault.Models
{
    public record CosmosDBSettings
    (
        string CosmosEndpoint,
        string CosmosKey,
        string CosmosDatabase,
        string CosmosContainer
    )
    {

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CosmosEndpoint) &&
                !string.IsNullOrWhiteSpace(CosmosKey) &&
                !string.IsNullOrWhiteSpace(CosmosDatabase) &&
                !string.IsNullOrWhiteSpace(CosmosContainer);
        }
    }
}

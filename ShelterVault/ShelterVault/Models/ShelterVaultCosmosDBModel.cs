using System;
using System.Collections.Generic;

namespace ShelterVault.Models
{
    public enum SourceType
    {
        Local,
        CosmosDB
    }

    public interface ICosmosDBModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public long version { get; set; }
    }

    public record CosmosDBVault
    (
        string id,
        string name,
        string encryptedTestValue,
        string iv,
        string salt,
        long version
    ) : ICosmosDBModel
    {
        public string id { get; set; } = id;
        public string type { get; set; } = "shelter_vault";
        public long version { get; set; } = version;
    }

    public record CosmosDBCredentials
    (
        string id,
        string encryptedValues,
        string iv,
        string shelterVaultUuid,
        long version
    ) : ICosmosDBModel
    {
        public string id { get; set; } = id;
        public string type { get; set; } = "shelter_vault_credentials";
        public long version { get; set; } = version;
    }

    public record CosmosDBSyncModel
    (
        string id,
        string type,
        long version,
        SourceType source
    ) : ICosmosDBModel
    {
        public string id { get; set; } = id;
        public string type { get; set; } = type;
        public long version { get; set; } = version;
        public bool IsNew { get; set; } = false;
#nullable enable
        public virtual bool Equals(CosmosDBSyncModel? other)
        {
            return id == other?.id && type == other?.type && version == other?.version && source == other.source;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, type, version, source);
        }
    }

    public record CosmosDBTinyModel
    (
        string id,
        string type,
        long version
    ) : ICosmosDBModel
    {
        public string id { get; set; } = id;
        public string type { get; set; } = type;
        public long version { get; set; } = version;

        public static IList<CosmosDBSyncModel> ToCosmosDBSyncModel(IList<CosmosDBTinyModel> cosmosDBTinyModels)
        {
            IList<CosmosDBSyncModel> cosmosDBSyncModels = new List<CosmosDBSyncModel>();
            foreach (CosmosDBTinyModel cosmosDBTinyModel in cosmosDBTinyModels)
            {
                cosmosDBSyncModels.Add(new CosmosDBSyncModel(cosmosDBTinyModel.id, cosmosDBTinyModel.type, cosmosDBTinyModel.version, SourceType.CosmosDB));
            }
            return cosmosDBSyncModels;
        }
    }
}
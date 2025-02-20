using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        string masterKeyHash,
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

        public virtual bool Equals(CosmosDBSyncModel other)
        {
            return id == other.id && type == other.type && version == other.version && source == other.source;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, type, version, source);
        }
    }
}

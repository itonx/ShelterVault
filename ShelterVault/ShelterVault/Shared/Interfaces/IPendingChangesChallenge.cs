using System.Threading.Tasks;

namespace ShelterVault.Shared.Interfaces
{
    public interface IPendingChangesChallenge
    {
        Task<bool> DiscardChangesAsync(bool completeChallenge = false);
        bool ChallengeCompleted { get; }
    }
}

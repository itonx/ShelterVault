using System.Threading.Tasks;

namespace Desktiny.UI.Interfaces
{
    public interface IPendingChangesChallenge
    {
        Task<bool> DiscardChangesAsync(bool completeChallenge = false);
        bool ChallengeCompleted { get; }
    }
}

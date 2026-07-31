using System.Threading.Tasks;

namespace ShelterVault.Interfaces
{
    public interface IProgressBarService
    {
        Task Show();
        Task Hide();
    }
}

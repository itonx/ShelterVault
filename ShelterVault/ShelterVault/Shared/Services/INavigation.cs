using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Services
{
    public interface INavigation
    {
        void OnNavigateTo(object parameter);
    }
}

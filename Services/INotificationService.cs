using System.Threading.Tasks;
using Rafiq.Models;

namespace Rafiq.Services
{
    public interface INotificationService
    {
        Task SendOrderAlertAsync ( Order order, string customerName, string status );
    }
}
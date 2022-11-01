using System.Threading.Tasks;

namespace WebApp.Jobs
{
    public interface RJob
    {
        Task Run();
    }
}
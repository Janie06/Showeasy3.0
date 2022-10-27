using Quartz;
using System.Threading.Tasks;

namespace WebApp.Jobs.Outlook
{
    public class RunSynJob : IJob
    {
        /// <summary>
        /// Called by the <see cref="IScheduler"/> when a <see cref="ITrigger"/> fires that is
        /// associated with the <see cref="IJob"/>.
        /// </summary>
        /// <param name="context">todo: describe context parameter on Execute</param>
        public virtual async Task Execute(IJobExecutionContext context) => await TaskUtil.CompletedTask;
    }
}
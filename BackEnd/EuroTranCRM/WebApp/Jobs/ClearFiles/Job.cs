using log4net;
using Quartz;
using System;
using System.Threading.Tasks;

namespace WebApp.Jobs.ClearFiles
{
    public class ClearFilesJob : IJob
    {
        private static ILog log = LogManager.GetLogger(typeof(RunJob));

        /// <summary>
        /// Called by the <see cref="IScheduler"/> when a <see cref="ITrigger"/> fires that is
        /// associated with the <see cref="IJob"/>.
        /// </summary>
        /// <param name="context">todo: describe context parameter on Execute</param>
        public virtual async Task Execute(IJobExecutionContext context)
        {
            // Say Hello to the World and display the date/time
            log.Debug($"Hello World! - {DateTime.Now:r}");
            await TaskUtil.CompletedTask;
        }
    }
}
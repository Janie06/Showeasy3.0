using log4net;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace WebApp.Jobs.Outlook
{
    public class RunSyn : RJob
    {
        private static ILog log = LogManager.GetLogger(typeof(RunSyn));

        public virtual async Task Run()
        {
            // First we must get a reference to a scheduler
            ISchedulerFactory sf = new StdSchedulerFactory();
            var sched = await sf.GetScheduler();

            // get a "nice round" time a few seconds in the future...
            var startTime = DateBuilder.NextGivenSecondDate(null, 35);
            // job1 will only fire once at date/time "ts"
            var job = JobBuilder.Create<RunSynJob>()
                .WithIdentity("job1", "group1")
                .Build();

            var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartAt(startTime)
                //.WithSimpleSchedule(x => x.WithIntervalInSeconds(40).RepeatForever())
                .Build();
            DateTimeOffset? ft = await sched.ScheduleJob(job, trigger);
            await sched.Start();
            await sched.Shutdown(true);
        }
    }
}
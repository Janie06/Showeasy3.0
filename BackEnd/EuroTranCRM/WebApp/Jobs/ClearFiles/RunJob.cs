using log4net;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace WebApp.Jobs.ClearFiles
{
    public class RunJob : RJob
    {
        private static ILog log = LogManager.GetLogger(typeof(RunJob));

        public virtual async Task Run()
        {
            log.Debug("------- Initializing -------------------");

            // First we must get a reference to a scheduler
            ISchedulerFactory sf = new StdSchedulerFactory();
            var sched = await sf.GetScheduler();

            log.Debug("------- Initialization Complete --------");

            log.Debug("------- Scheduling Jobs ----------------");

            // jobs can be scheduled before sched.start() has been called

            // get a "nice round" time a few seconds in the future...
            var startTime = DateBuilder.NextGivenSecondDate(null, 15);

            // job1 will only fire once at date/time "ts"
            var job = JobBuilder.Create<ClearFilesJob>()
                .WithIdentity("job1", "group1")
                .Build();

            var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartAt(startTime)
                .Build();

            // schedule it to run!
            DateTimeOffset? ft = await sched.ScheduleJob(job, trigger);
            log.Debug(job.Key +
                     " will run at: " + ft +
                     " and repeat: " + trigger.RepeatCount +
                     " times, every " + trigger.RepeatInterval.TotalSeconds + " seconds");

            // job2 will only fire once at date/time "ts"
            job = JobBuilder.Create<ClearFilesJob>()
                .WithIdentity("job2", "group1")
                .Build();

            trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger2", "group1")
                .StartAt(startTime)
                .Build();

            ft = await sched.ScheduleJob(job, trigger);
            log.Debug(job.Key +
                     " will run at: " + ft +
                     " and repeat: " + trigger.RepeatCount +
                     " times, every " + trigger.RepeatInterval.TotalSeconds + " seconds");

            // job3 will run 11 times (run once and repeat 10 more times) job3 will repeat every 10 seconds
            job = JobBuilder.Create<ClearFilesJob>()
                .WithIdentity("job3", "group1")
                .Build();

            trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger3", "group1")
                .StartAt(startTime)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).WithRepeatCount(10))
                .Build();

            ft = await sched.ScheduleJob(job, trigger);
            log.Debug(job.Key +
                     " will run at: " + ft +
                     " and repeat: " + trigger.RepeatCount +
                     " times, every " + trigger.RepeatInterval.TotalSeconds + " seconds");

            // the same job (job3) will be scheduled by a another trigger this time will only repeat
            // twice at a 70 second interval

            trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger3", "group2")
                .StartAt(startTime)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).WithRepeatCount(2))
                .ForJob(job)
                .Build();

            ft = await sched.ScheduleJob(trigger);
            log.Debug(job.Key +
                     " will [also] run at: " + ft +
                     " and repeat: " + trigger.RepeatCount +
                     " times, every " + trigger.RepeatInterval.TotalSeconds + " seconds");

            // job4 will run 6 times (run once and repeat 5 more times) job4 will repeat every 10 seconds
            job = JobBuilder.Create<ClearFilesJob>()
                .WithIdentity("job4", "group1")
                .Build();

            trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger4", "group1")
                .StartAt(startTime)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).WithRepeatCount(5))
                .Build();

            ft = await sched.ScheduleJob(job, trigger);
            log.Debug(job.Key +
                     " will run at: " + ft +
                     " and repeat: " + trigger.RepeatCount +
                     " times, every " + trigger.RepeatInterval.TotalSeconds + " seconds");

            // job5 will run once, five minutes in the future
            job = JobBuilder.Create<ClearFilesJob>()
                .WithIdentity("job5", "group1")
                .Build();

            trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger5", "group1")
                .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Minute))
                .Build();

            ft = await sched.ScheduleJob(job, trigger);
            log.Debug(job.Key +
                     " will run at: " + ft +
                     " and repeat: " + trigger.RepeatCount +
                     " times, every " + trigger.RepeatInterval.TotalSeconds + " seconds");

            // job6 will run indefinitely, every 40 seconds
            job = JobBuilder.Create<ClearFilesJob>()
                .WithIdentity("job6", "group1")
                .Build();

            trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger6", "group1")
                .StartAt(startTime)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(40).RepeatForever())
                .Build();

            ft = await sched.ScheduleJob(job, trigger);
            log.Debug(job.Key +
                     " will run at: " + ft +
                     " and repeat: " + trigger.RepeatCount +
                     " times, every " + trigger.RepeatInterval.TotalSeconds + " seconds");

            log.Debug("------- Starting Scheduler ----------------");

            // All of the jobs have been added to the scheduler, but none of the jobs will run until
            // the scheduler has been started
            await sched.Start();

            log.Debug("------- Started Scheduler -----------------");

            // jobs can also be scheduled after start() has been called... job7 will repeat 20 times,
            // repeat every five minutes
            job = JobBuilder.Create<ClearFilesJob>()
                .WithIdentity("job7", "group1")
                .Build();

            trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger7", "group1")
                .StartAt(startTime)
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).WithRepeatCount(20))
                .Build();

            ft = await sched.ScheduleJob(job, trigger);
            log.Debug(job.Key +
                     " will run at: " + ft +
                     " and repeat: " + trigger.RepeatCount +
                     " times, every " + trigger.RepeatInterval.TotalSeconds + " seconds");

            // jobs can be fired directly... (rather than waiting for a trigger)
            job = JobBuilder.Create<ClearFilesJob>()
                .WithIdentity("job8", "group1")
                .StoreDurably()
                .Build();

            await sched.AddJob(job, true);

            log.Debug("'Manually' triggering job8...");
            await sched.TriggerJob(new JobKey("job8", "group1"));

            log.Debug("------- Waiting 30 seconds... --------------");

            try
            {
                // wait 30 seconds to show jobs
                await Task.Delay(TimeSpan.FromSeconds(30));
                // executing...
            }
            catch (Exception)
            {
                throw;
            }

            // jobs can be re-scheduled... job 7 will run immediately and repeat 10 times for every second
            log.Debug("------- Rescheduling... --------------------");
            trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger7", "group1")
                .StartAt(startTime)
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).WithRepeatCount(20))
                .Build();

            ft = await sched.RescheduleJob(trigger.Key, trigger);
            log.Debug("job7 rescheduled to run at: " + ft);

            log.Debug("------- Waiting five minutes... ------------");
            // wait five minutes to show jobs
            await Task.Delay(TimeSpan.FromMinutes(5));
            // executing...

            log.Debug("------- Shutting Down ---------------------");

            await sched.Shutdown(true);

            log.Debug("------- Shutdown Complete -----------------");

            // display some stats about the schedule that just ran
            var metaData = await sched.GetMetaData();
            log.Debug($"Executed {metaData.NumberOfJobsExecuted} jobs.");
        }
    }
}
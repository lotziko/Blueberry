using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Blueberry
{
    public static class Timer
    {
        static Dictionary<Task, CancellationTokenSource> _scheduledTasks = new Dictionary<Task, CancellationTokenSource>();


        public static void Schedule(Task task, TimeSpan delay)
        {
            Schedule(task, TimeSpan.FromSeconds(0), delay, 1);
        }

        public static void Schedule(Task task, TimeSpan interval, TimeSpan delay)
        {
            Schedule(task, interval, delay, 1);
        }

        public static void Schedule(Task task, TimeSpan interval, TimeSpan delay, int count)
        {
            if (_scheduledTasks.ContainsKey(task) || task.IsScheduled())
            {
                return;
            }

            var cts = new CancellationTokenSource();
            _scheduledTasks.Add(task, cts);

            var tasks = _scheduledTasks;

            task.Schedule(true);
            ThreadPool.QueueUserWorkItem(s => 
            {
                bool delayed = delay.Milliseconds > 0;
                while(count == -1 || count > 0)
                {
                    CancellationToken token = (CancellationToken)s;
                    if (token.IsCancellationRequested)
                    {
                        task.Schedule(false);
                        _scheduledTasks.Remove(task);
                        return;
                    }
                        
                    if (delayed)
                    {
                        var watch = new Stopwatch();
                        watch.Start();
                        while (!token.IsCancellationRequested && watch.Elapsed <  delay)
                        {
                            
                        }
                        watch.Stop();
                    }
                    task.Run();
                    {
                        var watch = new Stopwatch();
                        watch.Start();
                        while (token.IsCancellationRequested && watch.Elapsed < interval)
                        {
                            
                        }
                        watch.Stop();
                    }
                    --count;
                }
                task.Schedule(false);
                _scheduledTasks.Remove(task);
                return;
            }, cts.Token);
        }

        internal static void Cancel(Task task)
        {
            var tsks = _scheduledTasks;
            if (!_scheduledTasks.ContainsKey(task)) return;
            _scheduledTasks[task].Cancel();
            _scheduledTasks.Remove(task);
        }

        public static bool IsScheduled(Task task)
        {
            return _scheduledTasks.ContainsKey(task);
        }
    }

    public abstract class Task
    {
        private bool scheduled;

        public abstract void Run();

        public bool IsScheduled()
        {
            return scheduled;
        }

        internal void Schedule(bool schedule)
        {
            scheduled = schedule;
        }

        public void Cancel()
        {
            Timer.Cancel(this);
        }
    }
}

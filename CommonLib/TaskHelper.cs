using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLib
{
    public class TaskHelper
    {
        private static readonly TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        public static void RunActionInTask(Action action)
        {
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).ContinueWith(task =>
            {
                if (task.Exception != null)
                    task.Exception.Handle(ex =>
                    {
                        LogSupport.Error(ex);
                        return true;
                    });
            }, uiScheduler);
        }
    }
}

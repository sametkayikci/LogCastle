using System.Threading.Tasks;
using LogCastle.Abstractions;
using LogCastle.Extensions;

namespace LogCastle.Logging
{
    public sealed class TaskLoggable : ILoggable
    {
        private readonly Task _task;
        public TaskLoggable(Task task)
        {
            _task = task;
        }

        public string ToLogString()
        {           
            var resultProperty = _task.GetType().GetProperty("Result");
            if (resultProperty is null)
                return "Task completed without result";

            var result = resultProperty.GetValue(_task);
            return result.ToDetailedLogString();
        }
    }
}
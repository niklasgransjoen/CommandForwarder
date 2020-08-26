using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommandForwarder
{
    public sealed class ActionExecutionException : Exception
    {
        public ActionExecutionException()
        {
        }

        public ActionExecutionException(string message) : base(message)
        {
        }

        public ActionExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    internal interface IActionExecutor
    {
        /// <summary>
        /// Executes an action.
        /// </summary>
        /// <exception cref="ActionExecutionException">Fails to execute action.</exception>
        void Execute(Action action, IEnumerable<string> args);
    }

    internal sealed class ActionExecutor : IActionExecutor
    {
        public void Execute(Action action, IEnumerable<string> args)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = action.Command,
                };

                foreach (var arg in args)
                {
                    startInfo.ArgumentList.Add(arg);
                }

                var process = Process.Start(startInfo);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new ActionExecutionException($"Failed to execute action '{action.Name}'.", ex);
            }
        }
    }
}
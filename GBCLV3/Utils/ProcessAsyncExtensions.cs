using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GBCLV3.Utils
{
    static class ProcessAsyncExtensions
    {
        public static async ValueTask<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            if (process.HasExited)
            {
                return process.ExitCode;
            }

            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            process.EnableRaisingEvents = true;

            void handler(object s, EventArgs args) => tcs.TrySetResult(process.ExitCode);
            process.Exited += handler;

            try
            {
                if (process.HasExited)
                {
                    return process.ExitCode;
                }

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
                }

                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                process.Exited -= handler;
            }
        }
    }
}

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace Oculus.Platform
{
  public static class OculusPlatformExtensions
  {
    public static async Task<Message<T>> WaitAsync<T>(
      this Request<T> oculusRequest,
      CancellationToken cancellationToken)
    {
      TaskCompletionSource<Message<T>> tcs = new TaskCompletionSource<Message<T>>();
      oculusRequest.OnComplete((Message<T>.Callback) (result => tcs.TrySetResult(result)));
      await using (cancellationToken.Register((Action) (() => tcs.TrySetCanceled(cancellationToken))))
        return await tcs.Task;
    }

    public static async Task<Message<T>> WaitWithTimeoutAsync<T>(
      this Request<T> oculusRequest,
      TimeSpan timeout,
      CancellationToken cancellationToken)
    {
      using (CancellationTokenSource timeoutTokenSource = new CancellationTokenSource(timeout))
      {
        if (!cancellationToken.CanBeCanceled)
          return await oculusRequest.WaitAsync<T>(timeoutTokenSource.Token);
        using (CancellationTokenSource combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token))
          return await oculusRequest.WaitAsync<T>(combinedTokenSource.Token);
      }
    }

    public static TaskAwaiter<Message<T>> GetAwaiter<T>(this Request<T> oculusRequest)
    {
      TaskCompletionSource<Message<T>> completionSource = new TaskCompletionSource<Message<T>>();
      oculusRequest.OnComplete(new Message<T>.Callback(completionSource.SetResult));
      return completionSource.Task.GetAwaiter();
    }

    public static TaskAwaiter<Message> GetAwaiter(this Request oculusRequest)
    {
      TaskCompletionSource<Message> completionSource = new TaskCompletionSource<Message>();
      oculusRequest.OnComplete(new Message.Callback(completionSource.SetResult));
      return completionSource.Task.GetAwaiter();
    }
  }
}

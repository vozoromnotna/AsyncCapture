using System.Collections.Concurrent;

namespace AsyncCapture.Core.Sinks;

public class BufferedSink<T> : ISink<T>
{
    private readonly ConcurrentQueue<(T, Dictionary<string, object>)> _queue = new ();
    public Task PutImage(T image, Dictionary<string, object> meta)
    {
       _queue.Enqueue((image, meta));
       return Task.CompletedTask;
    }

    public bool TryGetImage(out (T, Dictionary<string, object>) image)
    {
        return _queue.TryDequeue(out image);
    }
    
}
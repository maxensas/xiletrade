using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Notification;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Library.Services;

/// <summary>UI-specific implementation of the Core contract. State and expiry are owned by this provider.</summary>
public sealed class NotificationService : INotificationService, IDisposable
{
    private readonly INotificationHost _viewHost;

    private readonly Lock _gate = new();
    private readonly Dictionary<string, ToastNotification> _items = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _expiration
        = new(StringComparer.Ordinal);

    private bool _disposed;

    public event EventHandler<ToastNotification> NotificationAdded;
    public event EventHandler<string> NotificationRemoved;

    public IReadOnlyList<ToastNotification> Current
    {
        get
        {
            lock (_gate) return [.. _items.Values.OrderBy(x => x.CreatedAt)];
        }
    }

    public NotificationService(Func<Action<string>, INotificationHost> hostFactory)
    {
        ArgumentNullException.ThrowIfNull(hostFactory);

        _viewHost = hostFactory(id => Dismiss(id));

        NotificationAdded += _viewHost.OnAdded;
        NotificationRemoved += _viewHost.OnRemoved;

        foreach (var item in Current)
            _viewHost.OnAdded(this, item);
    }

    public void Show(ToastNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(notification.Id);

        bool replaced;
        lock (_gate)
        {
            replaced = _items.ContainsKey(notification.Id);
            _items[notification.Id] = notification;
        }

        CancelExpiration(notification.Id);

        if (replaced) NotificationRemoved?.Invoke(this, notification.Id);
        NotificationAdded?.Invoke(this, notification);

        if (notification.Duration is { } duration && duration > TimeSpan.Zero)
        {
            var cts = new CancellationTokenSource();
            _expiration[notification.Id] = cts;
            _ = ExpireAsync(notification.Id, duration, cts);
        }
    }

    public bool Dismiss(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        bool removed;
        lock (_gate)
        {
            removed = _items.Remove(id);
        }

        CancelExpiration(id);

        if (removed) NotificationRemoved?.Invoke(this, id);
        return removed;
    }

    public void DismissAll()
    {
        string[] ids;
        lock (_gate)
        {
            ids = _items.Keys.ToArray();
            _items.Clear();
        }
        foreach (var id in ids)
        {
            CancelExpiration(id);
            NotificationRemoved?.Invoke(this, id);
        }
    }

    private async Task ExpireAsync(string id, TimeSpan duration, CancellationTokenSource cts)
    {
        try
        {
            // Awaiting through WhenAny does not rethrow the cancellation (no TaskCanceledException when the toast is dismissed early).
            var delay = Task.Delay(duration, cts.Token);
            await Task.WhenAny(delay).ConfigureAwait(false);
            if (!delay.IsCanceled)
            {
                Dismiss(id);
            }
        }
        finally
        {
            if (_expiration.TryGetValue(id, out var current) && ReferenceEquals(current, cts))
            {
                _expiration.TryRemove(id, out _);
            }
            cts.Dispose();
        }
    }

    private void CancelExpiration(string id)
    {
        if (_expiration.TryRemove(id, out var cts)) cts.Cancel();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DismissAll();
        foreach (var id in _expiration.Keys.ToArray())
        {
            CancelExpiration(id);
        }
        NotificationAdded -= _viewHost.OnAdded; 
        NotificationRemoved -= _viewHost.OnRemoved;
        _viewHost.Dispose();
    }
}

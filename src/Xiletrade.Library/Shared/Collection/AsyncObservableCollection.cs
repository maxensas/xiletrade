using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Xiletrade.Library.Shared.Collection;

public sealed class AsyncObservableCollection<T> : ObservableCollection<T>
{
    private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current is not null ? SynchronizationContext.Current : Services.XiletradeService.UiThreadContext ;

    public AsyncObservableCollection()
    {
    }

    public AsyncObservableCollection(IEnumerable<T> list) : base(list)
    {
    }

    private void ExecuteOnSyncContext(Action action)
    {
        if (SynchronizationContext.Current == _synchronizationContext)
        {
            action();
            return;
        }
        _synchronizationContext.Send(_ => action(), null);
    }

    protected override void InsertItem(int index, T item)
    {
        ExecuteOnSyncContext(() => base.InsertItem(index, item));
    }

    protected override void RemoveItem(int index)
    {
        ExecuteOnSyncContext(() => base.RemoveItem(index));
    }

    protected override void SetItem(int index, T item)
    {
        ExecuteOnSyncContext(() => base.SetItem(index, item));
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        ExecuteOnSyncContext(() => base.MoveItem(oldIndex, newIndex));
    }

    protected override void ClearItems()
    {
        ExecuteOnSyncContext(() => base.ClearItems());
    }

    public void ReplaceRange(IEnumerable<T> items)
    {
        ExecuteOnSyncContext(() =>
        {
            base.ClearItems();

            foreach (var item in items)
                base.InsertItem(Count, item);
        });
    }
}

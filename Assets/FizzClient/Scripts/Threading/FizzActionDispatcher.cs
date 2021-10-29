using System;
using System.Collections.Generic;
using Fizz.Common;

namespace Fizz.Threading
{
    public class FizzActionDispatcher: IFizzActionDispatcher
    {
        // TODO: Switch to ReaderWriterLock
        private readonly object synclock = new object();
        private readonly SortedList<long, Action> timers = new SortedList<long, Action>(new DuplicateKeyComparer<long> ());

        public void Post(Action action)
        {
            if (action == null)
            {
                FizzLogger.W("Empty action scheduled posted");
                return;
            }

            Delay(0, action);
        }

        public void Delay(int delayMS, Action action)
        {
            if (action == null || delayMS < 0)
            {
                FizzLogger.W("Invalid timer scheduled");
                return;
            }
            lock (synclock)
            {
                timers.Add(FizzUtils.Now() + delayMS, action);
            }
        }

        public void Process()
        {
            long now = FizzUtils.Now();
            if (timers.Count <= 0 || timers.Keys[0] > now)
            {
                return;
            }
            List<Action> timersToDispatch = new List<Action>();

            lock(synclock)
            {
                while (timers.Count > 0 && timers.Keys[0] <= now)
                {
                    timersToDispatch.Add(timers.Values[0]);
                    timers.RemoveAt(0);
                }
            }

            foreach (var timer in timersToDispatch)
            {
                SafeInvoke(timer);
            }
        }

        private void SafeInvoke(Action callback)
        {
            try
            {
                callback.Invoke();
            }
            catch (Exception ex)
            {
                FizzLogger.E("Dispatched action threw except:\n" + ex.StackTrace);
            }
        }
    }

    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1;   // Handle equality as beeing greater
            else
                return result;
        }

        #endregion
    }
}

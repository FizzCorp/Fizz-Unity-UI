using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Fizz.Common
{
    public static class FizzUtils
    {
        public static void DoCallback(Action callback)
        {
            if (callback != null)
            {
                try
                {
                    callback.Invoke();
                }
                catch
                {
                    FizzLogger.W("Callback threw exception");
                }
            }
        }

        public static void DoCallback(FizzException ex, Action<FizzException> callback)
        {
            if (callback != null)
            {
                try
                {
                    callback(ex);
                }
                catch
                {
                    FizzLogger.W("Callback threw exception");
                }
            }
        }

        public static void DoCallback<TResult>(TResult result, FizzException ex, Action<TResult, FizzException> callback)
        {
            if (callback != null)
            {
                try
                {
                    callback(result, ex);
                }
                catch (Exception callbackEx)
                {
                    FizzLogger.W("Callback threw exception: " + callbackEx.Message);
                }
            }
        }

        public static long Now()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            long cur_time = (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;
            return cur_time;
        }

        public static IDictionary<string,string> headers(string sessionToken) {
            return new Dictionary<string, string> {
                    { FizzConfig.API_HEADER_SESSION_TOKEN, sessionToken }
            };
        }

        public static Action SafeCallback(Action callback)
        {
            bool called = false;
            return () => 
            {
                Debug.Assert(called == false);
                called = true;
                if (callback != null)
                {
                    callback.Invoke();
                }
            };
        }

        public static Action<TValue1, TValue2> SafeCallback<TValue1, TValue2>(Action<TValue1, TValue2> callback)
        {
            bool called = false;
            return (v1, v2) =>
            {
                Debug.Assert(called == false);
                called = true;
                callback.Invoke(v1, v2);
            };
        }
    }
}

using System;

using Fizz.Common;

namespace Fizz.Chat.Impl 
{
    public class FizzUsers: IFizzUsers 
    {
        private IFizzAuthRestClient _client;

        public void Open(IFizzAuthRestClient client) 
        {
            IfClosed(() => 
            {
                _client = client;
            });
        }

        public void Close() 
        {
            IfOpened(() => 
            {
                _client = null;
            });
        }

        public IFizzFetchUserGroupsQuery BuildFetchUserGroupsQuery(string userId) 
        {
            return new FizzFetchUserGroupsQuery(userId, _client);
        }

        public void JoinGroup(string userId, string groupId, Action<FizzException> callback) 
        {
            IfOpened(() => 
            {
                string path = string.Format(FizzConfig.API_PATH_USER_GROUP, userId, groupId);
                _client.Post(FizzConfig.API_BASE_URL, path, string.Empty, (response, ex) => 
                {
                    FizzUtils.DoCallback(ex, callback);
                });
            });
        }

        public void RemoveGroup(string userId, string groupId, Action<FizzException> callback) 
        {
            IfOpened(() => 
            {
                string path = string.Format(FizzConfig.API_PATH_USER_GROUP, userId, groupId);

                _client.Delete(FizzConfig.API_BASE_URL, path, string.Empty, (response, ex) => 
                {
                    FizzUtils.DoCallback(ex, callback);
                });
            });
        }

        private void IfOpened (Action callback)
        {
            if (_client != null)
            {
                FizzUtils.DoCallback (callback);
            }
            else
            {
                FizzLogger.W ("Client should have been opened.");
            }
        }

        private void IfClosed (Action callback)
        {
            if (_client == null)
            {
                FizzUtils.DoCallback (callback);
            }
            else
            {
                FizzLogger.W ("Client should have been closed.");
            }
        }
    }
}
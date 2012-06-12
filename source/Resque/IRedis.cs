using System;
using System.Collections.Generic;

namespace Resque
{
    public interface IRedis
    {
        string LPop(string key);
        void Set(string key, string value);
        long RemoveKeys(params string[] keys);
        bool Exists(string key);
        string Get(string key);

        bool SAdd(string key, string redisId);
        long SRemove(string key, params  string[] values);
        IEnumerable<string> SMembers(string key);
        long RPush(string key, string value);

        Tuple<string, string> BLPop(string[] keys, int timeoutSeconds = 0);

        Dictionary<string, string> HGetAll(string key);
        void HSet(string key, string field, string value);
    }
}
using System;
using System.Collections.Generic;

namespace Resque
{
    public interface IRedis
    {
        bool SAdd(string key, string redisId);
        string LPop(string key);
        void Set(string key, string value);
        long RemoveKeys(params string[] keys);
        long SRemove(string key, params  string[] values);
        long RPush(string key, string value);
        Tuple<string, string> BLPop(string[] keys, int timeoutSeconds = 0);
        IEnumerable<string> SMembers(string key);
        bool Exists(string key);
        string Get(string key);
    }
}
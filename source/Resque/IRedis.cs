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
        string HGet(string key, string field);
        bool HSet(string key, string field, string value);

        bool ZAdd(string key, string value, long score);
        long ZCard(string key);
        long ZCard(string key, long min, long max);
        Tuple<string, double>[] ZRange(string key, long start, long stop, bool ascending = false);
        double ZScore(string key, string member);
    }
}
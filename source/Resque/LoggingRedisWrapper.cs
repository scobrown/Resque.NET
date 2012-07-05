using System;
using System.Collections.Generic;

namespace Resque
{
    public class LoggingRedisWrapper : IRedis
    {
        private readonly IRedis redis;

        public string LPop(string key)
        {
            Console.Out.WriteLine("LPOP " + key);
            return redis.LPop(key);
        }

        public void Set(string key, string value)
        {
            Console.Out.WriteLine("SET {0} {1}",key,value);
            redis.Set(key, value);
        }

        public long RemoveKeys(params string[] keys)
        {
            Console.Out.WriteLine("DEL {0}",string.Join(" ",keys));
            return redis.RemoveKeys(keys);
        }

        public bool Exists(string key)
        {
            Console.Out.WriteLine("EXISTS " + key);
            return redis.Exists(key);
        }

        public bool SAdd(string key, string redisId)
        {
            Console.Out.WriteLine("SADD {0} {1}",key,redisId);
            return redis.SAdd(key, redisId);
        }

        public string Get(string key)
        {
            Console.Out.WriteLine("GET {0}",key);
            return redis.Get(key);
        }

        public long SRemove(string key, params string[] values)
        {
            Console.Out.WriteLine("SREMOVE {0} {1}",key, string.Join(" ",values));
            return redis.SRemove(key, values);
        }

        public IEnumerable<string> SMembers(string key)
        {
            Console.Out.WriteLine("SMEMBERS {0}",key);
            return redis.SMembers(key);
        }

        public long RPush(string key, string value)
        {
            Console.Out.WriteLine("RPUSH {0} {1}",key,value);
            return redis.RPush(key, value);
        }

        public Dictionary<string, string> HGetAll(string key)
        {
            Console.Out.WriteLine("HGETALL {0}",key);
            return redis.HGetAll(key);
        }

        public void HSet(string key, string field, string value)
        {
            Console.Out.WriteLine("HSET {0} {1} {1}",key,field,value);
            redis.HSet(key, field, value);
        }

        public Tuple<string, string> BLPop(string[] keys, int timeoutSeconds = 0)
        {
            Console.Out.WriteLine("BLPOP {0}", string.Join(" ",keys));
            return redis.BLPop(keys, timeoutSeconds);
        }

        public long Incr(string key)
        {
            Console.Out.WriteLine("INCR {0}", key);
            return redis.Incr(key);
        }

        public LoggingRedisWrapper(IRedis redis)
        {
            this.redis = redis;
        }
    }
}
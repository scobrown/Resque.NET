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
    }
}
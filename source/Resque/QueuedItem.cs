using Newtonsoft.Json;

namespace Resque
{
    public class QueuedItem
    {
        public string @class { get; set; }
        public string[] args { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
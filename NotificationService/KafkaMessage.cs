using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService;

public class KafkaMessage
{
    public long Offset { get; set; }
    public int Partition { get; set; }
    public string Topic { get; set; }
    public DateTime Timestamp { get; set; }
    public string Value { get; set; }
    public string Key { get; set; }
    public List<object> Headers { get; set; }
}

namespace CartApplication.Producer;

public interface IKafkaProducer
{
    Task SendMessageAsync(string topic, string message);
}
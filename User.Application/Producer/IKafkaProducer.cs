
namespace User.Application.Producer;

public interface IKafkaProducer
{
    Task SendMessageAsync(string topic, string message);
}
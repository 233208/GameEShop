using Confluent.Kafka;
using Microsoft.Extensions.Configuration; // Dodaj ten using
using Microsoft.Extensions.Logging;
using User.Application.Producer;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    // Zmieniamy konstruktor, aby przyjmował IConfiguration
    public KafkaProducer(ILogger<KafkaProducer> logger, IConfiguration configuration)
    {
        // Pobieramy adres serwera z konfiguracji, z wartością domyślną
        var bootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers") ?? "kafka:9092";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
        _logger = logger;
    }

    public async Task SendMessageAsync(string topic, string message)
    {
        var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
        _logger.LogInformation($"Wysłano wiadomość: {message} do partycji {result.Partition} z offsetem {result.Offset}");
    }
}
namespace BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQPublisher
{
    //Task Publish<T>(string routingKey, T message);
    Task Publish<T>(Dictionary<string,object> headers, T message);

}

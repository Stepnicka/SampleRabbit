namespace SampleRabbit.Shared.Config
{
    /// <summary>
    ///     Configuration used to connect to Rabbit MQ
    /// </summary>
    public class RabbitMQConfiguration
    {
        /// <summary>
        ///     RabbitMQ host url
        /// </summary>
        public string Host { get; set; } = null!;

        /// <summary>
        ///     RabbitMq virtual host
        /// </summary>
        public string VirtualHost { get; set; } = null!;

        /// <summary>
        ///     RabbitMQ UserName
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        ///     RabbitMq Password
        /// </summary>
        public string Password { get; set; } = null!;
    }
}

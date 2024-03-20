using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MqttPublicsher
{
    public class Publisher
    {
        

        static async Task Main(string[] args)
        {
            string broker = "696f79357b6e448585c862d1c831e4d4.s1.eu.hivemq.cloud";
            int port = 8883;
            string clientId = Guid.NewGuid().ToString();
            string topic = "Csharp/mqtt";
            string username = "hieuhoang";
            string password = "Hieuhoang02";
            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            var mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                .WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .WithTlsOptions(tls =>
                {
                    // Sử dụng SSL/TLS
                    tls.UseTls(true);

                    // Bỏ qua xác nhận chứng chỉ (không an toàn)
                    //tls.AllowUntrustedCertificates(true);
                    tls.WithAllowUntrustedCertificates(true);
                    // Đường dẫn tới file chứng chỉ (nếu cần)
                    //tls.Certificates = new X509CertificateCollection { new X509Certificate("path/to/certificate.pem") };
                })
                .WithCleanSession()
                .Build();

            // Connect to MQTT broker
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected to MQTT broker successfully.");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    return Task.CompletedTask;
                };

                // Publish a message 10 times
                for (int i = 0; i < 1000000000; i++)
                {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload($"Hello, MQTT! Message number {i}")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag()
                        .Build();

                    await mqttClient.PublishAsync(message);
                    await Task.Delay(10000); // Wait for 1 second
                }

                // Unsubscribe and disconnect
                await mqttClient.UnsubscribeAsync(topic);
                await mqttClient.DisconnectAsync();
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }

        }
    }
}

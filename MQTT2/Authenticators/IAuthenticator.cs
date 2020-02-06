namespace MQTT2.Authenticators
{
    internal interface IAuthenticator
    {
        bool Validate(MQTTnet.Server.MqttConnectionValidatorContext context);
    }
}
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MQTT2.Authenticators
{
    internal class HubAuthenticatorService
    {
        private IEnumerable<IAuthenticator> _authenticators;
        private readonly IConfiguration _configuration;

        public HubAuthenticatorService(IConfiguration configuration)
        {
            _configuration = configuration;
            Register();
        }

        private void RegisterAuthenticator(IAuthenticator authenticator)
        {
            (_authenticators as List<IAuthenticator>)?.Add(authenticator);
        }

        private void Register()
        {
            _authenticators = new List<IAuthenticator>();
            RegisterAuthenticator(new AgentAuthenticator(_configuration));
            RegisterAuthenticator(new HubUserAuthenticator(_configuration));
        }

        public void Validate(MQTTnet.Server.MqttConnectionValidatorContext context)
        {
            if (_authenticators.Any(authenticator => authenticator.Validate(context)))
            {
                context.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                return;
            }

            context.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
        }
    }
}
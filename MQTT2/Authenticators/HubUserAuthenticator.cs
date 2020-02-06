using MQTT2.Utilities;
using MQTTnet.Server;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MQTT2.Authenticators
{
    internal class HubUserAuthenticator : IAuthenticator
    {
        private readonly IConfiguration _configuration;

        public HubUserAuthenticator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Validate(MqttConnectionValidatorContext context)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HubDbContext>();
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("HubDatabase"));
            HubUser user;

            using (var dbContext = new HubDbContext(optionsBuilder.Options))
            {
                user = dbContext.HubUser.SingleOrDefault(new Validator(context).Predicate);
            }

            return user != null && "HUR-ACT" == user.StatusCode;
        }

        private class Validator
        {
            private MQTTnet.Server.MqttConnectionValidatorContext context;

            public Validator(MqttConnectionValidatorContext context)
            {
                this.context = context;
            }

            public bool Predicate(HubUser user)
            {
                var hashedPassword = Cryptography.CreateShaHash($"{context.Password}");
                return user != null
                    && context.ClientId == user.ApplicationType
                    && context.Username == user.UserName
                    && hashedPassword == user.Password;
            }
        }
    }
}
using MQTTnet.Server;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MQTT2.Authenticators
{
    public class AgentAuthenticator : IAuthenticator
    {
        private readonly IConfiguration _configuration;

        public AgentAuthenticator(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public bool Validate(MqttConnectionValidatorContext context)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HubDbContext>();
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("HubDatabase"));
            Agent agent;
            using (var dbContext = new HubDbContext(optionsBuilder.Options))
            {
                agent = dbContext.Agent.FirstOrDefault(new Validator(context).Predicate);
            }

            return agent != null;
        }

        private class Validator
        {
            private readonly MqttConnectionValidatorContext _context;

            public Validator(MqttConnectionValidatorContext context)
            {
                _context = context;
            }

            public bool Predicate(Agent agent)
            {
                return agent != null
                       && _context.Username == agent.IdentifyCode
                       && _context.Password == agent.IdentifyCode;
            }
        }
    }
}
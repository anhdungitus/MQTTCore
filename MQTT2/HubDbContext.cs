using Microsoft.EntityFrameworkCore;
using System;

namespace MQTT2
{
    public class HubDbContext : DbContext
    {
        public HubDbContext(DbContextOptions<HubDbContext> options)
            : base(options)
        { }

        public DbSet<WilinkHubLog> WilinkHubLog { get; set; }
        public DbSet<Agent> Agent { get; set; }
        public DbSet<HubUser> HubUser { get; set; }
    }

    public class WilinkHubLog
    {
        public Guid WilinkHubLogId { get; set; }
        public int LogLevel { get; set; }
        public string ClientId { get; set; }
        public string ClientIpAddress { get; set; }
        public string BrokerIpAddress { get; set; }
        public string ShortMessage { get; set; }
        public string FullMessage { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public partial class Agent
    {
        public Guid AgentId { get; set; }

        public Guid? LocationId { get; set; }

        public string IdentifyCode { get; set; }
    }

    public class HubUser
    {
        public Guid HubUserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ApplicationType { get; set; }
        public string Description { get; set; }
        public string StatusCode { get; set; }
    }
}
using Microsoft.EntityFrameworkCore;
using SecureGuard.Server.Data;
using SecureGuard.Server.Models;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Services;

public interface IAgentService
{
    Task<Agent> RegisterOrUpdateAsync(AgentDto dto);
    Task<IEnumerable<Agent>> GetAllAsync();
    Task<Agent?> GetByIdAsync(Guid id);
    Task<Agent?> GetByAgentIdAsync(string agentId);
    Task<Agent?> UpdateConfigAsync(Guid id, AgentConfigDto config);
    Task MarkOfflineAsync(TimeSpan timeout);
}

public class AgentService : IAgentService
{
    private readonly AppDbContext _db;

    public AgentService(AppDbContext db) => _db = db;

    public async Task<Agent> RegisterOrUpdateAsync(AgentDto dto)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == dto.AgentId);
        if (agent == null)
        {
            agent = new Agent { AgentId = dto.AgentId };
            _db.Agents.Add(agent);
        }

        agent.Hostname = dto.Hostname;
        agent.IpAddress = dto.IpAddress;
        agent.Username = dto.Username;
        agent.OsVersion = dto.OsVersion;
        agent.AgentVersion = dto.AgentVersion;
        agent.IsOnline = true;
        agent.LastSeen = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return agent;
    }

    public async Task<IEnumerable<Agent>> GetAllAsync() =>
        await _db.Agents.OrderByDescending(a => a.LastSeen).ToListAsync();

    public async Task<Agent?> GetByIdAsync(Guid id) =>
        await _db.Agents.FindAsync(id);

    public async Task<Agent?> GetByAgentIdAsync(string agentId) =>
        await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == agentId);

    public async Task<Agent?> UpdateConfigAsync(Guid id, AgentConfigDto config)
    {
        var agent = await _db.Agents.FindAsync(id);
        if (agent == null) return null;

        agent.ProxyEnabled = config.ProxyEnabled;
        agent.ProxyPort = config.ProxyPort;
        agent.FileMonitorEnabled = config.FileMonitorEnabled;
        agent.ProcessMonitorEnabled = config.ProcessMonitorEnabled;

        await _db.SaveChangesAsync();
        return agent;
    }

    public async Task MarkOfflineAsync(TimeSpan timeout)
    {
        var cutoff = DateTime.UtcNow - timeout;
        var staleAgents = await _db.Agents
            .Where(a => a.IsOnline && a.LastSeen < cutoff)
            .ToListAsync();

        foreach (var agent in staleAgents)
            agent.IsOnline = false;

        await _db.SaveChangesAsync();
    }
}

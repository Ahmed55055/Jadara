using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Rewards.Common;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Common.Interface;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.Data;

public sealed class SessionRewardEntity: ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public short? Year { get; set; }
    public byte? Term { get; set; }
    public decimal Percentage { get; set; }

    public Reward Reward { get; init; } = null!;

    private SessionRewardEntity()
    {
    }

    private SessionRewardEntity(short? year, byte? term, decimal percentage, Reward reward)
    {
        this.Year = year;
        this.Term = term;
        Percentage = percentage;

        Reward = reward;
    }

    public static SessionRewardEntity Create(short? year, byte? semester, decimal percentage, int CreatedBy,
        string? name = "Untitled", string? code = null)
    {
        var reward = new Reward
        {
            Name = name,
            Total = 0,
            Code = code,
            LastUpdate = DateTime.UtcNow,
            CreatedBy = CreatedBy,
            NumberOfEmployees = 0,
            RewardType = (int)RewardTypes.Sessions
        };

        return new(year, semester, percentage, reward);
    }
}
using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Rewards.Common;
using Reward_Flow_v2.Rewards.Data.Database;
using RewardFlow_API.Common.Interface;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.Data;

public class SessionRewardEntity: ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int? year { get; set; }
    public byte? semester { get; set; }
    public float Percentage { get; set; }

    public virtual RewardEntity Reward { get; set; } = null!;
}


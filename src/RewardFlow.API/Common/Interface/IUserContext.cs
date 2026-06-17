namespace RewardFlow_API.Common.Interface;

public interface IUserContext
{
    Guid Uuid { get; }
    int GetUserId();
    Guid GetTenantId();
}
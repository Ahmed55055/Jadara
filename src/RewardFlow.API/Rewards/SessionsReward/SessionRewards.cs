using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensibility;
using Reward_Flow_v2.Common.EmployeeLookup;
using Reward_Flow_v2.Employees.Data;
using Reward_Flow_v2.Rewards;
using Reward_Flow_v2.Rewards.Common;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Reward_Flow_v2.Rewards.SessionsReward;

public sealed partial class SessionRewards : Reward, ISessionReward
{
    public int SessionRewardId { get => SessionRewardEntity.Id; }
    public int? Year { get => SessionRewardEntity.year; }
    public byte? Semester { get => SessionRewardEntity.semester; }
    public float Percentage { get => SessionRewardEntity.Percentage; }
    public EntityState State {
        get
        {
            if (_sessionEntityState == EntityState.Added || base.State == EntityState.Added)
                return EntityState.Added;
            
            if (_sessionEntityState == EntityState.Modified || base.State == EntityState.Modified)
                return EntityState.Modified;

            return EntityState.Unchanged;
        }
    }
    private EntityState _sessionEntityState => _dbContext.Entry(SessionRewardEntity).State;
    private SessionRewardEntity SessionRewardEntity { get; set; }
    private readonly ISessionRewardCalculator rewardCalculator;
    private readonly ISessionRewardRules rules;
    private readonly IDbContextFactory<RewardDbContext> contextFactory;
    private readonly IEmployeeLookupService employeeLookup;
    private readonly RewardDbContext _dbContext;

    private SessionRewards(IDbContextFactory<RewardDbContext> contextFactory, ISessionRewardCalculator rewardCalculator,
        ISessionRewardRules rules, IEmployeeLookupService employeeLookup, int createdBy)
        : base(contextFactory.CreateDbContext(), createdBy, (int)RewardTypes.Sessions)
    {
        this.contextFactory = contextFactory;
        this.employeeLookup = employeeLookup;
        SessionRewardEntity = new SessionRewardEntity();
        this.rewardCalculator = rewardCalculator;
        this.rules = rules;
        _dbContext = this.contextFactory.CreateDbContext();
        
        _dbContext.Entry(SessionRewardEntity).State = EntityState.Added;
    }

    private SessionRewards(IDbContextFactory<RewardDbContext> contextFactory, SessionRewardEntity sessionRewardEntity,
        ISessionRewardCalculator rewardCalculator, ISessionRewardRules rules, IEmployeeLookupService employeeLookup,
        RewardEntity rewardEntity)
        : base(contextFactory.CreateDbContext(), rewardEntity)
    {
        this.contextFactory = contextFactory;
        this.employeeLookup = employeeLookup;
        SessionRewardEntity = sessionRewardEntity;
        this.rewardCalculator = rewardCalculator;
        this.rules = rules;
        _dbContext = this.contextFactory.CreateDbContext();
        
        _dbContext.Entry(SessionRewardEntity).State = EntityState.Unchanged;
    }

    public override FileStream ToPDF()
    {
        throw new NotImplementedException();
    }

    public override FileStream ToUploadingWorkbook()
    {
        throw new NotImplementedException();
    }

    public override async Task UpdateTotal()
    {
        using var context = contextFactory.CreateDbContext();

        var employeeSessionData = await GetEmployeesTotalSessionsAsync(context);

        var employeeIds = employeeSessionData.Select(e => e.EmployeeId);
        var employeeSalaries = await GetEmployeesSalariesAsync(employeeIds);


        var employeeRewards = await context.EmployeeReward
            .Where(er => er.RewardId == this.RewardId && !er.IsUpdated)
            .ToListAsync();

        foreach (var empData in employeeSessionData)
        {
            employeeSalaries.TryGetValue(empData.EmployeeId, out float salary);

            var allowedSessions = GetAllowedSessions(empData.TotalSessions);
            var total = rewardCalculator.CalculateTotal(allowedSessions, salary, this.Percentage);

            UpdateOrCreateEmployeeReward(context, employeeRewards, empData, total);
        }

        await context.SaveChangesAsync();
    }

    private async Task<bool> Save()
    {
        try
        {
            if (!await base.SaveAsync())
                return false;

            if (_dbContext.Entry(SessionRewardEntity).State == EntityState.Added)
                SessionRewardEntity.Id  = Entity.Id;

            return await _dbContext.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> AssignEmployeeHandler(SessionSubjectDto dto, RewardDbContext context)
    {
        var EmployeesIds = dto.Employees.Select(e => e.EmployeeId).ToList();

        if (!rules.CanAssignEmployeeToSubjectAsync(dto.SubjectId, dto.NumberOfStudents, EmployeesIds))
            return false;

        var subjectSessionReward = await GetOrCreateSubjectSessionReward(dto, context);
        subjectSessionReward.AddEmployees(EmployeesIds);
        await MarkEmployeeRewardsAsOutdated(EmployeesIds, context);

        return true;
    }

    public async Task<bool> AssignEmployeeToSubjectAsync(SessionSubjectDto dto)
    {
        var attempts = 0;
        while (attempts <= 1)
        {
            try
            {
                using var context = contextFactory.CreateDbContext();

                if (!await AssignEmployeeHandler(dto, context))
                    return false;

                return await context.SaveChangesAsync() > 0;
            }
            // WHT RETRY?
            // Single session subject could have multiple employees,
            // so it could be sent with multiple requests for the same session subject but with diffrant employees to add at the same time.
            // that's why the first catch is as safe net if the session subject is added before ending proccing of the second request
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                attempts++;
                continue;
            }
            catch
            {
                // Uhhhh i'm very sorry for future myself for this code, but i don't have time to add logs now
                // Please future me don't swear me!
                return false;
            }
        }

        return false;
    }

    public async Task<bool> AssignEmployeeToSubjectAsync(IEnumerable<SessionSubjectDto> dtos)
    {
        var attempts = 0;
        while (attempts <= 1)
        {
            try
            {
                using var context = contextFactory.CreateDbContext();

                // This code introduce overhead to the multiple round trips it takes and multiple queries.
                // but it works for now to keep code as simple as possible for MVP and the tiny user base targeted any way
                foreach (var dto in dtos)
                {
                    await AssignEmployeeHandler(dto, context);
                }

                return await context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                attempts++;
                continue;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private async Task<SubjectSessionReward> GetOrCreateSubjectSessionReward(SessionSubjectDto dto,
        RewardDbContext context)
    {
        var SSR = await SubjectSessionReward.FindByRewardAndSubject(dto.SubjectId, this.Semester.Value, this.RewardId,
            context);

        if (SSR is not null)
            return SSR;

        var subjectSessionReward = new SubjectSessionReward(
            this.RewardId,
            (byte)rewardCalculator.CalculateSessions(dto.NumberOfStudents),
            dto.SubjectId,
            dto.NumberOfStudents,
            dto.Employees.First().EmployeeId,
            context
        );

        return subjectSessionReward;
    }

    private bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException?.Message is not string message)
            return false;

        message = message.ToLowerInvariant();

        return message.Contains("unique constraint") ||
               message.Contains("unique index") ||
               message.Contains("duplicate key") ||
               message.Contains("duplicate entry");
    }

    public record EmployeeSessionData(int EmployeeId, int TotalSessions);

    void UpdateOrCreateEmployeeReward(RewardDbContext context, List<EmployeeReward> employeeRewards,
        EmployeeSessionData empData, float total)
    {
        var empReward = employeeRewards.FirstOrDefault(er => er.EmployeeId == empData.EmployeeId);
        if (empReward == null)
        {
            empReward = new EmployeeReward
            {
                RewardId = this.RewardId, EmployeeId = empData.EmployeeId, Total = total, IsUpdated = true
            };
            context.EmployeeReward.Add(empReward);
        }
        else
        {
            empReward.Total = total;
            empReward.IsUpdated = true;
        }
    }

    async Task<IEnumerable<EmployeeSessionData>> GetEmployeesTotalSessionsAsync(RewardDbContext context)
    {
        return await (
            from empSession in context.EmployeeSessionRewardEntity
            join subjectSession in context.SubjectSessionRewardEntity on empSession.SubjectSessionRewardId equals
                subjectSession.Id
            join empReward in context.EmployeeReward on empSession.EmployeeSnapshotId equals empReward.EmployeeId into
                empRewardGroup
            from empReward in empRewardGroup.DefaultIfEmpty()
            where subjectSession.SessionRewardId == this.RewardId && !empReward.IsUpdated
            group new { EmployeeId = empSession.EmployeeSnapshotId, subjectSession.NumberOfSessions } by empSession.EmployeeSnapshotId
            into g
            select new EmployeeSessionData
            (
                g.Key,
                g.Sum(x => x.NumberOfSessions)
            )
        ).ToListAsync();
    }

    async Task<Dictionary<int, float>> GetEmployeesSalariesAsync(IEnumerable<int> employeeIds)
    {
        return (await
                employeeLookup.GetEmployeesSalaryById(employeeIds))
            .ToDictionary(e => e.EmployeeId, e => e.Salary);
    }

    private int GetAllowedSessions(int totalSessions)
    {
        return rules.IsWithInMaximumNumberOfSession(totalSessions)
            ? totalSessions
            : rules.MaxSessionsNumber;
    }

    private async Task MarkEmployeeRewardsAsOutdated(IEnumerable<int> employeeIds, RewardDbContext context)
    {
        var employeeRewards = await context.EmployeeReward
            .Where(er => er.RewardId == this.RewardId && employeeIds.Contains(er.EmployeeId))
            .ToListAsync();

        foreach (var empReward in employeeRewards)
        {
            empReward.IsUpdated = false;
        }
    }

    public async Task<EmployeeRewardDto?> GetEmployeeReward(int employeeId)
    {
        using var context = contextFactory.CreateDbContext();
        var employeeReward = await context.EmployeeReward
            .Where(er => er.RewardId == this.RewardId && er.EmployeeId == employeeId)
            .FirstOrDefaultAsync();

        return employeeReward is not null
            ? new EmployeeRewardDto(employeeReward.Id, employeeReward.EmployeeId, employeeReward.Total)
            : null;
    }

    public async Task<IEnumerable<EmployeeRewardDto>> GetEmployeesReward()
    {
        using var context = contextFactory.CreateDbContext();
        return await context.EmployeeReward
            .Where(er => er.RewardId == this.RewardId)
            .Select(er => new EmployeeRewardDto(er.Id, er.EmployeeId, er.Total))
            .ToListAsync();
    }

    public async Task<float> GetRewardTotal()
    {
        using var context = contextFactory.CreateDbContext();
        return await context.EmployeeReward
            .Where(er => er.RewardId == this.RewardId)
            .SumAsync(er => er.Total);
    }
}
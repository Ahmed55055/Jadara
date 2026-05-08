using Microsoft.EntityFrameworkCore;
using Reward_Flow_v2.Common.EmployeeLookup;
using Reward_Flow_v2.Rewards.Common;
using Reward_Flow_v2.Rewards.Data;
using Reward_Flow_v2.Rewards.Data.Database;
using Reward_Flow_v2.Rewards.SessionsReward.Common;
using Reward_Flow_v2.Rewards.SessionsReward.Dtos;
using Reward_Flow_v2.Rewards.SessionsReward.Interface;

namespace Reward_Flow_v2.Rewards.SessionsReward;

public class SessionReward_v2 : ISessionReward, IExportable
{
    private SessionRewardEntity SessionRewardEntity { get; set; }
    private readonly ISessionRewardCalculator rewardCalculator;
    private readonly ISessionRewardRules rules;
    private readonly IDbContextFactory<RewardDbContext> contextFactory;
    private readonly IEmployeeLookupService employeeLookup;

    private SessionReward_v2(IDbContextFactory<RewardDbContext> contextFactory, ISessionRewardCalculator rewardCalculator,
        ISessionRewardRules rules, IEmployeeLookupService employeeLookup, int createdBy)
    {
        this._contextFactory = contextFactory;
        this._employeeLookup = employeeLookup;
        SessionRewardEntity = new SessionRewardEntity();
        this.rewardCalculator = rewardCalculator;
        this.rules = rules; 
    }

    private SessionReward_v2(IDbContextFactory<RewardDbContext> contextFactory, SessionRewardEntity sessionRewardEntity,
        ISessionRewardCalculator rewardCalculator, ISessionRewardRules rules, IEmployeeLookupService employeeLookup,
        RewardEntity rewardEntity)
    {
        this._contextFactory = contextFactory;
        this._employeeLookup = employeeLookup;
        SessionRewardEntity = sessionRewardEntity;
        SessionRewardEntity.Reward = rewardEntity;
        this.rewardCalculator = rewardCalculator;
        this.rules = rules;
    }

    async Task<IEnumerable<SessionRewards.EmployeeSessionData>> GetEmployeesTotalSessionsAsync(RewardDbContext context)
    {
        return await (
            from empSession in context.EmployeeSessionRewardEntity
            join subjectSession in context.SubjectSessionRewardEntity on empSession.SubjectSessionRewardId equals
                subjectSession.Id
            join empReward in context.EmployeeReward on empSession.EmployeeSnapshotId equals empReward.EmployeeId into
                empRewardGroup
            from empReward in empRewardGroup.DefaultIfEmpty()
            where subjectSession.SessionRewardId == this.SessionRewardEntity.Id && !empReward.IsUpdated
            group new { EmployeeId = empSession.EmployeeSnapshotId, subjectSession.NumberOfSessions } by empSession.EmployeeSnapshotId
            into g
            select new SessionRewards.EmployeeSessionData
            (
                g.Key,
                g.Sum(x => x.NumberOfSessions)
            )
        ).ToListAsync();
    }
    
    async Task<Dictionary<int, float>> GetEmployeesSalariesAsync(IEnumerable<int> employeeIds)
    {
        return (await employeeLookup.GetEmployeesSalaryById(employeeIds))
            .ToDictionary(e => e.EmployeeId, e => e.Salary);
    }
    
    private int GetAllowedSessions(int totalSessions)
    {
        return rules.IsWithInMaximumNumberOfSession(totalSessions)
            ? totalSessions : rules.MaxSessionsNumber;
    }
    
    void UpdateOrCreateEmployeeReward(RewardDbContext context, List<EmployeeReward> employeeRewards,
        SessionRewards.EmployeeSessionData empData, float total)
    {
        var empReward = employeeRewards.FirstOrDefault(er => er.EmployeeId == empData.EmployeeId);
        if (empReward == null)
        {
            empReward = new EmployeeReward
            {
                RewardId = this.SessionRewardEntity.Id, EmployeeId = empData.EmployeeId, Total = total, IsUpdated = true
            };
            context.EmployeeReward.Add(empReward);
        }
        else
        {
            empReward.Total = total;
            empReward.IsUpdated = true;
        }
    }
    public async Task Calculate()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var employeeSessionData = await GetEmployeesTotalSessionsAsync(context);

        var employeeIds = employeeSessionData.Select(e => e.EmployeeId);
        var employeeSalaries = await GetEmployeesSalariesAsync(employeeIds);


        var employeeRewards = await context.EmployeeReward
            .Where(er => er.RewardId == this.SessionRewardEntity.Id && !er.IsUpdated)
            .ToListAsync();

        foreach (var empData in employeeSessionData)
        {
            employeeSalaries.TryGetValue(empData.EmployeeId, out float salary);

            var allowedSessions = GetAllowedSessions(empData.TotalSessions);
            var total = rewardCalculator.CalculateTotal(allowedSessions, salary, this.SessionRewardEntity.Percentage);

            // This is not n+1, it just creates or adds the entity to the dbcontext
            // database hit just once by save changes async
            UpdateOrCreateEmployeeReward(context, employeeRewards, empData, total);
        }

        await context.SaveChangesAsync();
    }

    public async Task<float> GetTotal()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.EmployeeReward
            .Where(er => er.RewardId == this.SessionRewardEntity.Id )
            .SumAsync(er => er.Total);
    }

    public bool IsComplete()
    {
        throw new NotImplementedException();
    }

    public bool IsClosed()
    {
        throw new NotImplementedException();
    }

    public bool Delete()
    {
        throw new NotImplementedException();
    }

private async Task<bool> AssignEmployeesBulkHandler(IEnumerable<SessionSubjectDto> dtos, RewardDbContext context)
{
    var snapshotService = new EmployeeSnapshotService(context);

    var allEmployeeIds = dtos.SelectMany(d => d.Employees.Select(e => e.EmployeeId)).Distinct().ToList();
    var allSubjectIds = dtos.Select(d => d.SubjectId).Distinct().ToList();

    var employees = await employeeLookup.GetEmployees(allEmployeeIds); 
    
    var snapshotMap = (await snapshotService.EnsureLatest(employees)).ToDictionary(s => s.EmployeeId);

    var exsitingSubjectSessionRewards = await GetExsitingSubjectSessionRewards(context, allSubjectIds);

    foreach (var subjectSessionDto in dtos)
    {
        var subjectEmployeesSnapshots =
            subjectSessionDto.Employees.Select(e => e.EmployeeId)
                .Select(id => snapshotMap.TryGetValue(id, out var snapshot) ? snapshot : null)
                .Where(s => s != null)
                .ToList();
            
        EnsureSubjectSessionReward(context);
    }
    foreach (var dto in dtos)
    {
        var employeeIdsForThisDto = dto.Employees.Select(e => e.EmployeeId).ToList();

        // 2. Rule Validation
        if (!await rules.CanAssignEmployeeToSubjectAsync(dto.SubjectId, dto.NumberOfStudents, employeeIdsForThisDto))
        {
            continue; 
        }

        // 3. Get or Create using the Domain Wrapper from our pre-fetched dictionary
        if (!exsitingSubjectSessionRewards.TryGetValue(dto.SubjectId, out var ssr))
        {
            ssr = new SubjectSessionReward(
                this.SessionRewardEntity.Id,
                (byte)rewardCalculator.CalculateSessions(dto.NumberOfStudents),
                dto.SubjectId,
                dto.NumberOfStudents,
                employeeIdsForThisDto.First(), 
                context
            );
            // Add to dictionary so if the same subject appears twice in 'dtos', we don't create it twice
            exsitingSubjectSessionRewards[dto.SubjectId] = ssr;
        }

        // 4. Link employees using the snapshot data
        foreach (var empId in employeeIdsForThisDto)
        {
            if (snapshotMap.TryGetValue(empId, out var snapshot))
            {
                // We use the ID from the snapshot to ensure point-in-time accuracy
                // Note: Ensure your AddEmployee logic handles the SnapshotId correctly
                ssr.AddEmployee(snapshot.EmployeeId); 
            }
        }
    }

    // 5. Finalize Bulk Operation
    await MarkEmployeeRewardsAsOutdated(allEmployeeIds, context);
    await context.SaveChangesAsync();

    return true;
}

private void EnsureSubjectSessionReward(List<EmployeeSnapshot?> subjectEmployeesSnapshots, RewardDbContext context)
{
    throw new NotImplementedException();
}

private async Task<Dictionary<int, SubjectSessionRewardEntity>> GetExsitingSubjectSessionRewards(RewardDbContext context, List<int> allSubjectIds)
    {
        var existingRewards = await context.SubjectSessionRewardEntity
            .Include(ssr => ssr.Employees) 
            .Where(ssr => allSubjectIds.Contains(ssr.SemesterSubjectId) && 
                          ssr.SessionRewardId == this.SessionRewardEntity.Id)
            .ToDictionaryAsync(ssr => ssr.SemesterSubjectId);
        return existingRewards;
    }

    private async Task MarkEmployeeRewardsAsOutdated(IEnumerable<int> employeeIds, RewardDbContext context)
    {
        // Fetch all relevant rewards in one round trip
        var employeeRewards = await context.EmployeeReward
            .Where(er => er.RewardId == this.RewardId && employeeIds.Contains(er.EmployeeId))
            .ToListAsync();

        foreach (var empReward in employeeRewards)
        {
            empReward.IsUpdated = false;
        }
    }
    
    public async Task<bool> AssignEmployeesAsync(IEnumerable<SessionSubjectDto> dtos, int attempts = 3)
    {
        while (attempts <= 1)
        {
            try
            {
                using var context = contextFactory.CreateDbContext();
                
                foreach (var dto in dtos)
                {
                    await AssignEmployeeHandler(dto, context);
                }

                await context.SaveChangesAsync();
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

    public Task<EmployeeRewardDto?> GetEmployeeReward(int employeeId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EmployeeRewardDto>> GetEmployeesRewards()
    {
        throw new NotImplementedException();
    }

    public Task<FileStream> ExportPdf()
    {
        throw new NotImplementedException();
    }

    public Task<FileStream> ExportWorkbook()
    {
        throw new NotImplementedException();
    }
}

public interface IExportable
{
    Task<FileStream> ExportPdf();
    Task<FileStream> ExportWorkbook();
}
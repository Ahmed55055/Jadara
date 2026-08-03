using System.Linq.Expressions;
using Bogus;
using RewardFlow_API.Rewards.Data;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Courses;
using RewardFlow.TestUtilities.DataGenerators.Fakers.Employees;

public class CourseFaker : Faker<Course>, IEntityFaker<Course, CourseFields>
{
    private static readonly (string Code, string Name)[] Courses =
    [
        ("CS101", "مقدمة في البرمجة"),
        ("CS201", "هياكل البيانات"),
        ("CS301", "قواعد البيانات"),
        ("CS302", "هندسة البرمجيات"),
        ("CS303", "شبكات الحاسب"),
        ("CS304", "نظم التشغيل"),

        ("MTH101", "التفاضل والتكامل"),
        ("MTH201", "الجبر الخطي"),
        ("MTH301", "الإحصاء"),

        ("ACC101", "مبادئ المحاسبة"),
        ("ACC201", "المحاسبة المالية"),

        ("BUS101", "إدارة الأعمال"),
        ("BUS201", "التسويق"),

        ("EDU101", "أصول التربية"),
        ("EDU201", "علم النفس التربوي"),
        ("EDU301", "تكنولوجيا التعليم"),

        ("LAN101", "اللغة العربية"),
        ("LAN102", "اللغة الإنجليزية"),

        ("PHY101", "الفيزياء العامة"),
        ("CHE101", "الكيمياء العامة"),
        ("BIO101", "الأحياء العامة"),

        ("LAW101", "القانون المدني"),
        ("LAW201", "القانون التجاري"),

        ("GEN101", "حقوق الإنسان"),
        ("GEN201", "ريادة الأعمال"),
        ("GEN301", "الأخلاقيات المهنية")
    ];

    public CourseFaker()
    {
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        RuleFor(c => c.Name, (f, c) =>
        {
            var course = f.PickRandom(Courses);

            c.Code = f.Random.Bool(0.9f)
                ? course.Code
                : null;

            return course.Name;
        });

        RuleFor(c => c.IsTheoretical,
            f => f.Random.Bool(0.8f));

        RuleFor(c => c.IsPractical,
            f => f.Random.Bool(0.6f));

        RuleFor(c => c.SubjectPrice,
            f => Math.Round(f.Random.Decimal(50m, 5000m), 2));
    }

    /// <summary>
    /// Overwrites existing rules to force specific fields to NULL/default values.
    /// </summary>
    public IEntityFaker<Course, CourseFields> WithNulls(CourseFields fields)
    {
        if (fields.HasFlag(CourseFields.Name))
            RuleFor(c => c.Name, _ => null!);

        if (fields.HasFlag(CourseFields.Code))
            RuleFor(c => c.Code, _ => null);

        if (fields.HasFlag(CourseFields.IsTheoretical))
            RuleFor(c => c.IsTheoretical, _ => default);

        if (fields.HasFlag(CourseFields.IsPractical))
            RuleFor(c => c.IsPractical, _ => default);

        if (fields.HasFlag(CourseFields.SubjectPrice))
            RuleFor(c => c.SubjectPrice, _ => default);

        return this;
    }

    /// <summary>
    /// Helper to force a property to a specific value.
    /// </summary>
    public IEntityFaker<Course, CourseFields> ForProperty<TProperty>(
        Expression<Func<Course, TProperty>> property,
        TProperty value)
    {
        RuleFor(property, _ => value);
        return this;
    }

    /// <summary>
    /// Ensures specified fields receive valid values.
    /// </summary>
    public IEntityFaker<Course, CourseFields> WithValue(CourseFields fields)
    {
        if (fields.HasFlag(CourseFields.Name))
        {
            RuleFor(c => c.Name, (f, c) =>
            {
                var course = f.PickRandom(Courses);

                if (string.IsNullOrWhiteSpace(c.Code))
                    c.Code = course.Code;

                return course.Name;
            });
        }

        if (fields.HasFlag(CourseFields.Code))
        {
            RuleFor(c => c.Code, f => f.PickRandom(Courses).Code);
        }

        if (fields.HasFlag(CourseFields.IsTheoretical))
            RuleFor(c => c.IsTheoretical, f => f.Random.Bool());

        if (fields.HasFlag(CourseFields.IsPractical))
            RuleFor(c => c.IsPractical, f => f.Random.Bool());

        if (fields.HasFlag(CourseFields.SubjectPrice))
            RuleFor(c => c.SubjectPrice, f => f.Random.Decimal(50m, 5000m));

        return this;
    }
}
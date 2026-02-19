using System.Linq.Expressions;

namespace RewardFlow.TestUtilities.Extentions;

public static class ExpressionExtentions
{
    public static string GetPropertyName<TEntity,TProperty>(this Expression<Func<TEntity, TProperty>> property) =>
        property.Body switch
        {
            UnaryExpression { Operand: MemberExpression unaryMemberExpression } => unaryMemberExpression.Member.Name,
            MemberExpression member => member.Member.Name,
            _ => throw new ArgumentException("Expression must be a simple property access.")
        };
}
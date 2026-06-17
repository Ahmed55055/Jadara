using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;
using System.Linq.Expressions;

namespace Reward_Flow_v2.Common.Extentions;

public static class ModelBuilderExtensions
{
    public static IEnumerable<EntityTypeBuilder> GetEntityBuilders<TBase>(this ModelBuilder modelBuilder)
        where TBase : class
    {
        return modelBuilder.Model.GetEntityTypes()
            .Where(et => typeof(TBase).IsAssignableFrom(et.ClrType))
            .Select(et => modelBuilder.Entity(et.ClrType));
    }

    public static EntityTypeBuilder HasQueryFilter<TBase>(this EntityTypeBuilder entityTypeBuilder,
        Expression<Func<TBase, bool>>? filter) where TBase : class
    {
        var entitytype = entityTypeBuilder.Metadata.ClrType;
        var newLambda = RewriteLambda(filter, entitytype);
        return entityTypeBuilder.HasQueryFilter(newLambda);
    }

    public static PropertyBuilder Property<TBase, TProperty>(
        this EntityTypeBuilder builder,
        Expression<Func<TBase, TProperty>> propertyExpression) where TBase : class
    {
        var propertyName = GetPropertyName(propertyExpression);
        return builder.Property<TProperty>(propertyName);
    }
    
    public static IndexBuilder HasIndex<TBase>(
        this EntityTypeBuilder builder,
        Expression<Func<TBase, object>> indexExpression) where TBase : class
    {
        var propertyNames = GetPropertyNames(indexExpression);
        return builder.HasIndex(propertyNames);
    }
    
    private static LambdaExpression RewriteLambda(LambdaExpression expression, Type targetType)
    {
        var newParameter = Expression.Parameter(targetType, expression.Parameters[0].Name);
        var visitor = new ParameterReplacerVisitor(expression.Parameters[0], newParameter);
        var newBody = visitor.Visit(expression.Body);
        return Expression.Lambda(newBody, newParameter);
    }
    
    private static string GetPropertyName(LambdaExpression expression)
    {
        Expression body = expression.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            body = unary.Operand;
        }
        return (body as MemberExpression)?.Member.Name 
               ?? throw new ArgumentException($"Expression '{expression}' does not refer to a property.");
    }
    
    private static string[] GetPropertyNames(LambdaExpression expression)
    {
        if (expression.Body is NewExpression newExpression)
        {
            return newExpression.Arguments
                .Select(arg => GetPropertyName(Expression.Lambda(arg, expression.Parameters)))
                .ToArray();
        }
        return new[] { GetPropertyName(expression) };
    }
}

internal class ParameterReplacerVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter; // e.g., (ISoftDeletable IsDeleted)
    private readonly ParameterExpression _newParameter; // e.g., (Blog IsDeleted)

    public ParameterReplacerVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    // This method is called automatically for every "Parameter" node in the tree
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node == _oldParameter)
            return _newParameter;

        return base.VisitParameter(node);
    }
}
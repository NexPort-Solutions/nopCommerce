using System.Reflection;
using AutoMapper;
using AutoMapper.Internal;
using AutoMapper.Configuration;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

/// <summary>
/// Workaround for AutoMapper removal of ForAllOtherMembers
/// </summary>
public static class AutoMapperExtensions
{
    private static readonly PropertyInfo s_typeMapActionsProperty = typeof(TypeMapConfiguration).GetProperty("TypeMapActions", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidProgramException();

    // not needed in AutoMapper 12.0.1
    private static readonly PropertyInfo s_destinationTypeDetailsProperty = typeof(TypeMap).GetProperty("DestinationTypeDetails", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidProgramException();

    public static void ForAllOtherMembers<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> expression,
        Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
    {
        if (expression is not TypeMapConfiguration typeMapConfiguration)
        {
            return;
        }
        var typeMapActions = s_typeMapActionsProperty.GetValue(typeMapConfiguration) as List<Action<TypeMap>>;
        typeMapActions?.Add(typeMap =>
        {
            if (s_destinationTypeDetailsProperty.GetValue(typeMap) is not TypeDetails destinationTypeDetails)
            {
                return;
            }
            foreach (var accessor in destinationTypeDetails.WriteAccessors.Where(memberInfo =>
                typeMapConfiguration.GetDestinationMemberConfiguration(memberInfo) is null))
            {
                expression.ForMember(accessor.Name, memberOptions);
            }
        });
    }
}

using System.Reflection;
using AutoMapper;

namespace LpgErp.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);
        var mapToType = typeof(IMapTo<>);

        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == mapFromType || i.GetGenericTypeDefinition() == mapToType)))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);

            var mapFromInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType);
            if (mapFromInterface != null)
            {
                var methodInfo = type.GetMethod("Mapping")
                    ?? mapFromInterface.GetMethod("Mapping");
                methodInfo?.Invoke(instance, [this]);
            }

            var mapToInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapToType);
            if (mapToInterface != null)
            {
                var methodInfo = type.GetMethod("Mapping")
                    ?? mapToInterface.GetMethod("Mapping");
                methodInfo?.Invoke(instance, [this]);
            }
        }
    }
}

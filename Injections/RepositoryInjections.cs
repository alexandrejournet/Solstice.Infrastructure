using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Radiant.NET.Domain.Exceptions;
using Radiant.NET.Repository.Attributes;
using System.Reflection;

namespace Radiant.NET.Repository.Injections
{
    /// <summary>
    /// The RepositoryInjections static class contains extension methods for ModelBuilder and IServiceCollection instances.
    /// These extension methods add convenient functionality for database and service-related configurations.
    /// </summary>
    public static class RepositoryInjections
    {
        /// <summary>
        /// This extension method for the ModelBuilder class scans all assemblies in the current domain for types marked with the QueryAttribute.
        /// Each located type is then added to the ModelBuilder as an entity, with no key, and set to be excluded from migrations.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance to which the types are added.</param>
        /// <exception cref="CoreException">Thrown when no types with the QueryAttribute are found.</exception>
        public static void AddQueryDtoToDbContext(this ModelBuilder modelBuilder)
        {
            var typesWithMyAttribute =
                from assembly in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                from type in assembly.GetTypes()
                let attributes = type.GetCustomAttributes(typeof(QueryAttribute), true)
                where attributes is { Length: > 0 }
                select type;

            if (!typesWithMyAttribute.Any())
            {
                throw CoreException.Format(CoreExceptionEnum.NO_QUERY_DTO);
            }

            foreach (var type in typesWithMyAttribute)
            {
                modelBuilder.Entity(type).HasNoKey().ToTable(t => t.ExcludeFromMigrations());
            }
        }

        /// <summary>
        /// This extension method to the ModelBuilder class allows the application to apply database configurations from a specified assembly.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance on which to apply the assembly configurations.</param>
        /// <param name="assembly">The assembly from which configurations should be applied.</param>
        /// <remarks>
        /// The method loads the specified assembly and applies its configurations to the dbContext via the ModelBuilder ApplyConfigurationsFromAssembly method.
        /// </remarks>
        public static void ApplyDbConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }


        /// <summary>
        /// This extension method to the ModelBuilder class allows the application to apply database configurations from a specified assembly.
        /// </summary>
        /// <param name="modelBuilder">The ModelBuilder instance on which to apply the assembly configurations.</param>
        /// <param name="assemblyName">The name of the assembly from which configurations should be applied.</param>
        /// <remarks>
        /// The method loads the specified assembly and applies its configurations to the dbContext via the ModelBuilder ApplyConfigurationsFromAssembly method.
        /// </remarks>
        public static void ApplyDbConfigurations(this ModelBuilder modelBuilder, string assemblyName)
        {
            Assembly assembly = Assembly.Load(assemblyName);

            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        [Obsolete(
            "AddRepositories with namespace is deprecated, please use AddRepositories with Repository Attribute instead.")]
        public static void AddRepositories<TDbContext>(this IServiceCollection services, string assemblyName)
        where TDbContext : DbContext
        {
            AddUnitOfWork<TDbContext>(services);
            Assembly assembly = Assembly.Load(assemblyName);
            assembly.GetTypes().Where(t => $"{assembly.GetName().Name}.Repository" == t.Namespace
                                           && !t.IsAbstract
                                           && !t.IsInterface
                                           && t.Name.EndsWith("Repository"))
                .Select(a => new { assignedType = a })
                .ToList()
                .ForEach(typesToRegister => { services.AddScoped(typesToRegister.assignedType); });
        }

        /// <summary>
        /// The AddRepositories extension method for the IServiceCollection, which scans all assemblies in the current domain for types marked with the RepositoryAttribute.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to which the types are added.</param>
        /// <exception cref="CoreException">Thrown when no types with the RepositoryAttribute are found.</exception>
        /// <remarks>
        /// The located type(s) that have the RepositoryAttribute are added to the IServiceCollection as a Scope.
        /// If no such types are found, it throws a CoreInjectionsException.
        /// </remarks>
        public static void AddRepositories<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
        {

            AddUnitOfWork<TDbContext>(services);

            var typesWithMyAttribute =
                from assembly in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                from type in assembly.GetTypes()
                let attributes = type.GetCustomAttributes(typeof(RepositoryAttribute), true)
                where attributes is { Length: > 0 }
                select type;

            var withMyAttribute = typesWithMyAttribute.ToList();
            if (withMyAttribute.Count == 0)
            {
                throw CoreException.Format(CoreExceptionEnum.NO_REPOSITORY);
            }

            foreach (var type in withMyAttribute)
            {
                services.AddScoped(type);
            }
        }

        /// <summary>
        /// The AddRepositories extension method for the IServiceCollection, which scans all assemblies in the current domain for types marked with the RepositoryAttribute.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to which the types are added.</param>
        /// <param name="assembly">The assembly to scan</param>
        /// <exception cref="CoreException">Thrown when no types with the RepositoryAttribute are found.</exception>
        /// <remarks>
        /// The located type(s) that have the RepositoryAttribute are added to the IServiceCollection as a Scope.
        /// If no such types are found, it throws a CoreInjectionsException.
        /// </remarks>
        public static void ScanRepositoriesIn<TDbContext>(this IServiceCollection services, Assembly assembly)
        where TDbContext : DbContext
        {
            AddUnitOfWork<TDbContext>(services);

            IEnumerable<Type> typesWithMyAttribute =
                from type in assembly.GetTypes()
                let attributes = type.GetCustomAttributes(typeof(RepositoryAttribute), true)
                where attributes is { Length: > 0 }
                select type;

            var withMyAttribute = typesWithMyAttribute.ToList();
            if (withMyAttribute.Count == 0)
            {
                throw CoreException.Format(CoreExceptionEnum.NO_REPOSITORY);
            }

            foreach (var type in withMyAttribute)
            {
                services.AddScoped(type);
            }
        }
        
        /// <summary>
        /// The AddRepositories extension method for the IServiceCollection, which scans all assemblies in the current domain for types marked with the RepositoryAttribute.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to which the types are added.</param>
        /// <param name="assemblyName">The name of the assembly to scan</param>
        /// <exception cref="CoreException">Thrown when no types with the RepositoryAttribute are found.</exception>
        /// <remarks>
        /// The located type(s) that have the RepositoryAttribute are added to the IServiceCollection as a Scope.
        /// If no such types are found, it throws a CoreInjectionsException.
        /// </remarks>
        public static void ScanRepositoriesIn<TDbContext>(this IServiceCollection services, string assemblyName)
            where TDbContext : DbContext
        {
            AddUnitOfWork<TDbContext>(services);
            
            var assembly = Assembly.Load(assemblyName);

            var typesWithMyAttribute =
                from type in assembly.GetTypes()
                let attributes = type.GetCustomAttributes(typeof(RepositoryAttribute), true)
                where attributes is { Length: > 0 }
                select type;

            var withMyAttribute = typesWithMyAttribute.ToList();
            if (withMyAttribute.Count == 0)
            {
                throw CoreException.Format(CoreExceptionEnum.NO_REPOSITORY);
            }

            foreach (var type in withMyAttribute)
            {
                services.AddScoped(type);
            }
        }

        /// <summary>
        /// The AddUnitOfWork extension method for the IServiceCollection, which adds the UnitOfWork as a Scoped service.
        /// </summary>
        /// <param name="services"></param>
        public static void AddUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();
        }
    }
}
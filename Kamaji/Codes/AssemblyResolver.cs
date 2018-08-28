namespace Kamaji
{
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.DependencyModel.Resolution;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;

    public sealed class AssemblyResolver
    {
        private readonly ICompilationAssemblyResolver _assemblyResolver;
       

        public AssemblyResolver(string path)
        {
            this._assembly = new Lazy<Assembly>(() => AssemblyLoadContext.Default.LoadFromAssemblyPath(path), true);

            this._dependencyContext = new Lazy<DependencyContext>(() => DependencyContext.Load(this.Assembly), true);

            this._assemblyResolver = new CompositeCompilationAssemblyResolver
                                    (new ICompilationAssemblyResolver[]
            {
            new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
            new ReferenceAssemblyPathResolver(),
            new PackageCompilationAssemblyResolver()
            });

            this._loadContext = new Lazy<AssemblyLoadContext>(() => AssemblyLoadContext.GetLoadContext(this.Assembly), true);
        }


        private readonly Lazy<AssemblyLoadContext> _loadContext;
        private AssemblyLoadContext LoadContext => this._loadContext.Value;


        private readonly Lazy<DependencyContext> _dependencyContext;
        private DependencyContext DependencyContext => this._dependencyContext.Value;

        private readonly Lazy<Assembly> _assembly;
        public Assembly Assembly => this._assembly.Value;



        public T Resolve<T>()
            where T : class
        {
            IEnumerable<RuntimeLibrary> libraries = this.DependencyContext.RuntimeLibraries;
            if (libraries != null)
            {
                foreach (RuntimeLibrary library in libraries)
                {
                    var wrapper = new CompilationLibrary(
                    library.Type,
                    library.Name,
                    library.Version,
                    library.Hash,
                    library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                    library.Dependencies,
                    library.Serviceable);

                    var assemblies = new List<string>();
                    this._assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
                    if (assemblies.Count > 0)
                    {
                        foreach (string path in assemblies)
                        {
                            this.LoadContext.LoadFromAssemblyPath(path);
                            Assembly.UnsafeLoadFrom(path);
                        }
                    }
                }
            }

            return (T)Activator.CreateInstance(this.Assembly.GetTypes().FirstOrDefault(p => typeof(T).IsAssignableFrom(p)));
        }
    }
}

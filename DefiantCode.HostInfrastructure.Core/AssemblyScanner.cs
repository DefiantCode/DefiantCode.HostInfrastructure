using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DefiantCode.HostInfratructure.Core
{
    public class AssemblyScanner
    {
        private IEnumerable<Assembly> _assemblies;

        /// <summary>
        /// Loads all assemblies in the current app domain into the scanner
        /// </summary>
        public AssemblyScanner()
        {
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Loads assemblies into the scanner
        /// </summary>
        /// <param name="assemblies"></param>
        public AssemblyScanner(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        /// <summary>
        /// Scans the loaded assemblies and executes <paramref name="scanAction"/> on each assembly 
        /// </summary>
        /// <param name="scanAction"></param>
        public virtual void Scan(Action<Assembly> scanAction)
        {
            foreach (var assembly in _assemblies)
            {
                scanAction(assembly);
            }
        }

        public virtual AssemblyScanner ApplyFilter(IScannerFilter filter)
        {
            _assemblies = filter.Apply(_assemblies);
            return this;
        }
    }

    public enum FilterType
    {
        Include,
        Exclude
    }

    public interface IScannerFilter
    {
        IEnumerable<Assembly> Apply(IEnumerable<Assembly> assemblies);
    }

    public class UnaryFilter : IScannerFilter
    {
        protected Func<string, bool> UnaryFunc { get; set; }
    ;
        public UnaryFilter(Func<string, bool> unaryFunc)
        {
            UnaryFunc = unaryFunc;
        }

        public UnaryFilter()
        {
        }

        public IEnumerable<Assembly> Apply(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                if (UnaryFunc == null || UnaryFunc(assembly.FullName))
                    yield return assembly;

                yield break;
            }

        }
    }

    public class StartsWithFilter : UnaryFilter
    {
        public StartsWithFilter(FilterType filterType, string startsWithValue)
            : base(assemblyName =>
                filterType == FilterType.Include
                ? assemblyName.StartsWith(startsWithValue)
                : !assemblyName.StartsWith(startsWithValue))
        {
        }
    }

    public class RegexFilter : UnaryFilter
    {
        public RegexFilter(FilterType filterType, string regexPattern) : this(filterType, new Regex(regexPattern))
        {
            
        }

        public RegexFilter(FilterType filterType, string regexPattern, RegexOptions regexOptions) : this(filterType, new Regex(regexPattern, regexOptions))
        {
            
        }

        public RegexFilter(FilterType filterType, Regex regex)
        {
            UnaryFunc =
                assemblyName =>
                    filterType == FilterType.Include ? regex.IsMatch(assemblyName) : !regex.IsMatch(assemblyName);
        }
    }
}

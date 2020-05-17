using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Linq;

namespace MappingGenerator.Mappings.MappingImplementors
{
    internal class FallbackMappingImplementor : IMappingMethodImplementor
    {
        private readonly IMappingMethodImplementor[] implementors;

        public FallbackMappingImplementor(params IMappingMethodImplementor[] implementors)
        {
            this.implementors = implementors;
        }

        public bool CanImplement(IMethodSymbol methodSymbol)
        {
            return implementors.Any(x => x.CanImplement(methodSymbol));
        }

        public IEnumerable<SyntaxNode> GenerateImplementation(IMethodSymbol methodSymbol, SyntaxGenerator generator, SemanticModel semanticModel, IEnumerable<INamedTypeSymbol> typeMappers)
        {
            foreach (var implementor in implementors)
            {
                var result = implementor.GenerateImplementation(methodSymbol, generator, semanticModel, typeMappers).ToList();
                if (result.Count > 0)
                {
                    return result;
                }
            }

            return Enumerable.Empty<SyntaxNode>();
        }
    }
}
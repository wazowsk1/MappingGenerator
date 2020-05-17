using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;

namespace MappingGenerator.Mappings.MappingImplementors
{
    public interface IMappingMethodImplementor
    {
        bool CanImplement(IMethodSymbol methodSymbol);

        IEnumerable<SyntaxNode> GenerateImplementation(IMethodSymbol methodSymbol, SyntaxGenerator generator, SemanticModel semanticModel, IEnumerable<INamedTypeSymbol> typeMappers);
    }
}
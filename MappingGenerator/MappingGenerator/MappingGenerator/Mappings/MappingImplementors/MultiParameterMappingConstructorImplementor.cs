using MappingGenerator.Mappings.MappingMatchers;
using MappingGenerator.Mappings.SourceFinders;
using MappingGenerator.RoslynHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;

namespace MappingGenerator.Mappings.MappingImplementors
{
    internal class MultiParameterMappingConstructorImplementor : IMappingMethodImplementor
    {
        public bool CanImplement(IMethodSymbol methodSymbol)
        {
            return methodSymbol.Parameters.Length > 1 && SymbolHelper.IsConstructor(methodSymbol);
        }

        public IEnumerable<SyntaxNode> GenerateImplementation(IMethodSymbol methodSymbol, SyntaxGenerator generator, SemanticModel semanticModel, IEnumerable<INamedTypeSymbol> typeMappers)
        {
            var mappingEngine = new MappingEngine(semanticModel, generator, methodSymbol.ContainingAssembly, typeMappers);
            var sourceFinder = new LocalScopeMappingSourceFinder(semanticModel, methodSymbol.Parameters);
            var targets = ObjectHelper.GetFieldsThaCanBeSetFromConstructor(methodSymbol.ContainingType);
            return mappingEngine.MapUsingSimpleAssignment(targets, new SingleSourceMatcher(sourceFinder));
        }
    }
}
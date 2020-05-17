using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;

namespace MappingGenerator.Mappings.MappingImplementors
{
    internal class SingleParameterPureMappingMethodImplementor : IMappingMethodImplementor
    {
        public bool CanImplement(IMethodSymbol methodSymbol)
        {
            return methodSymbol.Parameters.Length == 1 && methodSymbol.ReturnsVoid == false;
        }

        public IEnumerable<SyntaxNode> GenerateImplementation(IMethodSymbol methodSymbol, SyntaxGenerator generator, SemanticModel semanticModel, IEnumerable<INamedTypeSymbol> typeMappers)
        {
            var mappingEngine = new MappingEngine(semanticModel, generator, methodSymbol.ContainingAssembly, typeMappers);
            var source = methodSymbol.Parameters[0];
            var targetType = methodSymbol.ReturnType;
            var newExpression = mappingEngine.MapExpression((ExpressionSyntax)generator.IdentifierName(source.Name), source.Type, targetType);
            return new[] { generator.ReturnStatement(newExpression).WithAdditionalAnnotations(Formatter.Annotation) };
        }
    }
}
using MappingGenerator.RoslynHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;

namespace MappingGenerator.Mappings.MappingImplementors
{
    internal class ThisObjectToOtherMappingMethodImplementor : IMappingMethodImplementor
    {
        public bool CanImplement(IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsStatic == false &&
                   methodSymbol.Parameters.Length == 0 &&
                   methodSymbol.ReturnsVoid == false &&
                   ObjectHelper.IsSimpleType(methodSymbol.ReturnType) == false;
        }

        public IEnumerable<SyntaxNode> GenerateImplementation(IMethodSymbol methodSymbol, SyntaxGenerator generator, SemanticModel semanticModel, IEnumerable<INamedTypeSymbol> typeMappers)
        {
            var mappingEngine = new MappingEngine(semanticModel, generator, methodSymbol.ContainingAssembly, typeMappers);
            var targetType = methodSymbol.ReturnType;
            var newExpression = mappingEngine.MapExpression((ExpressionSyntax)generator.ThisExpression(), methodSymbol.ContainingType, targetType);
            return new[] { generator.ReturnStatement(newExpression).WithAdditionalAnnotations(Formatter.Annotation) };
        }
    }
}
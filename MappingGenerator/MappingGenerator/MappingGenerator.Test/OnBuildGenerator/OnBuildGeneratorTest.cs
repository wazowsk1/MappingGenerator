using MappingGenerator.OnBuildGenerator;
using NUnit.Framework;
using SmartCodeGenerator.TestKit;
using static MappingGenerator.Test.OnBuildGenerator.OnBuildGeneratorTestCases;

namespace MappingGenerator.Test.OnBuildGenerator
{
    public class OnBuildGeneratorTest
    {
        [Test]
        public void should_be_able_to_generate_mapping_interface_implementation()
        {
            var generatorFixture = new SmartCodeGeneratorFixture(typeof(OnBuildMappingGenerator), new[]
            {
                ReferenceSource.FromType<MappingInterface>()
            });


            generatorFixture.AssertGeneratedCode(_001_SimpleMappingInterface, _001_SimpleMappingInterface_TRANSFORMED);
        }
    }
}

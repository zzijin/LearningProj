using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Do not use banned APIs for analyzers
// 热重载会应ISourceGenerator的部分逻辑导致反复触发，在某些复杂的项目下，开启热重载之后，在编辑并继续界面将会等非常久，甚至再也无法继续。
// 因此设计了Incremental Generators来解决热重载的源代码生成性能问题
#pragma warning disable RS1035 

namespace RoslyGeneratorTest
{
    [Generator]
    internal class SetParamValueGenerator : ISourceGenerator
    {
        private const string attributeText = @"
using System;
namespace AutoSetProperty
{
    [AttributeUsage(AttributeTargets.Class)]
    [System.Diagnostics.Conditional(""SetParamValueGenerator_DEBUG"")]
    sealed class AutoSetPropertyAttribute : Attribute
    {
        public AutoSetPropertyAttribute()
        {
        }
        public string PropertyName { get; set; }
    }
}
";
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterForPostInitialization((i) => i.AddSource("AutoNotifyAttribute.g.cs", attributeText));
            
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            foreach (var candidateClass in receiver.CandidateClasses)
            {
                var className = candidateClass.Identifier.Text;
                var namespaceName = candidateClass.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().First().Name.ToString();

                var properties = candidateClass.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Select(p => p.Identifier.Text)
                    .ToList();

                var sourceBuilder = new StringBuilder();
                sourceBuilder.AppendLine($"namespace {namespaceName}");
                sourceBuilder.AppendLine("{");
                sourceBuilder.AppendLine($"    public partial class {className}");
                sourceBuilder.AppendLine("    {");
                sourceBuilder.AppendLine("        public void SetProperty<T>(string name, T value)");
                sourceBuilder.AppendLine("        {");
                sourceBuilder.AppendLine("            switch (name)");
                sourceBuilder.AppendLine("            {");

                foreach (var property in properties)
                {
                    sourceBuilder.AppendLine($"                case \"{property}\":");
                    sourceBuilder.AppendLine($"                    this.{property} = (T)Convert.ChangeType(value, typeof(T));");
                    sourceBuilder.AppendLine("                    break;");
                }

                sourceBuilder.AppendLine("                default:");
                sourceBuilder.AppendLine("                    throw new ArgumentException(\"Property not found.\", name);");
                sourceBuilder.AppendLine("            }");
                sourceBuilder.AppendLine("        }");
                sourceBuilder.AppendLine("    }");
                sourceBuilder.AppendLine("}");

                var sourceCode = sourceBuilder.ToString();
                context.AddSource($"{className}_AutoSetProperty.g.cs", sourceCode);
            }
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Any(attrList => attrList.Attributes.Any(attr => attr.Name.ToString() == "AutoSetPropertyAttribute")))
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataEntityGenerator.Helpers;
using DataEntityGenerator.Models;
using EnvDTE;
using EnvDTE80;

namespace DataEntityGenerator.Generators
{
    public abstract class BaseFileGenerator
    {
        private static readonly Regex GenericReplace = new Regex(@"([A-Za-z0-9\.]+)<([\s\S]+?)>", RegexOptions.Compiled);

        private const string ObjectTypeString = "System.Object";
        private const string DataEntityGeneratorAttributeName = "DataEntityGenerator";

        private static readonly Dictionary<string, string> SimpleTypeMapping = new Dictionary<string, string>
                                                                               {
                                                                                   { "System.String", "string" },
                                                                                   { "System.Int32", "number" },
                                                                                   { "System.Int16", "number" },
                                                                                   { "System.Int64", "number" },
                                                                                   { "System.Single", "number" },
                                                                                   { "System.Double", "number" },
                                                                                   { "System.Decimal", "number" },
                                                                                   { "System.UInt32", "number" },
                                                                                   { "System.UInt16", "number" },
                                                                                   { "System.UInt64", "number" },
                                                                                   { "System.Byte", "number" },
                                                                                   { "System.SByte", "number" },
                                                                                   { "System.Char", "number" },
                                                                                   { "System.Boolean", "boolean" }
                                                                               };

        private static readonly Dictionary<string, string> ArrayGenericTypeMapping = new Dictionary<string, string>
                                                                                     {
                                                                                         { "System.Collections.Generic.List", "Array" },
                                                                                         { "System.Collections.Generic.IEnumerable", "Array" },
                                                                                     };
        private readonly IEnumerable<ProjectItem> _projectItems;
        private readonly FileGeneratorFactory _factory;

        protected BaseFileGenerator(IEnumerable<ProjectItem> projectItems, FileGeneratorFactory factory)
        {
            _projectItems = projectItems;
            _factory = factory;
            DependencyCodeTypes = new Dictionary<string, EntityCodeType>();
        }

        public Dictionary<string, EntityCodeType> DependencyCodeTypes { get; }

        public void GenerateChildren(string path)
        {
            foreach (var dependencyCodeType in DependencyCodeTypes.Values)
            {
                var codeType = dependencyCodeType.Type;
                var generator = _factory.GetFileGenerator(codeType.Kind, _projectItems);
                generator.Generate(path, codeType);
                generator.GenerateChildren(path);
            }
        }

        public void Merge(Dictionary<string, EntityCodeType> dependencyCodeTypes)
        {
            foreach (var dependencyCodeType in dependencyCodeTypes)
            {
                DependencyCodeTypes[dependencyCodeType.Key] = dependencyCodeType.Value;
            }
        }

        public void Generate(string path, CodeType codeType)
        {
            var sb = new StringBuilder();
            var name = codeType.Name.ToCamelCase();
            Render(sb, codeType, path, name);

            foreach (var dependencyCodeType in DependencyCodeTypes)
            {
                var childName = dependencyCodeType.Value.Type.Name.ToCamelCase();
                sb.Insert(0, $"import {{ {childName} }} from './{childName}'\r\n");
            }

            using (var file = File.Open(Path.Combine(path, $"{name}.ts"), FileMode.Create,
                                        FileAccess.ReadWrite,
                                        FileShare.Read))
            {
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(sb.ToString());
                }
            }

        }

        protected abstract void Render(StringBuilder sb, CodeType codeType, string path, string name);

        protected CodeType FindCodeTypeByFullName(string name)
        {
            foreach (var projectItem in _projectItems)
            {
                return projectItem.ContainingProject.CodeModel.CodeTypeFromFullName(name);
            }

            return null;
        }

        protected CodeTypeRef ToCodeTypeRef(CodeType element)
        {
            foreach (var projectItem in _projectItems)
            {
                return projectItem.ContainingProject.CodeModel.CreateCodeTypeRef(element);
            }

            return null;
        }

        protected CodeTypeRef FindTypeByFullName(string name)
        {
            foreach (var projectItem in _projectItems)
            {
                var element = projectItem.ContainingProject.CodeModel.CodeTypeFromFullName(name);
                if (element != null)
                {
                    return projectItem.ContainingProject.CodeModel.CreateCodeTypeRef(element);
                }
            }

            return null;
        }

        protected string GetType(CodeTypeRef type, string path, StringBuilder sb, CodeProperty property)
        {
            var fullName = type.AsFullName;
            if (SimpleTypeMapping.ContainsKey(fullName))
            {
                return SimpleTypeMapping[fullName];
            }

            if (fullName.Contains("[]"))
            {
                return $"Array<{GetType(type.ElementType, path, sb, null)}>";
            }

            var generic = GenericReplace.Match(fullName);
            if (generic.Success)
            {
                if (ArrayGenericTypeMapping.ContainsKey(generic.Groups[1].Value))
                {
                    var element = FindCodeTypeByFullName(generic.Groups[2].Value);
                    return element != null ? $"Array<{GetType(ToCodeTypeRef(element), path, sb, null)}>" : string.Empty;
                }
            }

            if (property != null && fullName.Equals(ObjectTypeString, StringComparison.Ordinal) && property.Kind == vsCMElement.vsCMElementProperty)
            {
                var attributes = ((CodeProperty2)property).Attributes.Cast<CodeAttribute2>().ToArray();
                foreach (var attribute in attributes)
                {
                    if (!attribute.Name.Equals(DataEntityGeneratorAttributeName, StringComparison.Ordinal)) continue;
                    var argument = attribute.Arguments.Cast<CodeAttributeArgument>().First();
                    var value = Regex.Replace(argument.Value, "^typeof.*\\((.*)\\)$", "$1");
                    var codeElements =
                        property.ProjectItem.FileCodeModel.CodeElements.Cast<CodeElement>().ToArray();
                    var imports =
                        GetTypeByKind(codeElements,
                                      vsCMElement.vsCMElementImportStmt).Cast<CodeImport>().Select(x => x.Namespace).ToList();
                    var namespaces = GetTypeByKind(codeElements, vsCMElement.vsCMElementNamespace).Cast<CodeNamespace>().Select(x => x.Name).ToArray();
                    if (namespaces.Any())
                    {
                        imports.AddRange(namespaces);
                    }

                    foreach (var codeImport in imports)
                    {
                        var typeCode = FindCodeTypeByFullName($"{codeImport}.{value}");
                        if (typeCode == null) continue;
                        DependencyCodeTypes[typeCode.FullName] = new EntityCodeType { Type = typeCode };
                        return "any";
                    }
                }
            }

            var childCodeType = FindCodeTypeByFullName(fullName);
            if (childCodeType != null)
            {
                DependencyCodeTypes[childCodeType.FullName] = new EntityCodeType { Type = childCodeType, IsNeedImport = true };
                return childCodeType.Name.ToCamelCase();
            }

            return string.Empty;
        }


        private List<CodeElement> GetTypeByKind(CodeElement[] codeElements, vsCMElement kind)
        {
            var list = new List<CodeElement>();
            foreach (CodeElement codeElement in codeElements)
            {
                if (codeElement.Kind == kind)
                {
                    list.Add(codeElement);
                }
                else
                {
                    list.AddRange(GetTypeByKind(codeElement.Children.Cast<CodeElement>().ToArray(), kind));
                }
            }

            return list;
        }

    }
}
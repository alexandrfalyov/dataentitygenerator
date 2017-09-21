using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntityGenerator.Helpers;
using EnvDTE;
using EnvDTE80;

namespace DataEntityGenerator.Generators
{
    public class ClassFileGenerator : BaseFileGenerator
    {
        public ClassFileGenerator(IEnumerable<ProjectItem> projectItems, FileGeneratorFactory fileGeneratorFactory)
            : base(projectItems, fileGeneratorFactory)
        {
        }

        protected override void Render(StringBuilder sb, CodeType codeType, string path, string name)
        {
            var properties = GetPublicProperties((CodeClass2)codeType);
            sb.Append($"export interface {name} {{");
            sb.AppendLine();
            foreach (var property in properties)
            {
                sb.AppendTabs();
                sb.Append($"{property.Name.ToCamelCase()}: {GetType((CodeTypeRef2)property.Type, path, sb, property)}");

                sb.AppendLine();
            }

            sb.Append("}");

        }

        private CodeProperty2[] GetPublicProperties(CodeClass2 codeElement)
        {
            return codeElement.Members.Cast<CodeElement>().Where(element => element.Kind == vsCMElement.vsCMElementProperty)
                .Cast<CodeProperty2>().Where(property => property.Access == vsCMAccess.vsCMAccessPublic).ToArray();
        }
    }
}
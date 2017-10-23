using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntityGenerator.Helpers;
using EnvDTE;
using EnvDTE80;

namespace DataEntityGenerator.Generators
{
    public class EnumFileGenerator : BaseFileGenerator
    {
        public EnumFileGenerator(IEnumerable<ProjectItem> projectItems, FileGeneratorFactory fileGeneratorFactory)
            : base(projectItems, fileGeneratorFactory)
        {
        }

        protected override void Render(StringBuilder sb, CodeType codeType, string path, string name)
        {
            var enumFields = GetEnumFields((CodeEnum)codeType);
            sb.Append($"export enum {name} {{");
            sb.AppendLine();
            for (var i = 0; i < enumFields.Length; i++)
            {
                var field = enumFields[i];
                sb.AppendTabs();
                sb.Append($"{field.Prototype.ToCamelCase()}");
                if (i + 1 != enumFields.Length)
                {
                    sb.Append(",");
                }

                sb.AppendLine();
            }

            sb.Append("}");
        }

        private CodeVariable2[] GetEnumFields(CodeEnum codeEnum)
        {
            return codeEnum.Members.Cast<CodeElement>().Where(x => x.Kind == vsCMElement.vsCMElementVariable).Cast<CodeVariable2>().ToArray();
        }
    }
}
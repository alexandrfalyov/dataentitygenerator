using System;
using System.Collections.Generic;
using EnvDTE;

namespace DataEntityGenerator.Generators
{
    public class FileGeneratorFactory
    {
        public BaseFileGenerator GetFileGenerator(vsCMElement kind, IEnumerable<ProjectItem> projectItems)
        {
            BaseFileGenerator fileGenerator = null;
            switch (kind)
            {
                case vsCMElement.vsCMElementEnum:
                    fileGenerator = new EnumFileGenerator(projectItems, this);
                    break;
                case vsCMElement.vsCMElementClass:
                    fileGenerator = new ClassFileGenerator(projectItems, this);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return fileGenerator;
        }
    }
}
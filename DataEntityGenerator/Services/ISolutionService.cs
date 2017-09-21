using System.Collections.Generic;
using EnvDTE;

namespace DataEntityGenerator.Services
{
    public interface ISolutionService
    {
        List<ProjectItem> GetSolutionCSFiles(bool useDtoSuffix);
        ProjectItem GetSelectedProjectItem();
    }
}
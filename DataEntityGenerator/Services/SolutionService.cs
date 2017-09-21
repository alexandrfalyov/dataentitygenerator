using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;

namespace DataEntityGenerator.Services
{
    public class SolutionService : ISolutionService
    {
        private readonly DTE2 _dte;

        public SolutionService(DTE2 dte)
        {
            _dte = dte;
        }

        public List<ProjectItem> GetSolutionCSFiles(bool useDtoSuffix)
        {
            var list = new List<ProjectItem>();
            foreach (Project project in _dte.Solution.Projects)
            {
                list.AddRange(GetInnerProjectItems(project.ProjectItems, useDtoSuffix));
            }

            return list.OrderBy(x=>x.Name).ToList();
        }

        public ProjectItem GetSelectedProjectItem()
        {
            return _dte.SelectedItems.Count == 0 ? null : _dte.SelectedItems.Item(1).ProjectItem;
        }

        private List<ProjectItem> GetInnerProjectItems(ProjectItems projectItems, bool useDtoSuffix)
        {
            var list = new List<ProjectItem>();
            if (projectItems == null) return list;
            var endWord = useDtoSuffix ? "DTO.cs" : ".cs";
            foreach (ProjectItem projectItem in projectItems)
            {
                var name = projectItem.Name;
                if (name.EndsWith(endWord))
                {
                    list.Add(projectItem);
                }
                else
                {
                    list.AddRange(GetInnerProjectItems(projectItem.ProjectItems, useDtoSuffix));
                }
            }

            return list;
        }
    }
}
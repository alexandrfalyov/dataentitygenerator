using EnvDTE;

namespace DataEntityGenerator.Models
{
    public class EntityCodeType
    {
        public CodeType Type { get; set; }

        public bool IsNeedImport { get; set; }
    }
}
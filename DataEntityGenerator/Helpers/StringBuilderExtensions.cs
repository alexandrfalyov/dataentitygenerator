using System.Text;

namespace DataEntityGenerator.Helpers
{
    public static class StringBuilderExtensions
    {
        public static void AppendTabs(this StringBuilder sb, int count = 1, int length = 4)
        {
            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    sb.Append(" ");
                }
            }
        }
    }
}
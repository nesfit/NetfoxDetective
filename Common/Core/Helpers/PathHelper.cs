using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.Core.Helpers
{
    public static class PathHelper
    {
        public static string CombineLongPath(string path1, string path2) { return Path.Combine($@"\\?\{path1}",path2); }
    }
}

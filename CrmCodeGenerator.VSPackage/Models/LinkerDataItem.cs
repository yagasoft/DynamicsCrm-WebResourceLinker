using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebResourceLinker
{
    [Serializable]
    public class LinkerDataItem
    {
        public Guid WebResourceId { get; set; }
        public string SourceFilePath { get; set; }
    }
}

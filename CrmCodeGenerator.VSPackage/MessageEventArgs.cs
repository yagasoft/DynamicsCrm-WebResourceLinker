using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebResourceLinkerExt.VSPackage
{
	public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string MessageExtended { get; set; }
    }
}

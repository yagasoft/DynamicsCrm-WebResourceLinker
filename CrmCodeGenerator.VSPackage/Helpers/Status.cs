using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;

namespace WebResourceLinkerExt.VSPackage.Helpers
{
    public static class Status
    {
	    private static bool isNewLine = true;

        public static void Update(string message, bool newLine = true)
        {
            //Configuration.Instance.DTE.ExecuteCommand("View.Output");
            var dte = (DTE)Package.GetGlobalService(typeof(SDTE));
            var win = dte.Windows.Item("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}");
            win.Visible = true;

            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var guidGeneral = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
	        outputWindow.CreatePane(guidGeneral, "Web Resource Linker Extension", 1, 0);
            outputWindow.GetPane(guidGeneral, out var pane);
            pane.Activate();

	        if (isNewLine)
	        {
		        message = $"{DateTime.Now:yyyy/MMM/dd hh:mm:ss tt}: {message}";
	        }

			pane.OutputString(message);

			if (newLine)
			{
				pane.OutputString("\n");
			}

			pane.FlushToTaskList();
            Application.DoEvents();

	        isNewLine = newLine;
        }

	    public static void Clear()
	    {
		    var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
		    var guidGeneral = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid;

		    if (outputWindow == null)
		    {
			    return;
		    }

		    outputWindow.CreatePane(guidGeneral, "Web Resource Linker Extension", 1, 0);
		    outputWindow.GetPane(guidGeneral, out var pane);
		    pane.Clear();
		    pane.FlushToTaskList();
		    Application.DoEvents();
	    }
    }
}

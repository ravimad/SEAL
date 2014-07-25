using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using EnvDTE;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.SourceSinkPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidSourceSinkPackagePkgString)]
    public sealed class SourceSinkPackagePackage : Package
    {
  
        string functioname = "";
        Project project = null;
        string dllfilename = "";

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public SourceSinkPackagePackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                
                CommandID menuCommandID = new CommandID(GuidList.guidSourceSinkPackageCmdSet, (int)PkgCmdIDList.cmdidSetAsSource);
                MenuCommand menuItem = new MenuCommand(SetAsSourceCallBack, menuCommandID );
                mcs.AddCommand( menuItem );

                menuCommandID = new CommandID(GuidList.guidSourceSinkPackageCmdSet, (int)PkgCmdIDList.cmdidSetAsSink);
                menuItem = new MenuCommand(SetAsSinkCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidSourceSinkPackageCmdSet, (int)PkgCmdIDList.cmdidSetAsAnalyzingFunction);
                menuItem = new MenuCommand(SetAsAnalyzingFunctionCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidSourceSinkPackageCmdSet, (int)PkgCmdIDList.cmdidAnalyze);
                menuItem = new MenuCommand(AnalyzeCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidSourceSinkPackageCmdSet, (int)PkgCmdIDList.cmdidCastAnalysis);
                menuItem = new MenuCommand(CastAnalysisCallBack, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidSourceSinkPackageCmdSet, (int)PkgCmdIDList.cmdidClearAll);
                menuItem = new MenuCommand(ClearAllCallback, menuCommandID);
                mcs.AddCommand(menuItem);

            }
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "SourceSinkPackage",
                       string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }

        private void SetAsSourceCallBack(object sender, EventArgs e)
        {
            /*sourcefilename = myDTE.ActiveDocument.Path + myDTE.ActiveDocument.Name;

            IVsTextManager txtMgr = (IVsTextManager)GetService(typeof(SVsTextManager));
            int mustHaveFocus = 1;
            IVsTextView vTextView;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);

            int linenumber, columnnumber;
            vTextView.GetCaretPos(out linenumber, out columnnumber);

            sourcelinenumber = linenumber + 1;*/

            DTE myDTE = GetService(typeof(SDTE)) as DTE;
            SourceSinkEP.sourceEP = (myDTE.ActiveDocument.Object("TextDocument") as TextDocument).Selection.ActivePoint.CreateEditPoint();
            
        }

        private void SetAsSinkCallBack(object sender, EventArgs e)
        {
            /*sinkfilename = myDTE.ActiveDocument.Path + myDTE.ActiveDocument.Name;
            IVsTextManager txtMgr = (IVsTextManager)GetService(typeof(SVsTextManager));
            int mustHaveFocus = 1;
            IVsTextView vTextView;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);

            int linenumber, columnnumber;
            vTextView.GetCaretPos(out linenumber, out columnnumber);

            sinklinenumber = linenumber + 1;*/

            DTE myDTE = GetService(typeof(SDTE)) as DTE;
            SourceSinkEP.sinkEP = (myDTE.ActiveDocument.Object("TextDocument") as TextDocument).Selection.ActivePoint.CreateEditPoint();

        }

        private void SetAsAnalyzingFunctionCallBack(object sender, EventArgs e)
        {
            
            DTE myDTE = GetService(typeof(SDTE)) as DTE;
            CodeElement function = (myDTE.ActiveDocument.Object("TextDocument") as TextDocument).Selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementFunction);

            if (function != null)
            {
                functioname = function.FullName;
                SourceSinkEP.functionEP = function.StartPoint.CreateEditPoint();
                project = myDTE.ActiveDocument.ProjectItem.ContainingProject;
                //projectname = myDTE.ActiveDocument.ProjectItem.ContainingProject.FullName;

                string outputfilerelative = myDTE.ActiveDocument.ProjectItem.ContainingProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                string projectlocation = myDTE.ActiveDocument.ProjectItem.ContainingProject.Properties.Item("FullPath").Value.ToString();
                string outputfilename = myDTE.ActiveDocument.ProjectItem.ContainingProject.Properties.Item("OutputFileName").Value.ToString();
                dllfilename = projectlocation + outputfilerelative + outputfilename;
            }
            else
            {
                functioname = "";
                SourceSinkEP.functionEP = null;

                IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                Guid clsid = Guid.Empty;
                int result;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                           0,
                           ref clsid,
                           "Error",
                           string.Format(CultureInfo.CurrentCulture, "Not a function"),
                           string.Empty,
                           0,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                           OLEMSGICON.OLEMSGICON_INFO,
                           0,        // false
                           out result));
            }
        }

        private void AnalyzeCallBack(object sender, EventArgs e)
        {
            DTE myDTE = GetService(typeof(SDTE)) as DTE;

            bool error = false;
            string errormessage = "";
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;

            if(functioname == "")
            {
                //TODO: Get the entry function (for exe output files)
            }


            if(SourceSinkEP.sourceEP == null)
            {
                error = true;
                errormessage = "Source not selected";
            }
            else if (SourceSinkEP.sinkEP == null)
            {
                error = true;
                errormessage = "Sink not selected";
            }
            else if (SourceSinkEP.functionEP == null)
            {
                error = true;
                errormessage = "Could not automatically determine the entry function. Please select a function as entry point";
            }

            if(error)
            {
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                           0,
                           ref clsid,
                           "Source Sink Analysis",
                           string.Format(CultureInfo.CurrentCulture, "Error : {0}",errormessage),
                           string.Empty,
                           0,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                           OLEMSGICON.OLEMSGICON_INFO,
                           0,        // false
                           out result));
                return;
            }

            myDTE.Solution.SolutionBuild.BuildProject("Debug", project.UniqueName, true);

            string sourcefilename = SourceSinkEP.sourceEP.Parent.Parent.FullName;
            int sourcelinenumber = SourceSinkEP.sourceEP.Line;
            string sinkfilename = SourceSinkEP.sinkEP.Parent.Parent.FullName;
            int sinklinenumber = SourceSinkEP.sinkEP.Line;

            string sealHome = Environment.GetEnvironmentVariable("SEALHOME");

            String scriptText = sealHome + "\\Bin\\Checker.exe /in (\"" + dllfilename + "\") /analysistype(\"sourcesinkanalysis\") /sourcefile(\"" + sourcefilename + "\") /sourceline " +  sourcelinenumber + " /sinkfile(\"" + sinkfilename + "\") /sinkline " +  sinklinenumber + " /function(\"" + functioname + "\")";

            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(scriptText);
            pipeline.Commands.Add("Out-String");
            Collection<PSObject> results = pipeline.Invoke();
            runspace.Close();

            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }
            String output = stringBuilder.ToString();


            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "Source Sink Analysis",
                       string.Format(CultureInfo.CurrentCulture, "{0}", output),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));

        }

        private void ClearAllCallback(object sender, EventArgs e)
        {
            SourceSinkEP.sourceEP = null;
            SourceSinkEP.sinkEP = null;
            SourceSinkEP.functionEP = null;

            functioname = "";
            project = null;
            dllfilename = "";
        }

        private void CastAnalysisCallBack(object sender, EventArgs e)
        {

            DTE myDTE = GetService(typeof(SDTE)) as DTE;

            IVsTextManager txtMgr = (IVsTextManager)GetService(typeof(SVsTextManager));
            int mustHaveFocus = 1;
            IVsTextView vTextView;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);

            int linenumber, columnnumber;
            vTextView.GetCaretPos(out linenumber, out columnnumber);
            int castlinenumber = linenumber + 1;

            string castfilename = myDTE.ActiveDocument.FullName;

            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;

            if (functioname == "")
            {
                //TODO: Get the entry function (for exe output files)

                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "Cast Analysis",
                       string.Format(CultureInfo.CurrentCulture, "Could not automatically determine the entry function. Please select a function as entry point"),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));

                return;

            }

            myDTE.Solution.SolutionBuild.BuildProject("Debug", project.UniqueName, true);
            
            string sealHome = Environment.GetEnvironmentVariable("SEALHOME");

            String scriptText = sealHome + "\\Bin\\Checker.exe /in (\"" + dllfilename + "\") /analysistype(\"castanalysis\") /castfile(\"" + castfilename + "\") /castline " + castlinenumber + " /function(\"" + functioname + "\")";

            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(scriptText);
            pipeline.Commands.Add("Out-String");
            Collection<PSObject> results = pipeline.Invoke();
            runspace.Close();

            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }
            String output = stringBuilder.ToString();

            
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "Cast Analysis",
                       string.Format(CultureInfo.CurrentCulture, "{0}", output),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));

        }

    }

}

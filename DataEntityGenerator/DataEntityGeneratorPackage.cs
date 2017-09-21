using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using DataEntityGenerator.Generators;
using DataEntityGenerator.Services;
using DataEntityGenerator.ViewModels.FileSelector;
using DataEntityGenerator.Views.FileSelector;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DataEntityGenerator
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(DataEntityGeneratorPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(FileSelectorWindow))]
    public sealed class DataEntityGeneratorPackage : Package, IVsSelectionEvents
    {
        private FileSelectorViewModel _findSelectorModel;
        private DTE2 _dte;
        private IVsMonitorSelection _monitorSelection;
        private uint _selectionHandle;

        /// <summary>
        /// DataEntityGenerator GUID string.
        /// </summary>
        public const string PackageGuidString = "af5e03c4-68b0-426a-9f0f-4ba9e3ce9d72";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityGeneratorPackage"/> class.
        /// </summary>
        public DataEntityGeneratorPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            _dte = (DTE2)GetGlobalService(typeof(DTE));
            var service = new SolutionService(_dte);
            ((IServiceContainer)this).AddService(typeof(ISolutionService), service);

            _findSelectorModel = new FileSelectorViewModel(service, new FileGeneratorFactory());

            GenerateTSClass.Initialize(this, _findSelectorModel);
            FileSelectorWindowCommand.Initialize(this);
            _monitorSelection = GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            _monitorSelection?.AdviseSelectionEvents(this, out _selectionHandle);
        }

        protected override WindowPane CreateToolWindow(Type toolWindowType, int id)
        {
            var window = base.CreateToolWindow(toolWindowType, id);
            if (window is FileSelectorWindow fileSelectorWindow)
            {
                fileSelectorWindow.Model = new FileSelectorViewModel((ISolutionService)GetService(typeof(ISolutionService)), new FileGeneratorFactory());
            }

            return window;
        }

        protected override int CreateToolWindow(ref Guid toolWindowType, int id)
        {
            var value = base.CreateToolWindow(ref toolWindowType, id);
            if (toolWindowType == Guids.FileSelectorWindowGuid)
            {
                var window = FindToolWindow(typeof(FileSelectorWindow), id, false);
                if (window is FileSelectorWindow fileSelectorWindow)
                {
                    fileSelectorWindow.Model = _findSelectorModel;
                }
            }

            return value;
        }

        #endregion

        public int OnSelectionChanged(IVsHierarchy pHierOld,
                                      uint itemidOld,
                                      IVsMultiItemSelect pMisOld,
                                      ISelectionContainer pScOld,
                                      IVsHierarchy pHierNew,
                                      uint itemidNew,
                                      IVsMultiItemSelect pMisNew,
                                      ISelectionContainer pScNew)
        {
            uint count = 0;
            pScNew?.CountObjects((uint)Microsoft.VisualStudio.Shell.Interop.Constants.GETOBJS_SELECTED, out count);
            if (count == 1)
            {
                var obj = new object[1];
                pScNew?.GetObjects((uint)Microsoft.VisualStudio.Shell.Interop.Constants.GETOBJS_SELECTED, 1, obj);

                var item = obj[0];
                if (item != null && item.GetType().Name == "DynamicTypeBrowseObjectFolder")
                {
                    _findSelectorModel.LoadSelectedFolder();
                }
            }

            return VSConstants.S_OK;
        }

        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }

        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }

        protected override void Dispose(bool disposing)
        {
            _monitorSelection?.UnadviseSelectionEvents(_selectionHandle);
            _monitorSelection = null;
            base.Dispose(disposing);
        }
    }
}

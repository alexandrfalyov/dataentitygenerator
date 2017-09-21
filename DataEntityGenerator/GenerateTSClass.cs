using System;
using System.ComponentModel.Design;
using DataEntityGenerator.Services;
using DataEntityGenerator.ViewModels.FileSelector;
using Microsoft.VisualStudio.Shell;

namespace DataEntityGenerator
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateTSClass
    {
        private readonly FileSelectorViewModel _findSelectorModel;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d6d38f24-7d46-4ec9-8200-2fe1853854dd");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateTSClass"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="findSelectorModel"></param>
        private GenerateTSClass(Package package, FileSelectorViewModel findSelectorModel)
        {
            _findSelectorModel = findSelectorModel;
            this._package = package ?? throw new ArgumentNullException(nameof(package));

            if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandId = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GenerateTSClass Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => this._package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="findSelectorModel"></param>
        public static void Initialize(Package package, FileSelectorViewModel findSelectorModel)
        {
            Instance = new GenerateTSClass(package, findSelectorModel);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            _findSelectorModel.LoadSelectedFolder();
            _findSelectorModel?.Load();
        }
    }
}

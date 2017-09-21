using DataEntityGenerator.Services;
using DataEntityGenerator.ViewModels.FileSelector;

namespace DataEntityGenerator.Views.FileSelector
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for FileSelectorWindowControl.
    /// </summary>
    public partial class FileSelectorWindowControl : UserControl
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:DataEntityGenerator.Views.FileSelector.FileSelectorWindowControl" /> class.
        /// </summary>
        public FileSelectorWindowControl()
        {
            this.InitializeComponent();
            Loaded += FileSelectorWindowControlLoaded;
        }

        private void FileSelectorWindowControlLoaded(object sender, RoutedEventArgs e)
        {
            Model?.Load();
        }

        public FileSelectorViewModel Model
        {
            get => DataContext as FileSelectorViewModel;
            set => DataContext = value;
        }
    }
}
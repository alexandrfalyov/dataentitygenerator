using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using DataEntityGenerator.Annotations;
using DataEntityGenerator.Generators;
using DataEntityGenerator.Helpers;
using DataEntityGenerator.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using MessageBox = System.Windows.MessageBox;

namespace DataEntityGenerator.ViewModels.FileSelector
{
    public class FileSelectorViewModel : INotifyPropertyChanged
    {
        private readonly ISolutionService _solutionService;
        private readonly FileGeneratorFactory _fileGeneratorFactory;
        private List<ProjectItem> _items;
        private ProjectItem _selectedFolder;
        private bool _useDTOSuffix;
        private string _selectedPath;

        public FileSelectorViewModel(ISolutionService solutionService, FileGeneratorFactory fileGeneratorFactory)
        {
            _solutionService = solutionService;
            _fileGeneratorFactory = fileGeneratorFactory;
            UseDTOSuffix = true;
            GenerateCommand = new DelegateCommand(OnGenerateCommandExecute, OnGenerateCommandCanExecute);
        }

        public ICommand GenerateCommand { get; }

        public List<ProjectItem> Items
        {
            get => _items;
            set
            {
                if (Equals(value, _items)) return;
                _items = value;
                OnPropertyChanged();
            }
        }

        public ProjectItem SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (Equals(value, _selectedFolder)) return;
                _selectedFolder = value;
                OnPropertyChanged();
            }
        }

        public bool UseDTOSuffix
        {
            get => _useDTOSuffix;
            set
            {
                if (value == _useDTOSuffix) return;
                _useDTOSuffix = value;
                OnPropertyChanged();
                Load();
            }
        }

        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                if (value == _selectedPath) return;
                _selectedPath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Load()
        {
            Items = _solutionService.GetSolutionCSFiles(UseDTOSuffix);
        }

        public void LoadSelectedFolder()
        {
            SelectedFolder = _solutionService.GetSelectedProjectItem();
            SelectedPath = (string)SelectedFolder?.Properties.Item("DefaultNamespace")?.Value;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private bool OnGenerateCommandCanExecute(object o)
        {
            return SelectedFolder != null;
        }

        private void OnGenerateCommandExecute(object obj)
        {
            try
            {
                var selectedItems = ((IList)obj)?.Cast<ProjectItem>().ToArray();
                if (selectedItems == null) return;
                var path = (string)SelectedFolder.Properties.Item("FullPath").Value;
                foreach (var item in selectedItems)
                {
                    var codeElements = item.FileCodeModel.CodeElements.Cast<CodeElement>().ToArray();
                    CreateFile(GetMainType(codeElements), path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Generation error");
            }
        }

        private void CreateFile(CodeType codeType, string path)
        {
            if (codeType == null)
            {
                return;
            }

            _fileGeneratorFactory.GetFileGenerator(codeType.Kind, Items).Generate(path, codeType);
        }

        private CodeType GetMainType(CodeElement[] codeElements)
        {
            foreach (CodeElement codeElement in codeElements)
            {
                if (codeElement.Kind == vsCMElement.vsCMElementClass ||
                    codeElement.Kind == vsCMElement.vsCMElementEnum)
                {
                    return (CodeType)codeElement;
                }

                var child = GetMainType(codeElement.Children.Cast<CodeElement>().ToArray());
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
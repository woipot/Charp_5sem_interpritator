﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Interpritator.Source.Convertors;
using Interpritator.Source.Interpritator;
using Interpritator.Source.MVVM.Models;
using Interpritator.Source.TreeView;
using Interpritator.Source.UserInterfaceUtilities;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Interpritator.Source.MVVM
{
    internal class InterpritatorVM : BindableBase
    {

        private FileSystemInfo _currentFilePath;

        private string _resultOutput;
        private string _errorOutput;
        private string _commandInput;

        private List<string> _commandsList;
        private int _currentCommand;
        private bool _isDebugMode;

        #region Properties
        public ObservableCollection<FileSystemObjectInfo> CurrentDirectories { get; set; }

        public string CurrentFileName
        {
            get => _currentFilePath?.Name ?? "file not open";
            set => _currentFilePath = new FileInfo(value);
        }

        public ObservableCollection<BreakPoint> BreakPointsList { get; set; }

        public string CommandInput
        {
            get => _commandInput;
            set
            {
                _commandInput = value;
                Update();

                OnPropertyChanged("CommandInput");
            }
        }

        public string ResultOutput
        {
            get => _resultOutput;
            private set
            {
                _resultOutput = value;
                OnPropertyChanged("ResultOutput");
            }
        }

        public string ErrorOutput
        {
            get => _errorOutput;
            private set
            {
                _errorOutput = value;
                OnPropertyChanged("ErrorOutput");
            }
        }

        public bool IsDebugMode
        {
            get => _isDebugMode;
            private set
            {
                _isDebugMode = value;
                OnPropertyChanged("IsDebugMode");
                OnPropertyChanged("IsSimpleMode");
            }
        }

        public bool IsSimpleMode => !IsDebugMode;

        #endregion


        public InterpritatorVM()
        {
            _currentFilePath = null;
            BreakPointsList = new ObservableCollection<BreakPoint>();

            OpenProjectCommand = new DelegateCommand(OpenProject);
            SaveFileCommand    = new DelegateCommand(Save);
            SaveFileAsCommand    = new DelegateCommand(SaveAs);

            ExitCommand = new DelegateCommand(Exit);

            StartCommand = new DelegateCommand(Start);
            StartDebugCommand = new DelegateCommand(StartDebug);
            StepOutCommand = new DelegateCommand(StepToNextBp);
            NextStepCommand = new DelegateCommand(Step);

            TreeDoubleClickCommand = new DelegateCommand<object>(TreeViewClick);
            CurrentDirectories = new ObservableCollection<FileSystemObjectInfo>(){new FileSystemObjectInfo(new DirectoryInfo(Directory.GetCurrentDirectory()))};

            HelpCommand = new DelegateCommand(OpenHelp);
        }

        private void OpenHelp()
        {
            var helpwin = new Help();
            helpwin.Show();
        }


        #region Commands
        public DelegateCommand OpenProjectCommand { get; set; }
        public DelegateCommand SaveFileCommand {get; set;}
        public DelegateCommand SaveFileAsCommand {get; set;}
        public DelegateCommand ExitCommand { get; set; }

        public DelegateCommand StartCommand { get; set; }
        public DelegateCommand StartDebugCommand { get; set; }
        public DelegateCommand NextStepCommand { get; set; }
        public DelegateCommand StepOutCommand { get; set; }

        public DelegateCommand HelpCommand { get; set; }


        public DelegateCommand<object> TreeDoubleClickCommand { get; set; }

        #endregion


        #region Menu Func

        private void SaveAs()
        {
            var isGoodDialogResult = SaveFileDialog();
            if (isGoodDialogResult)
            {
                MainMenuFunc.SaveFile(_currentFilePath.FullName, CommandInput);
            }
        }

        private void Save()
        {
            if (_currentFilePath == null)
            {
                var isGoodDialogResult = SaveFileDialog();
                if (isGoodDialogResult)
                {
                    MainMenuFunc.SaveFile(_currentFilePath.FullName, CommandInput);
                }
            }
            else
            {
                MainMenuFunc.SaveFile(_currentFilePath.FullName, CommandInput);
            }
        }


        private void Exit()
        {
            Application.Current.Shutdown();
        }

        #region Run Menu
        private void Start()
        {
            if (_commandsList == null || _commandsList.Count == 0)
            {
                MessageBox.Show("Commands not found");
                return;
            }

            var patch =  SaveBinFileDialog();
            if (patch != null)
            {
                try
                {
                    Compiler.SaveToBinFile(patch, _commandsList);
                    var interpritatorResult = NumberCommandInterpritator.ExecuteProgram(patch);

                    ResultOutput = interpritatorResult.Key;
                    ErrorOutput = interpritatorResult.Value;
                }
                catch (CompilerException ce)
                {
                    ErrorOutput = "#Error: " + ce.Message + "-->" + ce.WrongCommand + "Command № [" + ce.CommandNumber + "]\n";
                }

            }
        }

        private void StartDebug()
        {
            if (_commandsList == null || _commandsList.Count == 0)
            {
                MessageBox.Show("Commands not found");
                return;
            }

            _resultOutput = "";
            _errorOutput = "";
            IsDebugMode = true;

            _currentCommand = 0;
            StepToNextBp(); 
           
       }
        #endregion

        #endregion


        #region Dialogs

        private bool OpenFileDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = "inw",
                Filter = "Simple interpritator text file|*.inw"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CurrentFileName = openFileDialog.FileName;
                return true;
            }
            return false;
        }

        private bool SaveFileDialog()
        {

            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "inw",
                Filter = "Simple interpritator text file|*.inw"
            };

            if (saveFileDialog.ShowDialog() == true)
            {

                CurrentFileName = saveFileDialog.FileName;
                return true;
            }
            return false;
        }

        private string OpenBinFileDialog()
        {
            string newFilePatch = null;
            var openFileDialog = new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = "inwoi",
                Filter = "Command interpritator file|*.inwoi"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                newFilePatch = openFileDialog.FileName;
            }
            return newFilePatch;
        }

        private string SaveBinFileDialog()
        {
            string newFilePatch = null;
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "inwoi",
                Filter = "Command interpritator file|*.inwoi"
            };

            if (saveFileDialog.ShowDialog() == true)
            {

                newFilePatch = saveFileDialog.FileName;
            }
            return newFilePatch;
        }

        #endregion


        private void OpenProject()
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();
            if (result == DialogResult.OK)
            {
                _currentFilePath = null;
                CurrentDirectories.Clear();
                CurrentDirectories.Add(new FileSystemObjectInfo(new DirectoryInfo(fbd.SelectedPath)));
                OnPropertyChanged("CurrentDirectories");
            }
        }

        private void Update()
        {
            _commandsList = ArrToOneStringConverter.ConvertBack(_commandInput).ToList();
            GenerateBreakPoints();
        }

        private void GenerateBreakPoints()
        {
            var linesCount = GetCharCount('\n') + 1;
            var isSimilarSizes = BreakPointsList.Count == linesCount;
            if(isSimilarSizes) return;

            var differences = BreakPointsList.Count - linesCount;

            if (differences > 0 && BreakPointsList.Count != 0)
            {
                for (var i = 0; i < differences; i++)
                {
                    BreakPointsList.RemoveAt(BreakPointsList.Count - 1);
                }
            }
            else 
            {
                for (var i = 0; i < -differences; i++)
                {
                    BreakPointsList.Add(new BreakPoint());
                }
            }

            OnPropertyChanged("BreakPointsList");
        }

        private int GetCharCount(char findingChar)
        {
            return CommandInput.Count(character => character == findingChar);
        }


        private void StepToNextBp()
        {
            if (_currentCommand >= _commandsList.Count) return;
            while (!BreakPointsList[_currentCommand].IsEnabled)
            {
                Step();
                if (!IsDebugMode)
                    break;
            }

            if (IsDebugMode)
                Step();
        }

        private void Step()
        {
            try
            {
                if (!BreakPointsList.Any()) return;

                var strCommand = _commandsList[_currentCommand];

                if (!string.IsNullOrEmpty(strCommand.Trim()))
                {
                    var bitCommand = Compiler.CommandToBit(strCommand);

                    var interpritatorResult = NumberCommandInterpritator.RunCommand(bitCommand, true);

                    ResultOutput += interpritatorResult.Key;
                    ErrorOutput += interpritatorResult.Value;
                }

            }
            catch (CompilerException ce)
            {
                ErrorOutput = "#Error: " + ce.Message + "-->" + ce.WrongCommand + " : Command № [" + ce.CommandNumber +
                              "]\n";
            }
            catch (Exception e)
            {
                ErrorOutput = e.Message;
            }

            _currentCommand++;
            if (_currentCommand >= _commandsList.Count)
            {
                EndDebug();
            }
        }

        private void EndDebug()
        {
            IsDebugMode = false;
            _currentCommand = 0;

        }

        private void TreeViewClick(object obj)
        {
            try
            {
                var fileSystemObjectInfo = obj as FileSystemObjectInfo;
                if (fileSystemObjectInfo?.FileSystemInfo.Extension == ".inw")
                {
                    _currentFilePath = fileSystemObjectInfo.FileSystemInfo;
                    CommandInput = MainMenuFunc.OpenFile(fileSystemObjectInfo.FileSystemInfo.FullName);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error in opening file");
            }
        }
    }
}

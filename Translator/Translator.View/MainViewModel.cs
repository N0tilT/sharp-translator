using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Windows;
using Translator.Core;
using Translator.View.ViewModel.Core;

namespace Translator.View.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        const string DosBoxApplicationPath = @"DOSBox\DOSBox.exe";
        const string DosBoxProgramData = @"DOSBox\data\";
        const string SourceFileName = "code";
        const string ProgramFileName = "compile";

        private RelayCommand? openFileCommand = null;
        public RelayCommand OpenFileCommand
        {
            get
            {
                return openFileCommand ??
                  (openFileCommand = new RelayCommand(obj =>
                  {
                      OpenFile();
                  }));
            }
        }

        private void OpenFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (Path.GetExtension(openFileDialog.FileName) != ".txt")
                {
                    MessageBox.Show("Пожалуйста, выберите текстовый файл (.txt)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                InputText = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private RelayCommand? saveFileCommand = null;
        public RelayCommand SaveFileCommand
        {
            get
            {
                return saveFileCommand ??
                  (saveFileCommand = new RelayCommand(obj =>
                  {
                      SaveFile();
                  }));
            }
        }

        private void SaveFile()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, InputText);
            }
        }

        private RelayCommand? compileAndRunCommand = null;
        public RelayCommand CompileAndRunCommand
        {
            get
            {
                return compileAndRunCommand ??
                  (compileAndRunCommand = new RelayCommand(obj =>
                  {
                      string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                      string sourceFilePath = Path.Combine(baseDirectory, SourceFileName + ".txt");
                      string compiledFilePath = Path.Combine(baseDirectory, ProgramFileName + ".asm");
                      File.WriteAllText(sourceFilePath, InputText);
                      try
                      {
                          var syntaxAnalyzer = new SyntaxAnalyzer();
                          syntaxAnalyzer.Compile(InputText);

                          var code = string.Join("\n", CodeGenerator.GetGeneratedCode());
                          OutputText = code;
                          File.WriteAllText(compiledFilePath, code);
                          RunDosBoxTest(ProgramFileName, code);
                      }
                      catch (Exception ex)
                      {
                          OutputText = ex.Message;
                      }
                  }));
            }
        }
        // Запуск теста в DOSBox
        void RunDosBoxTest(string fileName, string code)
        {
            if (!File.Exists(DosBoxProgramData + fileName + ".asm"))
            {
                throw new FileNotFoundException("Не найден файл: " + DosBoxProgramData + fileName + ".asm");
            }

            File.WriteAllText(DosBoxProgramData + fileName + ".asm", code);
            var mountData = @"mount D " + DosBoxProgramData.Remove(DosBoxProgramData.Length - 1);
            var masmData = "MASM.EXE " + fileName + ".asm" + " " + fileName + ".obj" + " " + fileName + ".lst" + " " + fileName + ".crf";
            var linkData = "LINK.EXE " + fileName + ".obj" + "," + fileName + ".exe" + "," + fileName + ".map" + "," + "/NODEFAULTLIB";
            var programLauch = fileName + ".exe";
            var psi = new ProcessStartInfo
            {
                FileName = DosBoxApplicationPath,
                Arguments = $"-c \"{mountData}\" -c D: -c \"{masmData}\" -c \"{linkData}\" -c " + programLauch,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            Process.Start(psi);
        }

        private string _inputText;
        private string _outputText;

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged(nameof(OutputText));
            }
        }
    }
}

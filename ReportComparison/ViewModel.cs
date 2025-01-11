using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ReportComparison
{
    internal class ViewModel
    {
        public ViewModel()
        {
            _model = new Model();
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Model.WinTitle = "报表比对工具 - Chengdu Team "+version.ToString();
            Model.Profiles = new ObservableCollection<Profile>(Profile.ReadAllProfiles());
            if (Model.Profiles != null && Model.Profiles.Count > 0)
                Model.SelectedProfile = Model.Profiles[0];
        }

        private Model _model;
        public Model Model
        {
            get { return _model; }
        }

        public ICommand LeftFileBrowseCmd
        {
            get => new RelayCommand(BrowseLeftFile);
        }

        private void BrowseLeftFile(Object obj)
        {
            BrowseFile(true);
        }

        public ICommand RightFileBrowseCmd
        {
            get => new RelayCommand(BrowseRightFile);
        }

        private void BrowseRightFile(Object obj)
        {
            BrowseFile(false);
        }

        private void BrowseFile(bool fistPath)
        {
            var dlg = new OpenFileDialog { Multiselect = false };
            if (dlg.ShowDialog() == true)
            {
                if (fistPath)
                    Model.FirstPath = dlg.FileName;
                else
                    Model.SecondPath = dlg.FileName;
            }
        }

        public ICommand CompareCmd
        {
            get => new RelayCommand(Compare);
        }

        private void Compare(Object obj)
        {
            Profile selectedProfile = Model.SelectedProfile;
            var firstContentList = ReadFile(Model.FirstPath, true);
            var secondContentList = ReadFile(Model.SecondPath, false);

            var allKeys = GetAllKeys(firstContentList, secondContentList);
            var dt = new DataTable();
            AddTitiles(dt);

            foreach (var key in allKeys)
            {
                var rowContent = new List<string>();
                List<string> firstGuiContent = new List<string>();
                List<string> secondGuiContent = new List<string>();

                AddDispalyContentByFileContent(firstContentList, firstGuiContent, key, selectedProfile.FirstFileReadStrategy.ColumnNames.Count);
                AddDispalyContentByFileContent(secondContentList, secondGuiContent, key, selectedProfile.FirstFileReadStrategy.ColumnNames.Count);

                rowContent.AddRange(firstGuiContent);
                rowContent.AddRange(secondGuiContent);
                bool hasDiff = false;
                foreach (var calcuateIndex in Model.SelectedProfile.CompareStrategyCalculateColumnIndexs)
                {
                    decimal firstValue = 0;
                    bool firstValueParseSucc = false;
                    decimal secondValue = 0;
                    bool secondValueParseSucc = false;
                    if (firstGuiContent.Count > calcuateIndex && !string.IsNullOrEmpty(firstGuiContent[calcuateIndex]))
                    {
                        firstValueParseSucc = decimal.TryParse(firstGuiContent[calcuateIndex], out firstValue);
                    }
                    if (secondGuiContent.Count > calcuateIndex && !string.IsNullOrEmpty(secondGuiContent[calcuateIndex]))
                    {
                        secondValueParseSucc = decimal.TryParse(secondGuiContent[calcuateIndex], out secondValue);
                    }

                    if (firstValueParseSucc && secondValueParseSucc)
                    {
                        var diffVal = firstValue - secondValue;
                        rowContent.Add(diffVal.ToString());
                        if (diffVal.CompareTo(decimal.Zero) != 0)
                            hasDiff = true;
                    }
                    else
                    {
                        rowContent.Add("/");
                        hasDiff = true;
                    }
                }
                if (!Model.OnlyShowDiff || hasDiff)
                    dt.Rows.Add(rowContent.ToArray());
            }
            Model.Result = dt;
        }

        private Dictionary<string, List<string>> ReadFile(string path, bool firstPath)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            Profile selectedProfile = Model.SelectedProfile;
            FileReadStrategy fileReadStrategy = firstPath ? selectedProfile.FirstFileReadStrategy : selectedProfile.SecondFileReadStrategy;

            string splitChar = GetSpiltChar(fileReadStrategy);

            var reportData = File.ReadAllText(path, Encoding.GetEncoding(fileReadStrategy.Encoding)).Trim();

            var lines = Regex.Split(reportData, "\r\n|\r|\n");
            for (int i = 0; i < lines.Length; i++)
            {
                // Since the IgnoreXXXRowCount is One-Based, here is i+1
                if (i + 1 <= fileReadStrategy.IgnoreHeadRowCount) continue;
                if (i + 1 > lines.Length - fileReadStrategy.IgnoreTailRowCount) continue;

                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                StringBuilder key = new StringBuilder();
                List<string> contents = new List<string>();
                int columnIndex = 0;
                foreach (var s in Regex.Split(line, splitChar))
                {
                    if (fileReadStrategy.KeyColumnIndexs.Contains(columnIndex))
                        key.Append(s + "-");

                    if (fileReadStrategy.ColumnIndexs.Contains(columnIndex))
                        contents.Add(s);

                    columnIndex++;
                }
                result.Add(key.ToString(), contents);
            }
            return result;
        }

        private static string GetSpiltChar(FileReadStrategy fileReadStrategy)
        {
            string splitChar = "\t";
            switch (fileReadStrategy.Splitter)
            {
                case Splitter.Tab:
                    splitChar = "\t"; break;
                case Splitter.Space:
                    splitChar = @"\s+"; break;
                case Splitter.Comma:
                    splitChar = ","; break;
            }

            return splitChar;
        }

        private List<string> GetAllKeys(Dictionary<string, List<string>> firstContent, Dictionary<string, List<string>> secondContent)
        {
            HashSet<string> keys = new HashSet<string>();
            keys.UnionWith(firstContent.Keys);
            keys.UnionWith(secondContent.Keys);
            var allKeys = keys.ToList();
            allKeys.Sort();
            return allKeys;
        }

        private void AddTitiles(DataTable dt)
        {
            foreach (var name in Model.SelectedProfile.FirstFileReadStrategy.ColumnNames)
                dt.Columns.Add(name);
            foreach (var name in Model.SelectedProfile.SecondFileReadStrategy.ColumnNames)
                dt.Columns.Add(name);
            foreach (var name in Model.SelectedProfile.CompareStrategyAppendColumnNames)
                dt.Columns.Add(name);
        }

        private void AddDispalyContentByFileContent(Dictionary<string, List<string>> contentList, List<string> guiContent, string key, int columnCount)
        {
            if (contentList.ContainsKey(key))
            {
                guiContent.AddRange(contentList[key]);
            }
            else
            {
                Enumerable.Range(0, columnCount).ToList().ForEach(x => guiContent.Add(""));
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => (_canExecute == null) ? true : _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

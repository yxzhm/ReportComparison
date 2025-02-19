using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ReportComparison
{
    enum Splitter
    {
        Tab,
        Space,
        Comma
    }

    internal class Profile : INotifyPropertyChanged
    {
        private FileReadStrategy firstFileReadStrategy;
        private FileReadStrategy secondFileReadStrategy;
        public Profile(string name, Dictionary<string, List<KeyValuePair<string, string>>> table)
        {
            _name = name;
            _table = table;
            firstFileReadStrategy = new FileReadStrategy("FirstFileReadStrategy", _table);
            secondFileReadStrategy = new FileReadStrategy("SecondFileReadStrategy", _table);
        }

        private string _name;
        public string Name
        {
            get { return _name; }
        }

        private Dictionary<string, List<KeyValuePair<string, string>>> _table;
        public Dictionary<string, List<KeyValuePair<string, string>>> TomlTable
        {
            get { return _table; }
        }
        #region GUI
        public string FirstFileTitle
        {
            get
            {
                return GetValue(_table, "GUI", nameof(FirstFileTitle));
            }
        }

        public string SecondFileTitle
        {
            get
            {
                return GetValue(_table, "GUI", nameof(SecondFileTitle));
            }
        }
        #endregion

        #region FirstFileReadStrategy
        public FileReadStrategy FirstFileReadStrategy
        {
            get => firstFileReadStrategy;
        }
        #endregion

        #region SecondFileReadStrategy
        public FileReadStrategy SecondFileReadStrategy
        {
            get => secondFileReadStrategy;
        }

        #endregion

        #region CompareStrategy
        public List<string> CompareStrategyAppendColumnNames
        {
            get
            {
                var value = GetValue(_table, "CompareStrategy", "AppendColumnNames");
                return GetKVStrArray(value);
            }
        }

        public List<int> CompareStrategyCalculateColumnIndexs
        {
            get
            {
                var value = GetValue(_table, "CompareStrategy", "CalculateColumnIndexs");
                return GetKVIntArray(value);
            }
        }
        #endregion

        public bool Validate()
        {
            if (string.IsNullOrEmpty(FirstFileTitle)) return false;
            if (string.IsNullOrEmpty(SecondFileTitle)) return false;

            if (!ValidateFileReadStrategy(firstFileReadStrategy)) return false;
            if (!ValidateFileReadStrategy(secondFileReadStrategy)) return false;

            if (CompareStrategyAppendColumnNames == null) return false;

            if (CompareStrategyCalculateColumnIndexs == null || CompareStrategyCalculateColumnIndexs.Count == 0) return false;

            if (CompareStrategyCalculateColumnIndexs.Count != CompareStrategyAppendColumnNames.Count) return false;

            // if the two index are not equivalent, the calcuateColumnIndexs are incorrect
            if (firstFileReadStrategy.ColumnIndexs.Count!=secondFileReadStrategy.ColumnIndexs.Count) return false;

            if (CompareStrategyCalculateColumnIndexs.Distinct().Count() != CompareStrategyCalculateColumnIndexs.Count) return false;

            // the append index can't larger than index of report files, otherwise can't calulate the two number of report files
            if (CompareStrategyCalculateColumnIndexs.Any(x => x >= firstFileReadStrategy.ColumnNames.Count)) return false;

            if (CompareStrategyCalculateColumnIndexs.Any(x => x >= secondFileReadStrategy.ColumnNames.Count)) return false;

            return true;
        }

        private bool ValidateFileReadStrategy(FileReadStrategy fileReadStrategy)
        {
            if (fileReadStrategy == null) return false;

            if (string.IsNullOrEmpty(fileReadStrategy.Encoding)) return false;

            if (fileReadStrategy.ColumnNames == null || fileReadStrategy.ColumnIndexs == null) return false;

            if (fileReadStrategy.ColumnNames.Count == 0) return false;

            if (fileReadStrategy.ColumnNames.Count != fileReadStrategy.ColumnIndexs.Count) return false;

            // each index is unique
            if (fileReadStrategy.ColumnIndexs.Distinct().Count() != fileReadStrategy.ColumnIndexs.Count) return false;

            return true;
        }

        public static List<Profile> ReadAllProfiles()
        {
            List<Profile> list = new List<Profile>();
            var profileFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles"));
            foreach (FileInfo profileFile in profileFolder.GetFiles())
            {
                var profileStr = File.ReadAllText(profileFile.FullName);
                var model = new Dictionary<string, List<KeyValuePair<string, string>>>();
                string key = "";
                var kvs = new List<KeyValuePair<string, string>>();
                var lines = Regex.Split(profileStr, "\r\n|\r|\n");
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        if (i != 0 && !string.IsNullOrEmpty(key)) model[key] = kvs;

                        key = line.Replace("[", "").Replace("]", "");
                        kvs = new List<KeyValuePair<string, string>>();
                        continue;
                    }

                    if (line.Contains("="))
                    {
                        var values = line.Split('=');
                        if (values.Length == 2)
                        {
                            var kv = new KeyValuePair<string, string>(values[0], values[1]);
                            kvs.Add(kv);
                        }
                    }

                    if (i == lines.Length - 1)
                    {
                        model[key] = kvs;
                    }

                }
                var profile = new Profile(profileFile.Name.Replace(".txt", ""), model);
                if (profile.Validate())
                    list.Add(profile);
            }
            return list;
        }

        public static string GetValue(Dictionary<string, List<KeyValuePair<string, string>>> dic, string dicKey, string key)
        {
            if (dic == null || string.IsNullOrEmpty(dicKey) || string.IsNullOrEmpty(key)) return null;

            if (!dic.ContainsKey(dicKey)) return null;

            var kvs = dic[dicKey];

            foreach (var kv in kvs)
            {
                if (kv.Key.Trim() == key) return kv.Value.Replace("\"", "").Trim();
            }
            return null;
        }

        public static List<string> GetKVStrArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            List<string> result = new List<string>();
            foreach (var v in value.Replace("[", "").Replace("]", "").Split(','))
            {
                result.Add(v.Replace("\"", "").Trim());
            }
            return result;
        }

        public static List<int> GetKVIntArray(string value)
        {
            var list = GetKVStrArray(value);
            if (list == null) return null;

            return list.Select(x => int.Parse(x)).ToList();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class FileReadStrategy
    {
        private readonly string keyWord;
        private readonly Dictionary<string, List<KeyValuePair<string, string>>> table;
        public FileReadStrategy(string keyWord, Dictionary<string, List<KeyValuePair<string, string>>> table)
        {
            this.keyWord = keyWord;
            this.table = table;
        }
        public string Encoding
        {
            get
            {
                return Profile.GetValue(table, keyWord, nameof(Encoding));
            }
        }


        public Splitter Splitter
        {
            get
            {
                var val = Profile.GetValue(table, keyWord, nameof(Splitter));
                switch (val.Trim())
                {
                    case "Tab": return Splitter.Tab;
                    case "Space": return Splitter.Space;
                    case "Comma": return Splitter.Comma;
                    default: return Splitter.Comma;
                }

            }
        }

        public List<string> ColumnNames
        {
            get
            {
                var val = Profile.GetValue(table, keyWord, nameof(ColumnNames));
                return Profile.GetKVStrArray(val); ;
            }
        }

        public List<int> ColumnIndexs
        {
            get
            {
                var val = Profile.GetValue(table, keyWord, nameof(ColumnIndexs));
                return Profile.GetKVIntArray(val); ;
            }
        }


        public List<int> KeyColumnIndexs
        {
            get
            {
                var val = Profile.GetValue(table, keyWord, nameof(KeyColumnIndexs));
                return Profile.GetKVIntArray(val); ;
            }
        }
    }
}

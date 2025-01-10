using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn.Syntax;

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
        public Profile(string name, TomlTable table)
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

        private TomlTable _table;
        public TomlTable TomlTable
        {
            get { return _table; }
        }
        #region GUI
        public string FirstFileTitle
        {
            get
            {
                object obj = "";
                (TomlTable["GUI"] as TomlTable).TryGetValue(nameof(FirstFileTitle), out obj);
                return obj.ToString();
            }
        }

        public string SecondFileTitle
        {
            get
            {
                object obj = "";
                (TomlTable["GUI"] as TomlTable).TryGetValue(nameof(SecondFileTitle), out obj);
                return obj.ToString();
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
                object obj;
                (TomlTable["CompareStrategy"] as TomlTable).TryGetValue("AppendColumnNames", out obj);
                return (obj as TomlArray).Select(x => x.ToString()).ToList();
            }
        }

        public List<int> CompareStrategyCalculateColumnIndexs
        {
            get
            {
                object obj;
                (TomlTable["CompareStrategy"] as TomlTable).TryGetValue("CalculateColumnIndexs", out obj);
                return (obj as TomlArray).Select(x => int.Parse(x.ToString())).ToList();
            }
        }
        #endregion


        public static List<Profile> ReadAllProfiles()
        {
            List<Profile> list = new List<Profile>();
            var profileFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles"));
            foreach (FileInfo profileFile in profileFolder.GetFiles())
            {
                var profileStr = File.ReadAllText(profileFile.FullName);
                TomlTable model = Tomlyn.Toml.ToModel(profileStr);
                list.Add(new Profile(profileFile.Name.Replace(".txt", ""), model));
            }
            return list;
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
        private readonly TomlTable table;
        public FileReadStrategy(string keyWord, TomlTable table)
        {
            this.keyWord = keyWord;
            this.table = table;
        }
        public string Encoding
        {
            get
            {
                object obj;
                (table[keyWord] as TomlTable).TryGetValue("Encoding", out obj);
                return obj.ToString();
            }
        }

        public int IgnoreHeadRowCount
        {
            get
            {
                object obj;
                (table[keyWord] as TomlTable).TryGetValue("IgnoreHeadRowCount", out obj);
                return int.Parse(obj.ToString());
            }
        }

        public int IgnoreTailRowCount
        {
            get
            {
                object obj;
                (table[keyWord] as TomlTable).TryGetValue("IgnoreTailRowCount", out obj);
                return int.Parse(obj.ToString());
            }
        }

        public Splitter Splitter
        {
            get
            {
                object obj;
                (table[keyWord] as TomlTable).TryGetValue("Splitter", out obj);
                switch (obj.ToString().Trim())
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
                object obj;
                (table[keyWord] as TomlTable).TryGetValue("ColumnNames", out obj);
                return (obj as TomlArray).Select(x => x.ToString()).ToList();
            }
        }

        public List<int> ColumnIndexs
        {
            get
            {
                object obj;
                (table[keyWord] as TomlTable).TryGetValue("ColumnIndexs", out obj);
                return (obj as TomlArray).Select(x => int.Parse(x.ToString())).ToList();
            }
        }


        public List<int> KeyColumnIndexs
        {
            get
            {
                object obj;
                (table[keyWord] as TomlTable).TryGetValue("KeyColumnIndexs", out obj);
                return (obj as TomlArray).Select(x => int.Parse(x.ToString())).ToList();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportComparison
{
    internal class Model : INotifyPropertyChanged
    {

        public Model()
        {
            Profiles = new ObservableCollection<Profile>();
            Result = new DataTable("Result");
        }

        private ObservableCollection<Profile> _profiles;
        public ObservableCollection<Profile> Profiles
        {
            get { return _profiles; }
            set
            {
                _profiles = value;
                OnPropertyChanged();
            }
        }

        private Profile _selectedProfile;
        public Profile SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private string _leftPath;
        public string FirstPath
        {
            get { return _leftPath; }
            set
            {
                _leftPath = value;
                OnPropertyChanged();
            }
        }

        private string _rightPath;
        public String SecondPath
        {
            get { return _rightPath; }
            set
            {
                _rightPath = value;
                OnPropertyChanged();
            }
        }


        private DataTable _result;
        public DataTable Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
                OnPropertyChanged("DataView");
            }
        }

        public DataView DataView
        {
            get => Result.DefaultView;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

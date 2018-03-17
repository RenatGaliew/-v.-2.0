using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Спектральный_анализ_v._2._0
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ICommand StartCommand { get; set; }
        private WorkingPlace _workPlace;
        private string _filePath;
        private BitmapSource _selectedImage;
        private string _selectedFileName;
        private int _cCube;
        private List<string> _filePathes;
        private ObservableCollection<string> _filesNames;
        public event PropertyChangedEventHandler PropertyChanged;
        public string FilesPath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilesPath));
            }
        }
        public List<string> FilesPathes
        {
            get => _filePathes;
            set
            {
                _filePathes = value;
                OnPropertyChanged(nameof(FilesPathes));
            }
        }
        public ObservableCollection<string> FilesNames
        {
            get => _filesNames;
            set
            {
                _filesNames = value;
                OnPropertyChanged(nameof(FilesNames));
            }
        }
        public BitmapSource SelectedImage
        {
            get => _selectedImage;
            set
            {
                _selectedImage = value;
                OnPropertyChanged(nameof(SelectedImage));
            }
        }

        public string SelectedFileName
        {
            get => _selectedFileName;
            set
            {
                _selectedFileName = value;
                OnPropertyChanged(nameof(SelectedFileName));
            }
        }
        public int CountCube
        {
            get => _cCube;
            set
            {
                _cCube = value;
                OnPropertyChanged(nameof(CountCube));
            }
        }
        public object ViewerCommand { get; set; }
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (prop == nameof(SelectedFileName))
            {
                int i = FilesNames.IndexOf(SelectedFileName);
                SelectedImage = new BitmapImage(new Uri(FilesPathes[i]));
            }
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public ViewModel()
        {
            FilesPath = Application.StartupPath;
            _workPlace = new WorkingPlace(FilesPath);
            FilesPathes = _workPlace.FilesPaths;
            FilesNames = _workPlace.FilesNames;

            ViewerCommand = new RelayCommand(ViewPapks);
            StartCommand = new RelayCommand(Start);
        }

        private async void Start()
        {
            await Task.Run(() =>
            {
                _workPlace?.Start(_cCube);
            });
        }

        private void ViewPapks()
        {
            _workPlace.NewfilePath();
            FilesPath = _workPlace.Path;
            FilesPathes = _workPlace.FilesPaths;
            FilesNames = _workPlace.FilesNames;
        }
    }
}

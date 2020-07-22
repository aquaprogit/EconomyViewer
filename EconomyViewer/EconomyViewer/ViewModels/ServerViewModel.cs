using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EconomyViewer.Utils;

namespace EconomyViewer.ViewModels
{
    public class ServerViewModel : ViewModelBase
    {
        public string Server
        {
            get => App.Server;
            set
            {
                App.Server = value;
                OnPropertyChanged("Server");
            }
        }
        public List<string> Servers
        {
            get => DataBaseWorker.GetAllTables().OrderBy(c => c).ToList();
        }
        public List<string> Mods
        {
            get => DataBaseWorker.GetOnlyColumnList(App.Server, "i_mod");
        }
        public ServerViewModel()
        {
            DataBaseWorker.DataChanged += DataBaseWorker_DataChanged;
        }

        private void DataBaseWorker_DataChanged()
        {
            OnPropertyChanged("Mods");
        }
    }
}

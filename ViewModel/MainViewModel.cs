using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSSA.Model;
using SSSA.Socket;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using Telerik.Windows.Controls;

namespace SSSA.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            InitiCommand();
        }
        private HxServer server;
        //异常or提示信息集合
        private ObservableCollection<ExceptionModel> _exceptionModels = new ObservableCollection<ExceptionModel>();
        private string _port = "2020";
        private static bool CanExecute(object o)
        {
            return true;
        }

        //RadMenuItem Click Command
        public DelegateCommand RmcCommand { get; set; }

        //Windows Close Event
        public DelegateCommand ClosedCommand { get; set; }

        //GridView 右键菜单Item Click
        public DelegateCommand GridMenuCommand { get; set; }

        //busy binding
        private bool _isBusy;

        private int _sessionCount;

        public bool IsBusy
        {
            get { return _isBusy; }

            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        /// <summary>
        /// 初始化Command
        /// </summary>
        private void InitiCommand()
        {
            RmcCommand = new DelegateCommand(RadMenuItemClick, CanExecute);
            ClosedCommand = new DelegateCommand(WindowClosed, CanExecute);
            GridMenuCommand = new DelegateCommand(GridMenuItemClick, CanExecute);
        }

        private void RadMenuItemClick(object obj)
        {
            if (obj == null)
            {
                return;
            }
            var compara = obj.ToString();
            if (compara.Equals("启动"))
            {
                StartServer();
                WriteLog("启动", ExEnum.Infor);
            }
            else if (compara.Equals("停止"))
            {
                server?.Stop();
                WriteLog("停止", ExEnum.Infor);
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        private void StartServer()
        {
            server = new HxServer();
            ServerConfig config = new ServerConfig();
            config.Ip = "192.168.1.4";
            config.Port = Convert.ToInt32(Port);
            config.TextEncoding = "UTF-8";
            config.MaxConnectionNumber = 6000;
            //Setup the appServer
            if (!server.Setup(config)) //Setup with listening port
            {
                WriteLog("Failed to setup!", ExEnum.Infor);
                return;
            }
            //新连接处理
            server.NewSessionConnected += Server_NewSessionConnected;
            //请求处理
            server.NewRequestReceived += Server_NewRequestReceived;

            server.SessionClosed += ServerOnSessionClosed;
            server.Start();
        }

        private void ServerOnSessionClosed(HxSession session, CloseReason value)
        {
            SessionCount = server.SessionCount;
        }

        private void Server_NewRequestReceived(HxSession session, StringRequestInfo requestInfo)
        {
//            WriteLog(requestInfo.Key, ExEnum.Infor);
            session.Send("rerequestInfo1231#");
        }

        private void Server_NewSessionConnected(HxSession session)
        {
            SessionCount = server.SessionCount;
//            WriteLog(session.LocalEndPoint.ToString(), ExEnum.Infor);
        }

        private void GridMenuItemClick(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }
                var compara = obj.ToString();
                if (compara.Equals("清空"))
                {
                    ExceptionModels?.Clear();
                }
//                else if (compara.Equals("解密"))
//                {
//                    if (string.IsNullOrEmpty(SelectJkd.JKD_VALUE))
//                    {
//                        return;
//                    }
//                    if (SelectJkd.JKD_VALUE.TrimEnd('#').Length == 0)
//                    {
//                        return;
//                    }
//                    DeCodeWindow dcw = new DeCodeWindow();
//                    var deStr = DataPacketCodec.Decode(SelectJkd.JKD_VALUE.TrimEnd('#'), Settings.CryptKey);
//                    dcw.Tb1.Text = deStr;
//                    dcw.ShowDialog();
//                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message, ExEnum.Error);
            }
        }

        private void WindowClosed(object obj)
        {
//            _taskFlag = false;
        }
        public string Port
        {
            get
            {
                return _port;
            }

            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        /// <summary>
        /// 添加log到集合中显示
        /// </summary>
        /// <param name="paramStr">log信息</param>
        /// <param name="paramLevel">log级别</param>
        public void WriteLog(string paramStr, ExEnum paramLevel)
        {
            string level = string.Empty;
            if (paramLevel == ExEnum.Infor)
            {
                level = "提示";
            }
            if (paramLevel == ExEnum.Error)
            {
                level = "异常";
            }
            if (!string.IsNullOrEmpty(level))
            {
                ExceptionModels.Add(new ExceptionModel()
                {
                    ExTime = DateTime.Now,
                    ExLevel = level,
                    ExMessage = paramStr
                });
            }
        }

        //异常or提示信息集合
        public ObservableCollection<ExceptionModel> ExceptionModels
        {
            get { return _exceptionModels; }

            set
            {
                _exceptionModels = value;
                OnPropertyChanged("ExceptionModels");
            }
        }

        public int SessionCount
        {
            get
            {
                return _sessionCount;
            }

            set
            {
                _sessionCount = value;
                OnPropertyChanged("SessionCount");
            }
        }
    }
}

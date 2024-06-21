using Caliburn.Micro;
using HalconDotNet;
using HPSocket;
using HPSocket.Tcp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PropertyChanged;
using ScanKitWpf.DataReceiveAdapter;
using ScanKitWpf.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ScanKitWpf.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel : Screen
    {
        readonly AppConfig _config;
        readonly ILogger _logger;
        public string Title { get; set; }

        public ObservableCollection<CodeConfig> ScanCodeConfig { get; set; }

        private bool barCodeChecked;

        StringBuilder stringBuilder = new StringBuilder();
        static HObject ho_Image = null;
        HObject ho_SybolRegions = null;
        HObject ho_QRSybolRegions = null;
        HObject ho_DMSybolRegions = null;
        static HTuple hv_AcqHandle = null;
        HSmartWindowControlWPF hWindow = null;
        List<CodeInfo> codeInfos = new List<CodeInfo>();

        HTuple hv_Width = new HTuple();
        HTuple hv_Height = new HTuple();

        HTuple hv_BarCodeHandle = -1;
        HTuple hv_BarCodeType;
        HTuple hv_DecodeStrings = new HTuple();
        HTuple hv_DecodeTypes = new HTuple();
        HTuple hv_Area = new HTuple();
        HTuple hv_Row = new HTuple();
        HTuple hv_Column = new HTuple();

        HTuple hv_QRCodeHandle = -1;
        HTuple hv_QRCodeResultHandle = new HTuple();
        HTuple hv_QRArea = new HTuple();
        HTuple hv_QRRow = new HTuple();
        HTuple hv_QRColumn = new HTuple();
        HTuple hv_QRPointOrder = new HTuple();

        HTuple hv_DMCodeHandle = -1;
        HTuple hv_DMCodeResultHandle = new HTuple();
        HTuple hv_DMArea = new HTuple();
        HTuple hv_DMRow = new HTuple();
        HTuple hv_DMColumn = new HTuple();
        HTuple hv_DMPointOrder = new HTuple();


        Stopwatch sw = new Stopwatch();

        readonly ITcpServer<string> tcpServer;

        TextBox txtMsg;
        public StringBuilder sbMsg;
        public string rtxtMsg { get; set; }

        public string ImagePath { get; set; }

        private bool _cameraConnected;
        public bool CameraConnected
        {
            get { return _cameraConnected; }
            set { _cameraConnected = value; }
        }


        public MainViewModel(IOptionsMonitor<AppConfig> config, ILogger logger)
        {
            _config = config.CurrentValue;
            ScanCodeConfig = _config.ScanCodeConfig;
            _logger = logger;
            Title = _config.Title;
            var barCodeTypes = new List<string>();
            foreach (var barCodeType in ScanCodeConfig)
            {
                if (barCodeType.IsChecked && (barCodeType.CodeType == "Code 39" || barCodeType.CodeType == "Code 93" || barCodeType.CodeType == "Code 128"))
                {
                    barCodeTypes.Add(barCodeType.CodeType);
                }
            }
            if (barCodeTypes.Count > 0)
            {
                barCodeChecked = true;
            }
            hv_BarCodeType = barCodeTypes.ToArray();
            sbMsg = new StringBuilder();

            tcpServer = new TcpServer<string>();
            tcpServer.SocketBufferSize = 4096;
            tcpServer.Address = _config.ServerIp;
            tcpServer.Port = _config.ServerPort;
            tcpServer.DataReceiveAdapter = new TextDataReceiveAdapter();

            tcpServer.OnPrepareListen += TcpServer_OnPrepareListen;
            tcpServer.OnAccept += TcpServer_OnAccept;
            tcpServer.OnParseRequestBody += TcpServer_OnParseRequestBody;
            tcpServer.OnSend += TcpServer_OnSend;
            tcpServer.OnClose += TcpServer_OnClose;
            tcpServer.OnShutdown += TcpServer_OnShutdown;

        }

        private HandleResult TcpServer_OnPrepareListen(IServer sender, nint listen)
        {
            AddMsg($"相机服务启动监听， 监听端口：{sender.Port}");
            return HandleResult.Ok;
        }

        private HandleResult TcpServer_OnAccept(IServer sender, nint connId, nint client)
        {
            sender.GetRemoteAddress(connId, out string ip, out ushort port);
            AddMsg($"客户端：{ip},端口：{port} 建立连接");
            return HandleResult.Ok;
        }

        private HandleResult TcpServer_OnParseRequestBody(ITcpServer sender, nint connId, string obj)
        {
            AddMsg($"接收到消息:{obj}");


            if (string.Compare(obj, _config.Trigger.TriggerCommand, true) == 0)
            {
                AddMsg($"触发拍照解码");
                try
                {
                    OneGrab();
                    AddMsg($"解析耗时：{sw.ElapsedMilliseconds}毫秒");
                    //var materialInfo = ParseCode();
                    var sendData = JsonSerializer.Serialize(codeInfos);
                    var sendBytes = Encoding.UTF8.GetBytes(sendData+"\r\n");
                    sender.Send(connId, sendBytes, sendBytes.Length);

                }
                catch (Exception ex)
                {
                    AddMsg($"解码出错:{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
            return HandleResult.Ok;
        }

        private HandleResult TcpServer_OnSend(IServer sender, nint connId, byte[] data)
        {
            AddMsg($"发送消息:{Encoding.UTF8.GetString(data)}");
            return HandleResult.Ok;
        }

        private HandleResult TcpServer_OnClose(IServer sender, nint connId, SocketOperation socketOperation, int errorCode)
        {
            AddMsg($"connId:{connId}, socketOperation:{socketOperation}, errorCode:{errorCode}");
            return HandleResult.Ok;
        }

        private HandleResult TcpServer_OnShutdown(IServer sender)
        {
            return HandleResult.Ok;
        }

        public bool CanOpenCamera { get; set; } = true;
        public bool CanOneGrap { get; set; } = false;
        public bool CanCloseCamera { get; set; } = false;

        public void OpenCamera()
        {
            HOperatorSet.GenEmptyObj(out ho_Image);
            try
            {
                HOperatorSet.OpenFramegrabber("MVision", 1, 1, 0, 0, 0, 0, "progressive", 8, "default", -1, "false",
                    "auto", _config.Camera.Name, 0, -1, out hv_AcqHandle);
                CanOpenCamera = false;
                CanOneGrap = true;
                CanCloseCamera = true;
                SetCameraParam();
                HOperatorSet.CreateBarCodeModel(new HTuple(), new HTuple(), out hv_BarCodeHandle);
                //SetBarCodeParam();
                HOperatorSet.CreateDataCode2dModel("QR Code", null, null, out hv_QRCodeHandle);
                //SetQRCodeParam();
                HOperatorSet.CreateDataCode2dModel("Data Matrix ECC 200", null, null, out hv_DMCodeHandle);
                //SetDMCodeParam();


                // 启动相机采集
                HOperatorSet.GrabImageStart(hv_AcqHandle, -1);

                tcpServer.Start();

                //HOperatorSet.WaitSeconds(0.01);
                //HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "相机错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OneGrab()
        {
            sw.Restart();
            codeInfos.Clear();
            SoftTriggerAndCaptureImage();
            sw.Stop();
            //MessageBox.Show($"解析耗时:{sw.ElapsedMilliseconds}毫秒");
            //AddMsg(JsonSerializer.Serialize(codeInfos));
            //sbMsg.AppendLine(codeInfos.DumpText($"解析耗时:{sw.ElapsedMilliseconds}毫秒", tableConfig:new TableConfig { ShowTableHeaders=false,ShowMemberTypes=false}));
            AddMsg($"解析数据：{JsonSerializer.Serialize(codeInfos)}， 耗时:{sw.ElapsedMilliseconds}毫秒");
        }



        public void CloseCamera()
        {
            if (ho_Image != null)
            {
                ho_Image.Dispose();
            }
            HOperatorSet.CloseFramegrabber(hv_AcqHandle);
            CanOpenCamera = true;
            CanOneGrap = false;
            CanCloseCamera = false;

            tcpServer.Stop();
        }

        public void SaveConfig()
        {
            var configStr = JsonSerializer.Serialize(_config);
            File.WriteAllText("appsettings.json", configStr);

        }

        public void ManualAnalyse()
        {
            codeInfos.Clear();
            if (!File.Exists(ImagePath))
            {
                MessageBox.Show("文件不存在，请输入有效的图片路径");
                return;
            }
            if (hv_BarCodeHandle == null || hv_BarCodeHandle.Type != HTupleType.HANDLE)
            {
                HOperatorSet.CreateBarCodeModel(new HTuple(), new HTuple(), out hv_BarCodeHandle);
            }
            if (hv_QRCodeHandle == null || hv_QRCodeHandle.Type != HTupleType.HANDLE)
            {
                HOperatorSet.CreateDataCode2dModel("QR Code", null, null, out hv_QRCodeHandle);
            }
            if (hv_DMCodeHandle == null || hv_DMCodeHandle.Type != HTupleType.HANDLE)
            {
                HOperatorSet.CreateDataCode2dModel("Data Matrix ECC 200", "default_parameters", "enhanced_recognition", out hv_DMCodeHandle);
            }
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.ReadImage(out ho_Image, ImagePath);
            sw.Restart();
            AnalyseCode();
            sw.Stop();
            AddMsg($"手动识别， 耗时：{sw.ElapsedMilliseconds} 毫秒");
            AddMsg($"{JsonSerializer.Serialize(codeInfos)}");
        }

        private void SetCameraParam()
        {
            //HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "TriggerSelector", "FrameBurstStart");//设置帧触发

            HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "TriggerMode", "Off");
            //HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "external_trigger", "true");
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "TriggerSource", "Software");
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "grab_timeout", 2000);
            HOperatorSet.SetFramegrabberParam(hv_AcqHandle, "ExposureTime", _config.Camera.ExposeTime);//设置曝光值
        }

        private void SetBarCodeParam()
        {
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "element_size_min", 0.6);
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "check_char", "present");
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "min_identical_scanlines", 2);
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "contrast_min", 5);
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "persistence", 1);
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "start_stop_tolerance", "low");
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "stop_after_result_num", 0);
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "meas_thresh_abs", 0);
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "meas_thresh", 0.2);//* 0.5
            HOperatorSet.SetBarCodeParam(hv_BarCodeHandle, "num_scanlines", 10);//* 50
        }

        private void SetQRCodeParam()
        {
            HOperatorSet.SetDataCode2dParam(hv_QRCodeHandle, "polarity", "dark_on_light");
            //HOperatorSet.SetDataCode2dParam(hv_QRCodeHandle, "string_encoding", "utf8");
            //HOperatorSet.SetDataCode2dParam(hv_QRCodeHandle, "stop_after_result_num", 10);
        }

        private void SetDMCodeParam()
        {
            HOperatorSet.SetDataCode2dParam(hv_DMCodeHandle, "polarity", "dark_on_light");
            //HOperatorSet.SetDataCode2dParam(hv_DMCodeHandle, "small_modules_robustness", "high");
            //HOperatorSet.SetDataCode2dParam(hv_DMCodeHandle, "module_size_min", 4);
            //HOperatorSet.SetDataCode2dParam(hv_DMCodeHandle, "module_size_max", 100);
        }

        private void SoftTriggerAndCaptureImage()
        {
            ho_Image.Dispose();
            hv_Width.UnpinTuple();
            hv_Height.UnpinTuple();
            // 执行软触发
            HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);

            if (!string.IsNullOrEmpty(_config.PicSaveConfig.PicPath))
            {
                if (!Directory.Exists(_config.PicSaveConfig.PicPath))
                {
                    Directory.CreateDirectory(_config.PicSaveConfig.PicPath);
                }
                string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}.{_config.PicSaveConfig.PicType}";
                HOperatorSet.WriteImage(ho_Image, _config.PicSaveConfig.PicType, 0, Path.Combine(_config.PicSaveConfig.PicPath, fileName));
                Task.Run(() =>
                {
                    try
                    {
                        File.Copy(Path.Combine(_config.PicSaveConfig.PicPath, fileName), Path.Combine(_config.PicSaveConfig.PicPath, "1.jpg"), true);
                    }
                    catch (Exception ex)
                    {
                        AddMsg($"程序异常：{ex.Message}");
                    }

                });
            }
            //HOperatorSet.ReadImage(out ho_Image, "d:/ng/20.bmp");

            // 等待图像采集完成
            //HOperatorSet.WaitSeconds(1);

            AnalyseCode();
        }

        private void AnalyseCode()
        {
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            HOperatorSet.SetPart(hWindow.HalconWindow, 0, 0, hv_Height - 1, hv_Width - 1);

            // 显示图像
            HOperatorSet.DispObj(ho_Image, hWindow.HalconWindow);
            HOperatorSet.SetDraw(hWindow.HalconWindow, "margin");

            if (barCodeChecked)
            {
                AnalyseBarCode();
            }
            if (ScanCodeConfig.Any(p => p.CodeType == "QR Code" && p.IsChecked))
            {
                AnalyseQrCode();
            }
            if (ScanCodeConfig.Any(p => p.CodeType == "Data Matrix ECC 200" && p.IsChecked))
            {
                AnalyseDMCode();
            }
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            var frameworkElement = view as System.Windows.FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }
            hWindow = frameworkElement.FindName("Hsmart") as HalconDotNet.HSmartWindowControlWPF;
            hWindow.SetFullImagePart();
            txtMsg = frameworkElement.FindName("rtxtMsg") as TextBox;
        }

        private void AnalyseBarCode()
        {
            HOperatorSet.GenEmptyObj(out ho_SybolRegions);
            HOperatorSet.SetColor(hWindow.HalconWindow, "green");//设置条码框选的颜色为单选色框
            HOperatorSet.SetLineWidth(hWindow.HalconWindow, 1);//设置框的大小范围

            hv_DecodeStrings = new HTuple();
            hv_Area = new HTuple();
            hv_Row = new HTuple();
            hv_Column = new HTuple();

            HOperatorSet.FindBarCode(ho_Image, out ho_SybolRegions, hv_BarCodeHandle, hv_BarCodeType, out hv_DecodeStrings);

            HOperatorSet.GetBarCodeResult(hv_BarCodeHandle, "all", "decoded_types", out hv_DecodeTypes);
            HOperatorSet.AreaCenter(ho_SybolRegions, out hv_Area, out hv_Row, out hv_Column);
            FitBarCodeInfo();
            HOperatorSet.DispObj(ho_SybolRegions, hWindow.HalconWindow);
            hv_DecodeStrings.UnpinTuple();
            hv_Area.UnpinTuple();
            hv_Row.UnpinTuple();
            hv_Column.UnpinTuple();
            ho_SybolRegions.Dispose();
        }

        private void AnalyseQrCode()
        {
            HOperatorSet.GenEmptyObj(out ho_QRSybolRegions);
            HOperatorSet.SetColor(hWindow.HalconWindow, "blue");
            HOperatorSet.SetLineWidth(hWindow.HalconWindow, 2);

            hv_DecodeStrings = new HTuple();
            hv_QRCodeResultHandle = new HTuple();
            hv_QRArea = new HTuple();
            hv_QRRow = new HTuple();
            hv_QRColumn = new HTuple();
            hv_QRPointOrder = new HTuple();

            HOperatorSet.FindDataCode2d(ho_Image, out ho_QRSybolRegions, hv_QRCodeHandle,
                "stop_after_result_num", 16, out hv_QRCodeResultHandle, out hv_DecodeStrings);

            HOperatorSet.AreaCenterXld(ho_QRSybolRegions, out hv_QRArea, out hv_QRRow, out hv_QRColumn, out hv_QRPointOrder);
            FitQrCodeInfo();
            HOperatorSet.DispObj(ho_QRSybolRegions, hWindow.HalconWindow);

            hv_QRCodeResultHandle.UnpinTuple();
            hv_DecodeStrings.UnpinTuple();
            hv_QRArea.UnpinTuple();
            hv_QRRow.UnpinTuple();
            hv_QRColumn.UnpinTuple();
            hv_QRPointOrder.UnpinTuple();
            ho_QRSybolRegions.Dispose();
        }

        private void AnalyseDMCode()
        {
            HOperatorSet.GenEmptyObj(out ho_DMSybolRegions);
            HOperatorSet.SetColor(hWindow.HalconWindow, "red");

            hv_DecodeStrings = new HTuple();
            hv_DMArea = new HTuple();
            hv_DMRow = new HTuple();
            hv_DMColumn = new HTuple();
            hv_DMCodeResultHandle = new HTuple();
            hv_DMPointOrder = new HTuple();
            try
            {
                HOperatorSet.FindDataCode2d(ho_Image, out ho_DMSybolRegions, hv_DMCodeHandle, "stop_after_result_num", 5, out hv_DMCodeResultHandle, out hv_DecodeStrings);
                HOperatorSet.AreaCenterXld(ho_DMSybolRegions, out hv_DMArea, out hv_DMRow, out hv_DMColumn, out hv_DMPointOrder);
                FitDMCodeInfo();
                HOperatorSet.DispObj(ho_DMSybolRegions, hWindow.HalconWindow);
            }
            catch (Exception ex)
            {
                AddMsg($"解析DM码出错：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            hv_DecodeStrings.UnpinTuple();
            hv_DMArea.UnpinTuple();
            hv_DMRow.UnpinTuple();
            hv_DMColumn.UnpinTuple();
            hv_DMCodeResultHandle.UnpinTuple();
            hv_DMPointOrder.UnpinTuple();
            ho_DMSybolRegions.Dispose();

        }

        private void FitBarCodeInfo()
        {
            if (hv_Area.Length > 0 && hv_Area.LArr.Length > 0)
            {
                for (int i = 0; i < hv_Area.LArr.Length; i++)
                {
                    var codeInfo = new CodeInfo();
                    codeInfo.CodeType = hv_DecodeTypes.SArr[i];
                    codeInfo.CodeValue = hv_DecodeStrings.SArr[i];
                    codeInfo.CenterX = Math.Round(hv_Column.DArr[i], 2);
                    codeInfo.CenterY = Math.Round(hv_Row.DArr[i], 2);
                    codeInfo.Angle = GetAngle(codeInfo.CenterX, codeInfo.CenterY);
                    codeInfos.Add(codeInfo);
                }
            }
        }

        private void FitQrCodeInfo()
        {
            if (hv_QRArea.Length > 0 && hv_QRArea.DArr.Length > 0)
            {
                for (int i = 0; i < hv_QRArea.DArr.Length; i++)
                {
                    var codeInfo = new CodeInfo();
                    codeInfo.CodeType = "QR Code";
                    codeInfo.CodeValue = hv_DecodeStrings.SArr[i];
                    codeInfo.CenterX = Math.Round(hv_QRColumn.DArr[i], 2);
                    codeInfo.CenterY = Math.Round(hv_QRRow.DArr[i], 2);
                    codeInfo.Angle = GetAngle(codeInfo.CenterX, codeInfo.CenterY);
                    codeInfos.Add(codeInfo);
                }
            }
        }

        private void FitDMCodeInfo()
        {
            if (hv_DMArea.Length > 0 && hv_DMArea.DArr.Length > 0)
            {
                for (int i = 0; i < hv_DMArea.DArr.Length; i++)
                {
                    var codeInfo = new CodeInfo();
                    codeInfo.CodeType = "DM";
                    codeInfo.CodeValue = hv_DecodeStrings.SArr[i];
                    codeInfo.CenterX = Math.Round(hv_DMColumn.DArr[i], 2);
                    codeInfo.CenterY = Math.Round(hv_DMRow.DArr[i], 2);
                    codeInfo.Angle = GetAngle(codeInfo.CenterX, codeInfo.CenterY);
                    codeInfos.Add(codeInfo);
                }
            }
        }

        private double GetAngle(double x, double y)
        {
            return Math.Round(Math.Atan2(y - _config.ImgCenter.Center_Y, x - _config.ImgCenter.Center_X) * 180 / Math.PI, 2);
        }

        public void ScrollToEnd(object sender)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void AddMsg(string msg)
        {
            sbMsg.AppendLine($"{DateTime.Now:yy-MM-dd HH:mm:ss.fff}: {msg}");
            txtMsg.Dispatcher.BeginInvoke(() =>
            {
                txtMsg.Text = sbMsg.ToString();
            });
            _logger.LogInformation(msg);
        }

        private MaterialInfoModel ParseCode()
        {
            var returnModel = new MaterialInfoModel();
            try
            {
                var matchedCodes = codeInfos.Where(c => c.CodeType == "DM" && c.CodeValue.Split("{").Length == 9).ToList();
                if (matchedCodes.Count == 1)
                {
                    var matchedCode = matchedCodes.First();
                    var splitCodes = matchedCode.CodeValue.Split("{");
                    returnModel.SN = splitCodes[8];
                    returnModel.PN = splitCodes[0];
                    returnModel.Qty = Convert.ToInt32(splitCodes[1]);
                    returnModel.DC = splitCodes[4];
                    returnModel.Supplier = splitCodes[3];
                    returnModel.OtherBarcode = matchedCode.CodeValue;
                }
            }
            catch (Exception ex)
            {
                AddMsg($"解析物料信息失败，{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return returnModel;
        }
    }
}

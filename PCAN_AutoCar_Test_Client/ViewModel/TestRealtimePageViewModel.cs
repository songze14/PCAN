using DynamicData;
using Excel.Tool;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using OfficeOpenXml;
using PCAN.Notification.Log;
using PCAN_AutoCar_Test_Client.Models;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using Peak.Can.Basic;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unit = System.Reactive.Unit;

namespace PCAN_AutoCar_Test_Client.ViewModel
{
    public class TestRealtimePageViewModel :ReactiveObject
    {
        private RepetitiveInstruction _repetitiveinstruction;
        public TestRealtimePageViewModel(PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel,IMediator mediator,IOptions<Repetitiveinstructions> repetitiveinstructionoptions)
        {
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
            _mediator = mediator;
            _repetitiveinstruction= repetitiveinstructionoptions.Value.LinTest;
            _testcancellationtokensource = new CancellationTokenSource();
            this.ChangeObs = this._sourceTestExcelGridModels.Connect();

            var d = this.ChangeObs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _testExcelGridModels)
                .DisposeMany()
                .Subscribe();
            PCanClientUsercontrolViewModel.NewMessage.Subscribe(msg =>
            {
                try
                {
                    if (msg == null)
                        return;
                    if (!string.IsNullOrWhiteSpace( _repetitiveinstruction.ReciveId))
                    {
                        var repetitiveinstructionrecvId = "0X" + msg.ID.ToString("X");
                      
                        if (repetitiveinstructionrecvId == _repetitiveinstruction.ReciveId.ToUpper())
                        {
                          
                            if (BitConverter.ToString(msg.DATA[0..msg.LEN]) == _repetitiveinstruction.ReciveOkData.ToUpper())
                            {
                                _mediator.Publish(new LogNotification()
                                {
                                    LogLevel = LogLevel.Information,
                                    LogSource = LogSource.TestRealtime,
                                    Message = $"回复正确数据，进入产测！！！！！回复数据:{msg.DATASTR }"
                                });
                                RestRecTimeOut();
                                return;
                            }
                            else if (msg.DATASTR == _repetitiveinstruction.ReciveNgData)
                            {
                                _mediator.Publish(new LogNotification()
                                {
                                    LogLevel = LogLevel.Error,
                                    LogSource = LogSource.TestRealtime,
                                    Message = $"回复NG数据，不进入产测！！！！！回复数据:{msg.DATASTR}"
                                });
                                RestRecTimeOut();
                                return;
                            }
                        }
                    }
                   
                    var recvId = "0X" + msg.ID.ToString("X");
                    var recvCommandId = "0X" + (msg.ID>>18>>16).ToString("X");
                    var sunrecvCommandId = "0X" + (msg.ID >> 16).ToString("X");
                    var findTestExcels = _sourceTestExcelGridModels.Items.Where(t => t.RecvId == recvId && t.RecvCommandId == recvCommandId && t.RecvSunCommandId==sunrecvCommandId);
                    if (!findTestExcels.Any())
                    {
                        return;
                    }
                    foreach (var testExcel in findTestExcels)
                    {
                        //解析数据
                        if (testExcel.RecvEnDataIndex >= msg.LEN)
                            continue;
                        byte[] dataBytes = new byte[testExcel.RecvEnDataIndex - testExcel.RecvBeDataIndex + 1];
                        Array.Copy(msg.DATA, testExcel.RecvBeDataIndex, dataBytes, 0, dataBytes.Length);

                        switch (testExcel.DataType)
                        {
                            case "uint8_t":
                                {
                                    var recvValue = dataBytes[0];
                                    var minValue =byte.Parse (testExcel.MinData);
                                    var maxValue = byte.Parse(testExcel.MaxData);
                                    testExcel.RecvData = recvValue.ToString();
                                    testExcel.Pass = (recvValue >= minValue && recvValue <= maxValue)? TestPassEnum.Pass: TestPassEnum.NG;
                                }
                                break;
                            case "uint16_t":
                                {
                                    if (dataBytes.Length<2)
                                    {
                                        _mediator.Publish(new LogNotification()
                                        {
                                            LogLevel = LogLevel.Error,
                                            LogSource = LogSource.TestRealtime,
                                            Message = $"数据目标格式uint16_t数据长度应为2，实际为{dataBytes.Length}"
                                        });
                                        return;
                                    }
                                    var recvValue = BitConverter.ToUInt16(dataBytes);
                                    var minValue = Convert.ToUInt16(testExcel.MinData);
                                    var maxValue = Convert.ToUInt16(testExcel.MaxData);
                                    testExcel.RecvData = recvValue.ToString();
                                    testExcel.Pass = (recvValue >= minValue && recvValue <= maxValue) ? TestPassEnum.Pass : TestPassEnum.NG;
                                }
                                break;
                            case "uint32_t":
                                {
                                    if (dataBytes.Length < 4)
                                    {
                                        _mediator.Publish(new LogNotification()
                                        {
                                            LogLevel = LogLevel.Error,
                                            LogSource = LogSource.TestRealtime,
                                            Message = $"数据目标格式uint32_t数据长度应为4，实际为{dataBytes.Length}"
                                        });
                                        return;

                                    }
                                    var recvValue = BitConverter.ToUInt32(dataBytes);
                                    var minValue = Convert.ToUInt32(testExcel.MinData);
                                    var maxValue = Convert.ToUInt32(testExcel.MaxData);
                                    testExcel.RecvData = recvValue.ToString();
                                    testExcel.Pass = (recvValue >= minValue && recvValue <= maxValue) ? TestPassEnum.Pass : TestPassEnum.NG;
                                }
                                break;
                            case "char":
                                {
                                    var recvValue = Encoding.ASCII.GetString(dataBytes).TrimEnd('\0');
                                    testExcel.RecvData = recvValue;
                                    testExcel.Pass = TestPassEnum.人工;
                                }
                                break;
                            case "int8_t":
                                {
                                    var recvValue =(sbyte)dataBytes[0];
                                    var minValue =sbyte.Parse( testExcel.MinData);
                                    var maxValue = sbyte.Parse(testExcel.MaxData);
                                    testExcel.RecvData = recvValue.ToString();
                                    testExcel.Pass = (recvValue >= minValue && recvValue <= maxValue) ? TestPassEnum.Pass : TestPassEnum.NG;
                                }
                                break;
                            case "int16_t":
                                {
                                    if (dataBytes.Length < 2)
                                    {
                                        _mediator.Publish(new LogNotification()
                                        {
                                            LogLevel = LogLevel.Error,
                                            LogSource = LogSource.TestRealtime,
                                            Message = $"数据目标格式int16_t数据长度应为2，实际为{dataBytes.Length}"
                                        });
                                        return;

                                    }
                                    var recvValue = BitConverter.ToInt16(dataBytes);
                                    var minValue = Convert.ToInt16(testExcel.MinData);
                                    var maxValue = Convert.ToInt16(testExcel.MaxData);
                                    testExcel.RecvData = recvValue.ToString();
                                    testExcel.Pass = (recvValue >= minValue && recvValue <= maxValue) ? TestPassEnum.Pass : TestPassEnum.NG;
                                }
                                break;
                            case "int32_t":
                                {
                                    if (dataBytes.Length < 4)
                                    {
                                        _mediator.Publish(new LogNotification()
                                        {
                                            LogLevel = LogLevel.Error,
                                            LogSource = LogSource.TestRealtime,
                                            Message = $"数据目标格式int32_t数据长度应为4，实际为{dataBytes.Length}"
                                        });
                                        return;

                                    }
                                    var recvValue = BitConverter.ToInt32(dataBytes);
                                    var minValue = Convert.ToInt32(testExcel.MinData);
                                    var maxValue = Convert.ToInt32(testExcel.MaxData);
                                    testExcel.RecvData = recvValue.ToString();
                                    testExcel.Pass = (recvValue >= minValue && recvValue <= maxValue) ? TestPassEnum.Pass : TestPassEnum.NG;
                                }
                                break;
                            case "float":
                                {
                                    if (dataBytes.Length < 4)
                                    {
                                        _mediator.Publish(new LogNotification()
                                        {
                                            LogLevel = LogLevel.Error,
                                            LogSource = LogSource.TestRealtime,
                                            Message = $"数据目标格式float数据长度应为4，实际为{dataBytes.Length}"
                                        });
                                        return;

                                    }
                                    var recvValue = BitConverter.ToSingle(dataBytes);
                                    var minValue = Convert.ToSingle(testExcel.MinData);
                                    var maxValue = Convert.ToSingle(testExcel.MaxData);
                                    testExcel.RecvData = recvValue.ToString();
                                    testExcel.Pass = (recvValue >= minValue && recvValue <= maxValue) ? TestPassEnum.Pass : TestPassEnum.NG;
                                }
                               
                                break;
                        }
                        _mediator.Publish(new LogNotification()
                        {
                            LogLevel = LogLevel.Information,
                            LogSource = LogSource.TestRealtime,
                            Message = $"收到ID:0x{testExcel.RecvId:X3} 数据:{BitConverter.ToString(dataBytes)} 最大值{testExcel.MaxData} 最小值{testExcel.MinData} 结果{testExcel.Pass}"
                        });

                    }
                    RestRecTimeOut();

                }
                catch (Exception ex)
                {
                    _mediator.Publish(new LogNotification()
                    {
                        LogLevel = LogLevel.Error,
                        LogSource = LogSource.TestRealtime,
                        Message = $"解析回复数据时出现错误:{ex.Message}"
                    });
                    RestRecTimeOut();
                }

            });
            BrowseFileCommand = ReactiveCommand.Create(async () =>
            {
                CanStartTest = false;
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "测试文件/xlsx|*.xlsx",
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    SelectedFilePath = openFileDialog.FileName;
                }
                if (string.IsNullOrWhiteSpace(SelectedFilePath))
                {
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.TestRealtime, Message = $"文件未选择！" });
                    return;

                }
                var stream = File.OpenRead(SelectedFilePath);

                try
                {
                    _sourceTestExcelGridModels.Clear();
                    ExcelPackage excelPackage = new ExcelPackage(File.OpenRead(SelectedFilePath));
                    ExcelWorksheets worksheets = excelPackage.Workbook.Worksheets;
                    var excelTools = ExcelToEntity.WorksheetToDataRow<TestExcel>(stream, 1, 2, 0, 0);
                    foreach (var item in excelTools)
                    {
                        _sourceTestExcelGridModels.Add(new TestExcelGrid
                        {
                            DataType = item.DataType.Trim(),
                            MaxData = item.MaxData.Trim(),
                            MinData = item.MinData.Trim(),
                            RecvBeDataIndex = item.RecvBeDataIndex,
                            RecvEnDataIndex = item.RecvEnDataIndex,
                            ParmName=item.ParmName.Trim(),
                            RecvId = item.RecvId.ToUpper().Trim(),
                            RecvCommandId = item.RecvCommandId.ToUpper().Trim(),
                            SendData = item.SendData.Trim(),
                            SendId =item.SendId.Trim(),
                            Index = _sourceTestExcelGridModels.Count + 1,
                            帧间隔=item.帧间隔,
                        });
                    }
                    CanStartTest = true;


                }
                catch (Exception ex)
                {

                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.TestRealtime, Message = $"获取检测内容出现错误:{ex.Message}" });

                }
                finally
                {
                    stream.Close();
                }

            }
           );
            TestCommand= ReactiveCommand.Create<Task>(async () =>
            {
                try
                {
                    if (!PCanClientUsercontrolViewModel.IsConnected)
                    {
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.TestRealtime, Message = $"设备未连接，请先连接设备！" });
                        return;

                    }
                    this.CanStartTest = false;  
                    await ResetGridStatus();
                    if (string.IsNullOrWhiteSpace( _repetitiveinstruction.Id))
                    {
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.TestRealtime, Message = $"未配置开启产测指令！" });
                    }
                    else
                    {
                        ///若配置了产测指令，则发送产测指令
                        var repetitiveinstructionid = Convert.ToUInt32(_repetitiveinstruction.Id, 16);
                        byte[] repetitiveinstructiondata;
                        var datastr = _repetitiveinstruction.Data.Split('-', StringSplitOptions.RemoveEmptyEntries);
                        if (datastr != null)
                        {
                            repetitiveinstructiondata = new byte[datastr.Length];
                            for (int i = 0; i < datastr.Length; i++)
                            {
                                repetitiveinstructiondata[i] = Convert.ToByte(datastr[i], 16);
                            }
                        }
                        else
                        {
                            //无数据
                            repetitiveinstructiondata = [];
                        }
                        PCanClientUsercontrolViewModel.WriteMsg(repetitiveinstructionid, repetitiveinstructiondata, _repetitiveinstruction.Extended, async () => { await RecTimeOut(true); });
                        await Task.Delay(_repetitiveinstruction.SendDelay);
                        for (int i = 1; i < _repetitiveinstruction.SendCount; i++)
                        {
                            PCanClientUsercontrolViewModel.WriteMsg(repetitiveinstructionid, repetitiveinstructiondata, _repetitiveinstruction.Extended);

                            await Task.Delay(_repetitiveinstruction.SendDelay);
                        }
                        await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.TestRealtime, Message = $"已发送开启产测指令，ID:0x{_repetitiveinstruction.Id} 数据:{_repetitiveinstruction.Data}" });
                        await _semaphoreslim.WaitAsync();
                        if (_testcancellationtokensource.IsCancellationRequested)
                        {
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.TestRealtime, Message = $"超时未回应进入产测，退出测试！！！！！！" });

                            return;
                        } 
                    }
                    var sendgroups = _sourceTestExcelGridModels.Items.GroupBy(t => t.SendId);

                    foreach (var sendgroup in sendgroups)
                    {
                        var sendid = Convert.ToUInt32(sendgroup.Key, 16);
                        var senddatagroups = sendgroup.GroupBy(t => (t.SendData,t.帧间隔));
                        foreach (var senddatagroup in senddatagroups)
                        {
                            PCanClientUsercontrolViewModel.WriteMsg(sendid, [],true, async () => {await RecTimeOut(); });
                            await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.TestRealtime, Message = $"发送ID:0x{sendgroup.Key:X} 数据:{senddatagroup.Key.SendData},下一帧间隔:{senddatagroup.Key.帧间隔}" });
                            await _semaphoreslim.WaitAsync();
                            await Task.Delay(senddatagroup.Key.帧间隔);
                        }

                    }
                }
                catch (Exception ex)
                {

                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.TestRealtime, Message = $"测试时发生错误{ex.Message}" });

                }
                finally
                {
                    this.CanStartTest = true;

                }

            });
            ExportTemplateCommand = ReactiveCommand.Create(async () =>
            {
                var templateSaveFileDialog = new SaveFileDialog
                {
                    Filter = "测试文件/xlsx|*.xlsx",
                    FileName = "TestTemplate.xlsx"
                };
                if (templateSaveFileDialog.ShowDialog() != true)
                {
                    return;
                }
                var saveFilePath = templateSaveFileDialog.FileName;
                var savedata = new List<TestExcel>
                {
                    new TestExcel
                    {
                        DataType="uint8_t",
                        MaxData="100",
                        MinData="00",
                        RecvBeDataIndex=0,
                        RecvEnDataIndex=0,
                        ParmName="测试参数1",
                        RecvId="0x100",
                        RecvCommandId="0x00",
                        RecvSunCommandId="0x02",
                        SendData="01-02-03-04-05-06-07-08",
                        SendId="0x200",
                        帧间隔=10,
                        
                    }
                };
                ExcelPackage excelPackage = new ExcelPackage();
                var worksheet = ExcelToEntity.ListToExcel<TestExcel>(excelPackage, "TestTemplate", 1, savedata);
                worksheet.SaveAs(new FileInfo(saveFilePath));
                worksheet.Dispose();
                await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.TestRealtime, Message = $"已导出测试模板至:{saveFilePath}" });
            });
        }
        /// <summary>
        /// 回复超时
        /// </summary>
        /// <param name="canceltest">是否取消测试</param>
        /// <returns></returns>
        private async Task RecTimeOut(bool canceltest=false)
        {
            try
            {
                _timecancellationtokensource = new CancellationTokenSource();
                var periodictimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
                while (await periodictimer.WaitForNextTickAsync(_timecancellationtokensource.Token))
                {
                   
                    if (_semaphoreslim.CurrentCount == 0)
                    {
                        _semaphoreslim.Release();

                    }
                    if (canceltest)
                    {
                        _testcancellationtokensource.Cancel();
                        CanStartTest = true;

                    }
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.TestRealtime, Message = $"信息回复超时,退出测试！！！！！！" });
                    return;
                }
            }
            catch (Exception ex)
            {
               
            }
        }
        /// <summary>
        /// 重置回复超时
        /// </summary>
        private void RestRecTimeOut()
        {
            try
            {
                if (_timecancellationtokensource != null)
                {
                    _timecancellationtokensource.Cancel();
                    ///尝试清除，一般情况下，线程取消信号量会自动回收，这里做个保险
                    if (_semaphoreslim.CurrentCount == 0)
                    {
                        _semaphoreslim.Release();

                    }
                }
            }
            catch (Exception ex)
            {

                 _mediator.Publish(new LogNotification()
                {
                    LogLevel = LogLevel.Error,
                    LogSource = LogSource.TestRealtime,
                    Message = $"解除超时计时器时出现错误:{ex.Message}"
                });
            }
           
        }
        private async Task ResetGridStatus()
        {
            foreach (var item in _sourceTestExcelGridModels.Items)
            {
                item.Pass = TestPassEnum.Non;
                item.RecvData = string.Empty;
            }
        }
        [Reactive]
        public string SelectedFilePath { get; set; }
       
        [Reactive]
        public bool CanStartTest { get; set; }
        public ReactiveCommand<Unit,Task> TestCommand { get; set; }
        public ReactiveCommand<Unit,Task> ExportTemplateCommand { get; set; }
        public ReactiveCommand<Unit, Task> BrowseFileCommand { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
        public IObservable<IChangeSet<TestExcelGrid>> ChangeObs { get; }

        public SourceList<TestExcelGrid> _sourceTestExcelGridModels = new SourceList<TestExcelGrid>();
        private readonly ReadOnlyObservableCollection<TestExcelGrid> _testExcelGridModels;
        private readonly IMediator _mediator;
        private SemaphoreSlim _semaphoreslim = new SemaphoreSlim(0, 1);
        private CancellationTokenSource _timecancellationtokensource;
        private CancellationTokenSource _testcancellationtokensource;

        public ReadOnlyObservableCollection<TestExcelGrid> TestExcelGridModels => _testExcelGridModels;
    }
}

using DynamicData;
using Excel.Tool;
using MediatR;
using Microsoft.Extensions.Logging;
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
        public TestRealtimePageViewModel(PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel,IMediator mediator)
        {
            PCanClientUsercontrolViewModel = pCanClientUsercontrolViewModel;
            _mediator = mediator;
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
                                    ulong minValue = Convert.ToUInt64(testExcel.MinData);
                                    ulong maxValue = Convert.ToUInt64(testExcel.MaxData);
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
                                    }
                                    var recvValue = BitConverter.ToUInt16(dataBytes);
                                    ulong minValue = Convert.ToUInt64(testExcel.MinData);
                                    ulong maxValue = Convert.ToUInt64(testExcel.MaxData);
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
                                    }
                                    var recvValue = BitConverter.ToUInt32(dataBytes);
                                    ulong minValue = Convert.ToUInt64(testExcel.MinData);
                                    ulong maxValue = Convert.ToUInt64(testExcel.MaxData);
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
                        }
                        _mediator.Publish(new LogNotification()
                        {
                            LogLevel = LogLevel.Information,
                            LogSource = LogSource.TestRealtime,
                            Message = $"收到ID:0x{testExcel.RecvId:X3} 数据:{BitConverter.ToString(dataBytes)} 最大值{testExcel.MaxData} 最小值{testExcel.MinData} 结果{testExcel.Pass}"
                        });

                    }
                   
                }
                catch (Exception ex)
                {
                    _mediator.Publish(new LogNotification()
                    {
                        LogLevel = LogLevel.Error,
                        LogSource = LogSource.TestRealtime,
                        Message = $"解析回复数据时出现错误:{ex.Message}"
                    });

                }
                finally
                {
                    if (_timecancellationtokensource!=null)
                    {
                        _timecancellationtokensource.Cancel();
                        if (_semaphoreslim.CurrentCount == 0)
                        {
                            _semaphoreslim.Release();

                        }
                    }
                    
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
            TestCommand= ReactiveCommand.Create(async () =>
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
                    var sendgroups = _sourceTestExcelGridModels.Items.GroupBy(t => t.SendId);

                    foreach (var sendgroup in sendgroups)
                    {
                        var sendid = Convert.ToUInt32(sendgroup.Key, 16);
                        var senddatagroups = sendgroup.GroupBy(t => (t.SendData,t.帧间隔));
                        foreach (var senddatagroup in senddatagroups)
                        {
                            //var datastr = senddatagroup.Key.SendData.Split('-', StringSplitOptions.RemoveEmptyEntries);
                            //if (datastr == null)
                            //{
                            //    return;
                            //}
                            //var commandFrame = new byte[datastr.Length];
                            //for (int i = 0; i < datastr.Length; i++)
                            //{
                            //    commandFrame[i] = Convert.ToByte(datastr[i], 16);
                            //}
                            PCanClientUsercontrolViewModel.WriteMsg(sendid, [], async () => {await Reset(); });
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
                        RecvCommandId="0x01",
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
        private async Task Reset()
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
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.Upload, Message = $"信息回复超时" });
                    return;
                }
            }
            catch (Exception ex)
            {
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

        public ReadOnlyObservableCollection<TestExcelGrid> TestExcelGridModels => _testExcelGridModels;
    }
}

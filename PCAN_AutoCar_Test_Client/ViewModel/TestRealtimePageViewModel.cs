using DynamicData;
using Excel.Tool;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using OfficeOpenXml;
using PCAN.Notification.Log;
using PCAN_AutoCar_Test_Client.Models;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
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
                if (msg==null)
                 return;
                var recvId ="0X"+msg.ID.ToString("X");
                var recvCommandId = msg.DATA.Length > 0 ? "0X" + msg.DATA[0].ToString("X2") : string.Empty;
                var findTestExcels = _sourceTestExcelGridModels.Items.Where(t => t.RecvId == recvId && t.RecvCommandId== recvCommandId);
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
                                testExcel.Pass = recvValue >= minValue && recvValue <= maxValue;
                            }
                            break;
                        case "uint16_t":
                            {
                                var recvValue = BitConverter.ToUInt16(dataBytes);
                                ulong minValue = Convert.ToUInt64(testExcel.MinData);
                                ulong maxValue = Convert.ToUInt64(testExcel.MaxData);
                                testExcel.RecvData = recvValue.ToString();
                                testExcel.Pass = recvValue >= minValue && recvValue <= maxValue;
                            }
                            break;
                        case "uint32_t":
                            {
                                var recvValue = BitConverter.ToUInt32(dataBytes);
                                ulong minValue = Convert.ToUInt64(testExcel.MinData);
                                ulong maxValue = Convert.ToUInt64(testExcel.MaxData);
                                testExcel.RecvData = recvValue.ToString();
                                testExcel.Pass = recvValue >= minValue && recvValue <= maxValue;
                            }
                            break;
                        case "char":
                            {
                                var recvValue = Encoding.ASCII.GetString(dataBytes).TrimEnd('\0');
                                testExcel.RecvData = recvValue;
                                testExcel.Pass = true;
                            }
                            break;
                    }
                     _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.TestRealtime, 
                         Message = $"收到ID:0x{testExcel.RecvId:X3} 数据:{BitConverter.ToString( dataBytes)} 最大值{testExcel.MaxData} 最小值{testExcel.MinData} 结果{testExcel.Pass}" });
                 
                }
                _timecancellationtokensource.Cancel();
                if (_semaphoreslim.CurrentCount==0)
                {
                    _semaphoreslim.Release();

                }

            });
            BrowseFileCommand = ReactiveCommand.Create(async () =>
            {
                try
                {
                    _sourceTestExcelGridModels.Clear();
                    var openFileDialog = new OpenFileDialog
                    {
                        Filter = "测试文件/xlsx|*.xlsx",
                    };
                    if (openFileDialog.ShowDialog() == true)
                    {
                        SelectedFilePath = openFileDialog.FileName;
                        ExcelPackage excelPackage = new ExcelPackage(File.OpenRead(SelectedFilePath));
                        ExcelWorksheets worksheets = excelPackage.Workbook.Worksheets;
                      
                        var worksheet = worksheets[0];
                        var excelTools = ExcelToEntity.WorksheetToDataRow<TestExcel>(File.OpenRead(SelectedFilePath), 1, 2, 0, 0);
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
                                SendId = item.SendId.Trim(),
                                Index = _sourceTestExcelGridModels.Count + 1,
                                帧间隔=item.帧间隔,
                            });
                        }
                    }
                }
                catch (Exception ex)
                {

                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.TestRealtime, Message = $"获取检测内容出现错误:{ex.Message}" });

                }

            }
           );
            TestCommand= ReactiveCommand.Create(async () =>
            {
                try
                {
                    await ResetGridStatus();
                    var sendgroups = _sourceTestExcelGridModels.Items.GroupBy(t => t.SendId);

                    foreach (var sendgroup in sendgroups)
                    {
                        var sendid = Convert.ToUInt32(sendgroup.Key, 16);
                        var senddatagroups = sendgroup.GroupBy(t => (t.SendData,t.帧间隔));
                        foreach (var senddatagroup in senddatagroups)
                        {
                            var datastr = senddatagroup.Key.SendData.Split('-', StringSplitOptions.RemoveEmptyEntries);
                            if (datastr == null)
                            {
                                return;
                            }
                            var commandFrame = new byte[datastr.Length];
                            for (int i = 0; i < datastr.Length; i++)
                            {
                                commandFrame[i] = Convert.ToByte(datastr[i], 16);
                            }
                            PCanClientUsercontrolViewModel.WriteMsg(sendid, commandFrame, async () => {await Reset(); });
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
                        MaxData="FF",
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
                item.Pass = false;
                item.RecvData = string.Empty;
            }
        }
        [Reactive]
        public string SelectedFilePath { get; set; }
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

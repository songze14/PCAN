using DynamicData;
using Excel.Tool;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
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
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
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
                var findTestExcels = _sourceTestExcelGridModels.Items.Where(t => t.RecvId == msg.ID);
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
                                ulong minValue = Convert.ToUInt64(testExcel.MinData, 16);
                                ulong maxValue = Convert.ToUInt64(testExcel.MaxData, 16);
                                testExcel.Pass = recvValue >= minValue && recvValue <= maxValue;
                            }
                            break;
                        case "uint16_t":
                            {
                                var recvValue = BitConverter.ToUInt16(dataBytes);
                                ulong minValue = Convert.ToUInt64(testExcel.MinData);
                                ulong maxValue = Convert.ToUInt64(testExcel.MaxData);
                                testExcel.Pass = recvValue >= minValue && recvValue <= maxValue;
                            }
                            break;
                        case "uint32_t":
                            {
                                var recvValue = BitConverter.ToUInt32(dataBytes);
                                ulong minValue = Convert.ToUInt64(testExcel.MinData);
                                ulong maxValue = Convert.ToUInt64(testExcel.MaxData);
                                testExcel.Pass = recvValue >= minValue && recvValue <= maxValue;
                            }
                            break;
                    }
                     _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.TestRealtime, 
                         Message = $"收到ID:0x{testExcel.RecvId:X3} 数据:{msg.DATASTR} 最大值{testExcel.MaxData} 最小值{testExcel.MinData} 结果{testExcel.Pass}" });

                    _semaphoreslim.Release();
                }

            });
            BrowseFileCommand = ReactiveCommand.Create(() =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "升级文件/excel|*.excel",
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    SelectedFilePath = openFileDialog.FileName;
                    var excelTools = ExcelToEntity.WorksheetToDataRow<TestExcel>(File.OpenRead(SelectedFilePath),1,1,0,0);
                    foreach (var item in excelTools)
                    {
                        _sourceTestExcelGridModels.Add(new TestExcelGrid 
                        {
                            DataType=item.DataType,
                            MaxData=item.MaxData,
                            MinData=item.MinData,
                            RecvBeDataIndex=item.RecvBeDataIndex,
                            RecvEnDataIndex=item.RecvEnDataIndex,
                            RecvId=item.RecvId,
                            SendData=item.SendData,
                            SendId=item.SendId,
                            Index=_sourceTestExcelGridModels.Count+ 1
                        });
                    }
                }
            }
           );
            TestCommand= ReactiveCommand.Create(async () =>
            {
                foreach (var testExcel in _sourceTestExcelGridModels.Items)
                {
                    var datastr = testExcel.SendData.Split('-',StringSplitOptions.RemoveEmptyEntries);
                    if (datastr==null)
                    {
                        return;
                    }
                    var commandFrame = new byte[datastr.Length];
                    for (int i = 0; i < datastr.Length; i++)
                    {
                        commandFrame[i] = Convert.ToByte(datastr[i], 16);
                    }
                    PCanClientUsercontrolViewModel.WriteMsg(testExcel.SendId, commandFrame);
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.TestRealtime, Message = $"发送ID:0x{testExcel.SendId:X3} 数据:{testExcel.SendData}" });
                    await _semaphoreslim.WaitAsync();
                }
            });
        }
        [Reactive]
        public string SelectedFilePath { get; set; }
        public ReactiveCommand<Unit,Task> TestCommand { get; set; }
        public ReactiveCommand<Unit,Unit> BrowseFileCommand { get; set; }
        public PCanClientUsercontrolViewModel PCanClientUsercontrolViewModel { get; }
        public IObservable<IChangeSet<TestExcelGrid>> ChangeObs { get; }

        public SourceList<TestExcelGrid> _sourceTestExcelGridModels = new SourceList<TestExcelGrid>();
        private readonly ReadOnlyObservableCollection<TestExcelGrid> _testExcelGridModels;
        private readonly IMediator _mediator;
        private SemaphoreSlim _semaphoreslim = new SemaphoreSlim(0, 1);
        public ReadOnlyObservableCollection<TestExcelGrid> TestExcelGridModels => _testExcelGridModels;
    }
}

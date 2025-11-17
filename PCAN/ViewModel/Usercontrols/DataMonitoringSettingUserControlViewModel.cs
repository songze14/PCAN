using DynamicData;
using DynamicData.Binding;
using MediatR;
using Microsoft.Extensions.Logging;
using PCAN.Modles;
using PCAN.Notification.Log;
using PCAN.Shard.Tools;
using PCAN.SqlLite.Abs;
using PCAN.SqlLite.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Unit = System.Reactive.Unit;

namespace PCAN.ViewModel.Usercontrols
{
    public class DataMonitoringSettingUserControlViewModel :ReactiveObject
    {
        private readonly IDataMonitoringSettingService _datamonitoringsettingservice;
        private readonly IMediator _mediator;

        public DataMonitoringSettingUserControlViewModel(IDataMonitoringSettingService dataMonitoringSettingService,IMediator mediator)
        {
            _datamonitoringsettingservice = dataMonitoringSettingService;
            _mediator = mediator;
            
            DataMonitoringSettingDataParmSourceList
             .Connect()
             .Sort(SortExpressionComparer<DataMonitoringSettingDataParm>.Ascending(x => x.Index)) // 排序
             .Bind(out _dataMonitoringSettingDataParmSourceList)
             .Subscribe();
            this.AnalysisParmstrCommand =ReactiveCommand.Create(()=> 
            {
                try
                {
                    if (DeviceParmValueStr == null)
                    {
                        return;
                    }
                    //解析参数字符串
                    var inputparmstrs = DeviceParmValueStr.Split("\r\n");
                    var typerepagex = ParmRegex.TypeRegex();
                    var namerepagex = ParmRegex.NameRegex();
                    var remarkrepagex = ParmRegex. RemarkRegex();
                    var remark = string.Empty;
                    DataMonitoringSettingDataParmSourceList.Clear();

                    foreach (var inputparmstr in inputparmstrs)
                    {
                        var parmstr = inputparmstr.Trim().TrimEnd().TrimStart();
                        if (string.IsNullOrWhiteSpace(parmstr))
                            continue;
                        var typematch = typerepagex.Match(parmstr);
                        if (typematch == null || !typematch.Success)
                        {
                            MessageBox.Show($"字符串{parmstr}找不到数据类型！");
                            return;
                        }
                        var type = typematch.Value;
                        var typeinfo = CTypeToCsharpTypeValue.TypeInfos.FirstOrDefault(o => o.Name == type);
                        if (typeinfo == null)
                        {
                            MessageBox.Show($"解析参数失败:{parmstr}存在不可解析类型{type}");
                            return;
                        }
                        var namematch = namerepagex.Match(parmstr);
                        if (namematch == null || !namematch.Success)
                        {
                            MessageBox.Show($"字符串{parmstr}找不到参数名称！");
                            return;
                        }
                        var name = namematch.Value;
                        var remarkmatch = remarkrepagex.Match(parmstr);
                        if (remarkmatch != null && remarkmatch.Success)
                        {
                            remark = remarkmatch.Value;
                        }
                        else
                        {
                            remark = string.Empty;
                        }
                        DataMonitoringSettingDataParmSourceList.Add(new DataMonitoringSettingDataParm()
                        {
                            Id = 0,
                            Index = DataMonitoringSettingDataParmSourceList.Count+1,
                            Type = type,
                            Name = name,
                            Remark = remark,
                            Size = typeinfo.Size,
                        });
                    }
                }
                catch (Exception ex)
                {
                    _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.DataMonitoring, Message = $"文本解析错误:{ex.Message}" });


                }

            });
            this.SaveParmCommand= ReactiveCommand.CreateFromTask(async() =>
            {
                try
                {
                    var datas = DataMonitoringSettingDataParmSourceList.Items.ToList();
                    await _datamonitoringsettingservice.AddDataMonitoringSettingDataParms(datas);
                    await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Information, LogSource = LogSource.DataMonitoring, Message = $"保存数据监控参数成功" });
                }
                catch (Exception ex)
                {
                   await _mediator.Publish(new LogNotification() { LogLevel = LogLevel.Error, LogSource = LogSource.DataMonitoring, Message = $"保存数据监控参数失败:{ex.Message}" });
                }
            });
            this.RefreshParmCommand = ReactiveCommand.CreateFromTask(async() =>
            {
                await GetDataMonitoringSettingDataParmSourceList();
            });
            RefreshParmCommand.Execute();
        }
       
        public ReactiveCommand<Unit,Unit> RefreshParmCommand { get; }
        public ReactiveCommand<Unit,Unit> SaveParmCommand { get; }
        [Reactive]
        public string DeviceParmValueStr { get; set; }
        /// <summary>
        /// 解析参数命令
        /// </summary>
        public ReactiveCommand<Unit,Unit> AnalysisParmstrCommand { get; }   
        private async Task GetDataMonitoringSettingDataParmSourceList()
        {
            DataMonitoringSettingDataParmSourceList.Clear();
            var result = await _datamonitoringsettingservice.GetDataMonitoringSettingDataParms();
           
            DataMonitoringSettingDataParmSourceList.AddRange(result);
        }
        public SourceList<DataMonitoringSettingDataParm> DataMonitoringSettingDataParmSourceList { get; } = new();
        private readonly ReadOnlyObservableCollection<DataMonitoringSettingDataParm> _dataMonitoringSettingDataParmSourceList;
        public ReadOnlyObservableCollection<DataMonitoringSettingDataParm> DataMonitoringSettingDataParm => _dataMonitoringSettingDataParmSourceList;
    }
}

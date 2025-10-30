using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using PCAN.Notification.Log;
using PCAN_AutoCar_Test_Client.Models;
using PCAN_AutoCar_Test_Client.ViewModel.USercontrols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PCAN.Shard.Tools;

namespace PCAN_AutoCar_Test_Client.ViewModel
{
    public class WriteSNWindowsViewModel:ReactiveObject
    {
        private readonly PCanClientUsercontrolViewModel _pcanclientusercontrolviewmodel;
        private readonly RepetitiveInstruction _repetitiveinstruction;
        private SemaphoreSlim _semaphoreslim = new SemaphoreSlim(0, 1);
        private CancellationTokenSource _timecancellationtokensource;

        public WriteSNWindowsViewModel(PCanClientUsercontrolViewModel pCanClientUsercontrolViewModel, RepetitiveInstruction repetitiveInstruction)
        {
            _pcanclientusercontrolviewmodel = pCanClientUsercontrolViewModel;
            _repetitiveinstruction = repetitiveInstruction;
            _timecancellationtokensource = new CancellationTokenSource();
            _pcanclientusercontrolviewmodel.NewMessage.Subscribe(msg =>
            {
                if (msg != null) 
                {
                    var recvId = "0X" + msg.ID.ToString("X");
                    if (msg.ID.ToString() == _repetitiveinstruction.ReciveId.ToUpper())
                    {
                        try
                        {
                            if (BitConverter.ToString(msg.DATA[0..msg.LEN]) == _repetitiveinstruction.ReciveOkData.ToUpper())
                            {
                                MessageBox.Show("写入完成!");
                                return;
                            }
                            else if (msg.DATASTR == _repetitiveinstruction.ReciveNgData)
                            {
                                MessageBox.Show("写入失败!");
                                return;
                            }
                        }
                        catch (Exception ex)
                        {

                            throw;
                        }
                        finally
                        {
                            if (_semaphoreslim.CurrentCount == 0)
                            {
                                var a = _semaphoreslim.Release();
                                 _timecancellationtokensource.Cancel();
                            }
                        }
                      
                    }
                }
            });
            this.WriteCommand = ReactiveCommand.Create(() =>
            {
                //_ = Task.Run(() =>
                //{
                    if (string.IsNullOrWhiteSpace(SN))
                        return;
                    //获取UTC时间
                    var time = DateTime.Now.Get1970ToNowSeconds();
                    //拼合字符串
                    var writedata = SN + "|" + time;
                    //写入数据
                    var sendid = Convert.ToUInt32(_repetitiveinstruction.Id, 16);
                var d = System.Text.Encoding.ASCII.GetBytes(writedata);
                    _pcanclientusercontrolviewmodel.WriteMsg(sendid, System.Text.Encoding.ASCII.GetBytes(writedata), true, async () => { await RecTimeOut(sendid); });

                    _semaphoreslim.Wait();
                //});
              

            }
            );
        }
        private async Task RecTimeOut(uint id)
        {
            _ = Task.Run(async () =>
             {
                 try
                 {
                     _timecancellationtokensource = new CancellationTokenSource();
                     var periodictimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
                    
                     while (await periodictimer.WaitForNextTickAsync(_timecancellationtokensource.Token))
                     {

                         if (_semaphoreslim.CurrentCount == 0)
                         {
                             var a = _semaphoreslim.Release();

                         }
                         //_barrier.SignalAndWait();
                         MessageBox.Show($"信息id:0x{id:X},信息回复超时,退出测试！！！！！！");
                         return;
                     }
                 }
                 catch (Exception ex)
                 {

                 }

             });

        }
        [Reactive]
        public string SN { get; set; }
        public ReactiveCommand<Unit,Unit> WriteCommand { get; }
      
    }
}

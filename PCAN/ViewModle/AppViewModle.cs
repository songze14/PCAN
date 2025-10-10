using PCAN.Modles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PCAN.ViewModle
{
    public class AppViewModle:ReactiveObject
    {
        private readonly IServiceProvider _serviceprovider;
        public ReactiveProperty<Page> CurrentPage;
        public Func<string, Page> MapSourceToPage { get; set; }

        public AppViewModle(IServiceProvider serviceProvider)
        {
            _serviceprovider = serviceProvider;
            CurrentPage=new ReactiveProperty<Page>();
            
        }
        public void NavigateTo(string source) 
        {
            var page =MapSourceToPage(source);
            if (page != null)
            {
                CurrentPage.Value = page;
            }
        }
        
    }
}

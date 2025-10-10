using PCAN.Modles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PCAN.ViewModel
{
    public class AppViewModel:ReactiveObject
    {
        private readonly IServiceProvider _serviceprovider;
        public ReactiveProperty<Page> CurrentPage;
        public Func<string, Page> MapSourceToPage { get; set; }

        public AppViewModel(IServiceProvider serviceProvider)
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

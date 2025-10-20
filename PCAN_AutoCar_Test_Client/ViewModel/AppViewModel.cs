using ReactiveUI;
using System.Windows.Controls;

namespace PCAN_AutoCar_Test_Client.ViewModel
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

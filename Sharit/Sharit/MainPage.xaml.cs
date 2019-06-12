using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sharit
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<RecordSource> RecordSources { get; set; } = new ObservableCollection<RecordSource>();
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }
        public int SliderValue { get; set; } = 2000;
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(async () =>
            {
                while (true)
                {
                    await UpdateList();
                    await Task.Delay(SliderValue);
                }
            });
        }

        async Task UpdateList()
        {
            var list = await Xamarin.Forms.DependencyService.Get<IRecordSourceManager>().GetSources();
            Device.BeginInvokeOnMainThread( () =>
            {
                foreach (var item in list)
                {
                    var oldItem = RecordSources.FirstOrDefault(x => x?.Id == item.Id);
                    if (oldItem != null)
                    {
                        oldItem.Image = item.Image;
                        oldItem.Name = item.Name;
                        oldItem.Height = item.Height;
                        oldItem.Width = item.Width;
                    }
                    else
                    {
                        RecordSources.Add(item);
                    }
                }/*
                RecordSources.Clear();
                foreach (var item in list)
                {
                    RecordSources.Add(item);
                }*/
            });
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            UpdateList();
        }
    }
}

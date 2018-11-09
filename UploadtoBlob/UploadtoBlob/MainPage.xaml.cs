using Microsoft.WindowsAzure.Storage;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace UploadtoBlob
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private MediaFile _mediaFile;
        public string URL { get; set; }

        //Picture choose from device
        private async void Button_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Error", "This is not support on your device.", "OK");
                return;
            }
            else
            {
                var mediaOption = new PickMediaOptions()
                {
                    PhotoSize = PhotoSize.Medium
                };
                _mediaFile = await CrossMedia.Current.PickPhotoAsync();
                imageView.Source = ImageSource.FromStream(() => _mediaFile.GetStream());
            }
        }
        
        //Upload picture button
        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if (_mediaFile == null)
            {
                await DisplayAlert("Error", "There was an error when trying to get your image.", "OK");
                return;
            }
            else
            {
                UploadImage(_mediaFile.GetStream());
            }
            
        }

        //Take picture from camera
        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if(!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":(No Camera available.)", "OK");
                return;
            }
            else
            {
                _mediaFile = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Sample",
                    Name = "myImage.jpg"
                });
               
                
                imageView.Source = ImageSource.FromStream(() => _mediaFile.GetStream());
                var mediaOption = new PickMediaOptions()
                {
                    PhotoSize = PhotoSize.Medium
                };
            }            
        }

        //Upload to blob function
        private async void UploadImage(Stream stream)
        {
            uploadIndicator.IsVisible = true;
            uploadIndicator.IsRunning = true;

            var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=blobacount;AccountKey=hMZuDKfdz1iGPVsV+W9V52YnsjD6F1tjdH89XIw0QM3J6FB+tdJ9IgQI+OAWHgCRKSYMK0EwGcpB0qCJI8kY+w==;EndpointSuffix=core.windows.net");
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("images");
            await container.CreateIfNotExistsAsync();
            var name = Guid.NewGuid().ToString();
            var blockBlob = container.GetBlockBlobReference($"{name}.png");
            await blockBlob.UploadFromStreamAsync(stream);
            URL = blockBlob.Uri.OriginalString;
            
            UploadedUrl.Text = URL;
            uploadIndicator.IsVisible = false;
            uploadIndicator.IsRunning = false;
            await DisplayAlert("Success", "Image uploaded to Blob Successfully!.", "OK");
        }

    }
}

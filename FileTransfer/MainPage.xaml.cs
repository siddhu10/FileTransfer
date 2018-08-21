using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FluentFTP;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FileTransfer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static bool bIsDownload = true;
        StorageFolder pickedFolder = null;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void ftpBtnText_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.FileTypeFilter.Add(".txt");

            pickedFolder = await folderPicker.PickSingleFolderAsync();
            ftpPathBox.Text = pickedFolder.Path;
        }

        private async void ftpStrtText_Click(object sender, RoutedEventArgs e)
        {
            ftpStatusText.Text = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(ftpUrlBox.Text))
                {
                    if (!string.IsNullOrEmpty(ftpPathBox.Text))
                    {
                        if (bIsDownload)
                        {
                            await HandleFTPDownload();
                        }
                        else
                        {
                            ContentDialog dialog = Helper.GetDialog();
                            dialog.Content = "FTP Upload is not supported";
                            await dialog.ShowAsync();
                        }
                    }
                    else
                    {
                        ContentDialog dialog = Helper.GetDialog();
                        dialog.Content = "Please select a path for above operation";
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    ContentDialog dialog = Helper.GetDialog();
                    dialog.Content = "Please enter FTP Server URL";
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ContentDialog dialog = Helper.GetDialog();
                dialog.Content = ex;
                await dialog.ShowAsync();
            }
        }

        private void Operation_Checked(object sender, RoutedEventArgs e)
        {
            if (ftpBtn2Text != null)
                if (ftpBtn2Text.IsChecked == true)
                    bIsDownload = false;
        }

        async Task HandleFTPDownload()
        {
            try
            {
                //Uri uri = new Uri(ftpUrlBox.Text.Trim());
                //StorageFile storageFile = await pickedFolder.CreateFileAsync("DownloadedFile.pdf", CreationCollisionOption.ReplaceExisting);

                //BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
                //DownloadOperation downloadOperation = backgroundDownloader.CreateDownload(uri, storageFile);

                //PasswordCredential passwordCredential = new PasswordCredential();
                //passwordCredential.UserName = "ebX_Developer_M2T";
                //passwordCredential.Password = "7ujmKI";
                //backgroundDownloader.ServerCredential = passwordCredential;

                //ftpBar.Visibility = Visibility.Visible;
                //await downloadOperation.StartAsync();
                //ftpBar.Visibility = Visibility.Collapsed;

                //ResponseInformation responseInformation = downloadOperation.GetResponseInformation();
                //ftpStatusText.Text = responseInformation != null ? responseInformation.StatusCode.ToString() : string.Empty;

                string strURL = ftpUrlBox.Text;
                string[] strItems = strURL.Split('/');

                string strLocalFile = pickedFolder.Path + "\\" + strItems[3];
                string strRemotePath = "/" + strItems[3];

                FtpClient ftpClient = new FtpClient("157.69.121.248", "ebX_Developer_M2T", "7ujmKI");
                await ftpClient.ConnectAsync();

                ftpBar.Visibility = Visibility.Visible;
                bool bSuccess = await Task.Run(async () =>
                {
                    bool bRet = await ftpClient.DownloadFileAsync(strLocalFile, strRemotePath, true, FtpVerify.None, null);
                    return bRet;
                });
                ftpBar.Visibility = Visibility.Collapsed;

                ftpStatusText.Text = bSuccess ? "File download completed successfully" : "File download failed";
            }
            catch (Exception ex)
            {
                ContentDialog dialog = Helper.GetDialog();
                dialog.Content = ex;
                await dialog.ShowAsync();
            }
        }
    }
}

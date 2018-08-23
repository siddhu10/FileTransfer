using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
        StorageFile pickedFile = null;

        string strHostName = string.Empty;
        string strUserName = string.Empty;
        string strPassword = string.Empty;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void ftpBtnText_Click(object sender, RoutedEventArgs e)
        {
            if (bIsDownload)
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                folderPicker.ViewMode = PickerViewMode.Thumbnail;
                folderPicker.FileTypeFilter.Add(".txt");

                pickedFolder = await folderPicker.PickSingleFolderAsync();
                if (pickedFolder != null)
                    ftpPathBox.Text = pickedFolder.Path;
            }
            else
            {
                FileOpenPicker fileOpenPicker = new FileOpenPicker();
                fileOpenPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
                fileOpenPicker.FileTypeFilter.Add("*");

                pickedFile = await fileOpenPicker.PickSingleFileAsync();
                if (pickedFile != null)
                    ftpPathBox.Text = pickedFile.Path;
            }
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
                            await HandleFTPUpload();
                        }
                    }
                    else
                    {
                        ContentDialog dialog = Helper.GetDialog();
                        dialog.Content = Helper.GetResourceString("ID_SEL_PATH");
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    ContentDialog dialog = Helper.GetDialog();
                    dialog.Content = Helper.GetResourceString("ID_SEL_FTPURL");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ContentDialog dialog = Helper.GetDialog();
                dialog.Content = ex.Message;
                await dialog.ShowAsync();
            }
        }

        private void Operation_Checked(object sender, RoutedEventArgs e)
        {
            if (ftpBtn2Text != null)
            {
                if (ftpBtn2Text.IsChecked == true)
                {
                    bIsDownload = false;
                    ftpPathBox.Header = Helper.GetResourceString("ID_BRWS_FILE");
                }
                else
                {
                    bIsDownload = true;
                    ftpPathBox.Header = Helper.GetResourceString("ID_BRWS_FOLDER");
                }
            }
        }

        async Task HandleFTPDownload()
        {
            strHostName = string.Empty;
            strUserName = string.Empty;
            strPassword = string.Empty;

            try
            {
                string strRemotePath = string.Empty;

                string strURL = ftpUrlBox.Text;
                string[] strItems = strURL.Split('/');
                for (int index = 3; index < strItems.Length; index++)
                {
                    strRemotePath += "/" + strItems[index];
                }

                string strLocalFile = pickedFolder.Path + "\\" + strItems[strItems.Length - 1];

                FetchCredentials(strItems[2], out strHostName, out strUserName, out strPassword);
                FtpClient ftpClient = new FtpClient(strHostName, strUserName, strPassword);
                await ftpClient.ConnectAsync();

                ftpBar.Visibility = Visibility.Visible;
                bool bSuccess = await Task.Run(async () =>
                {
                    bool bRet = await ftpClient.DownloadFileAsync(strLocalFile, strRemotePath, true, FtpVerify.None, null);
                    return bRet;
                });
                ftpBar.Visibility = Visibility.Collapsed;

                ftpStatusText.Foreground = bSuccess ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                ftpStatusText.Text = bSuccess ? Helper.GetResourceString("ID_DWNLD_SUCCESS") : Helper.GetResourceString("ID_DWNLD_FAIL");
            }
            catch (Exception ex)
            {
                ContentDialog dialog = Helper.GetDialog();
                dialog.Content = ex.Message;
                await dialog.ShowAsync();
            }
        }

        async Task HandleFTPUpload()
        {
            strHostName = string.Empty;
            strUserName = string.Empty;
            strPassword = string.Empty;

            try
            {
                string strLocalFile = pickedFile.Path;
                string strRemotePath = string.Empty;

                string strURL = ftpUrlBox.Text;
                string[] strItems = strURL.Split('/');
                for (int index = 3; index < strItems.Length; index++)
                {
                    strRemotePath += "/" + strItems[index];
                }
                strRemotePath += "/" + pickedFile.Name;

                FetchCredentials(strItems[2], out strHostName, out strUserName, out strPassword);
                FtpClient ftpClient = new FtpClient(strHostName, strUserName, strPassword);
                await ftpClient.ConnectAsync();

                ftpBar.Visibility = Visibility.Visible;
                bool bSuccess = await Task.Run(async () =>
                {
                    bool bRet = await ftpClient.UploadFileAsync(strLocalFile, strRemotePath, FtpExists.Overwrite, false, FtpVerify.None);
                    return bRet;
                });
                ftpBar.Visibility = Visibility.Collapsed;

                ftpStatusText.Foreground = bSuccess ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                ftpStatusText.Text = bSuccess ? Helper.GetResourceString("ID_UPLOAD_SUCCESS") : Helper.GetResourceString("ID_UPLOAD_FAIL");
            }
            catch (Exception ex)
            {
                ContentDialog dialog = Helper.GetDialog();
                dialog.Content = ex.Message;
                await dialog.ShowAsync();
            }
        }

        void FetchCredentials(string strCred, out string strHostName, out string strUserName, out string strPassword)
        {
            strHostName = string.Empty;
            strUserName = string.Empty;
            strPassword = string.Empty;

            if (!string.IsNullOrEmpty(strCred))
            {
                string[] strTemp = strCred.Split('@');

                if (strTemp.Length > 0)
                {
                    strHostName = strTemp[1];

                    string[] strCredentials = strTemp[0].Split(':');

                    if (strCredentials.Length > 0)
                    {
                        strUserName = strCredentials[0];
                        strPassword = strCredentials[1];
                    }
                }
            }
        }
    }
}

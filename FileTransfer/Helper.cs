using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace FileTransfer
{
    class Helper
    {
        private static ResourceLoader loader = new ResourceLoader();

        public static string GetResourceString(string strID)
        {
            string strValue = string.Empty;
            if (loader != null)
            {
                strValue = loader.GetString(strID);
            }
            return strValue;
        }

        public static ContentDialog GetDialog()
        {
            ContentDialog chkDialog = new ContentDialog();
            try
            {
                chkDialog.Title = GetResourceString("IDS_CHKDLG_TITLE");
                chkDialog.PrimaryButtonText = GetResourceString("IDS_CHKDLG_OK");
                chkDialog.SecondaryButtonText = GetResourceString("IDS_CHKDLG_CLOSE");
            }
            catch (Exception ex)
            {
                
            }
            return chkDialog;
        }
    }
}
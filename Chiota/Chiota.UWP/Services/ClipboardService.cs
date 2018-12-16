﻿#region References

using Chiota.Services;
using Windows.ApplicationModel.DataTransfer;

#endregion

namespace Chiota.UWP.Services
{
    public class ClipboardService : IClipboardService
    {
        public string GetTextFromClipboard()
        {
            var dataPackage = Clipboard.GetContent();
            var text = dataPackage.GetTextAsync();
            return text.GetResults();
        }

        public void SendTextToClipboard(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
    }
}

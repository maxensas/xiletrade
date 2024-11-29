namespace Xiletrade.Library.Services.Interface
{
    public interface ISendInputService
    {
        void CleanChatAndPasteClipboard();
        /// <summary>
        /// COPY to clipboard (item details from POE with CTRL-C)
        /// </summary>
        void CopyItemDetail();
        /// <summary>
        /// COPY to clipboard (item details from POE with CTRL-ALT-C)
        /// </summary>
        void CopyItemDetailAdvanced();
        void CutLastWhisperToClipboard();
        void PasteClipboard();
        void ReplyLastWhisper();

        /// <summary>
        /// Start binding CTRL key + mouse wheel TO mouse LEFT click.
        /// </summary>
        void StartMouseWheelCapture();
        /// <summary>
        /// Stop binding CTRL key + mouse wheel TO mouse LEFT click.
        /// </summary>
        void StopMouseWheelCapture();

        /// <summary>
        /// COPY Regex to clipboard and paste it into poe search bar.
        /// </summary>
        void CleanPoeSearchBarAndPasteClipboard();
    }
}
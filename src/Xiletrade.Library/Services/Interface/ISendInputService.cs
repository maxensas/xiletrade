namespace Xiletrade.Library.Services.Interface
{
    public interface ISendInputService
    {
        (string Key, ushort Code) ChatKey { get; set; }

        void CleanChatAndPasteClipboard();
        /// <summary>
        /// COPY to clipboard (item details from POE with CTRL-C)
        /// </summary>
        void CopyItemDetail();
        /// <summary>
        /// COPY to clipboard (item details from POE with CTRL-ALT-C)
        /// </summary>
        /// <remarks>not needed anymore</remarks>
        void CopyItemDetailAdvanced();
        void CutLastWhisperToClipboard();
        void PasteClipboard();
        void ReplyLastWhisper();

        /// <summary>
        /// COPY Regex to clipboard and paste it into poe search bar.
        /// </summary>
        void CleanPoeSearchBarAndPasteClipboard();
    }
}
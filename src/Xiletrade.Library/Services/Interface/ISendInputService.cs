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
    }
}
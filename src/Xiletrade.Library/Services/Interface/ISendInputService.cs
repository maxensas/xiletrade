namespace Xiletrade.Library.Services.Interface
{
    public interface ISendInputService
    {
        void CleanChatAndPasteClipboard();
        void CopyItemDetail();
        void CopyItemDetailAdvanced();
        void CutLastWhisperToClipboard();
        void PasteClipboard();
        void ReplyLastWhisper();
    }
}
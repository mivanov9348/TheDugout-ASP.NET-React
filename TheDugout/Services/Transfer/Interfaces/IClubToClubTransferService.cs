namespace TheDugout.Services.Transfer.Interfaces
{
    using TheDugout.DTOs.Transfer;
    public interface IClubToClubTransferService
    {
        Task<(bool Success, string ErrorMessage)> SendOfferAsync(TransferOfferRequest request);
    }
}

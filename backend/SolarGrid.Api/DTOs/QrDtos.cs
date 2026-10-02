/*
 * File:        QrDtos.cs
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: DTO for verifying QR code.
 * Created:     29/09/2026
 */
using System.ComponentModel.DataAnnotations;

namespace SolarGrid.Api.DTOs
{
    public class QrVerifyRequest
    {
        [Required] public string ReservationId { get; set; } = string.Empty;
        [Required] public string QrToken { get; set; } = string.Empty;
    }
}

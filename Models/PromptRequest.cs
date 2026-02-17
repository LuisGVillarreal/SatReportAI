using System.ComponentModel.DataAnnotations;

namespace SatReportAI.Models
{
    public class PromptRequest
    {
        [Required]
        [MinLength(12)]
        [MaxLength(13)]
        public string RfcOwner { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
        public string Prompt { get; set; } = string.Empty;

        [Required]
        [Range(1, 999, ErrorMessage = "PageSize debe ser mayor a 0 y menor a 1000")]
        public int PageSize { get; set; } = 10;
    }
}

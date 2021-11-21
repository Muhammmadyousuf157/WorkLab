using System.ComponentModel.DataAnnotations;

namespace WorkLabWeb.Areas.WorkSpace.Models
{
    public class JoinSessionViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter your name")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter session key")]
        public string SessionKey { get; set; }

        public string DocumentType { get; set; }
    }
}
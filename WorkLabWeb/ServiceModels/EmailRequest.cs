namespace WorkLabWeb.ServiceModels
{
    public class EmailRequest
    {
        public string Email { get; set; }

        public string Link { get; set; }

        public string Type { get; set; }

        public string Secret { get; set; }
    }
}
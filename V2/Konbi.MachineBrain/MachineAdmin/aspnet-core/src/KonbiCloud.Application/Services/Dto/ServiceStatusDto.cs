namespace KonbiCloud.Services.Dto
{
    public class ServiceStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsArchived { get; set; }
        public bool? Status { get; set; }
        public string Message { get; set; }
    }
}

namespace fnPostDatabase
{
    internal class MovieRequest
    {
        public string id { get { return Guid.NewGuid().ToString(); } }

        public string title { get; set; } = string.Empty;

        public string year { get; set; } = string.Empty;
        public string video { get; set; } = string.Empty;

        public string thumb { get; set; } = string.Empty;
    }
}

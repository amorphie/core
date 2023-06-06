namespace amorphie.core.Base
{
    public class DtoSearchBase
    {
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 100;

        public string? Keyword { get; set; }
    }
}

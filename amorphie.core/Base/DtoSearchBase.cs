using amorphie.core.Extension;

namespace amorphie.core.Base
{
    public class DtoSearchBase
    {
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 100;

        public string? Keyword { get; set; }

        public string? SortColumn { get; set; }
        public SortDirectionEnum SortDirection { get; set; } = SortDirectionEnum.Asc;
    }
}

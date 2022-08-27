using BlazorApp.Shared.CoreBase;

namespace BlazorApp.Shared.CoreDto
{
    public abstract class DtoBase : CommonBase
    {
        public bool IsNew { get; set; }
    }
}

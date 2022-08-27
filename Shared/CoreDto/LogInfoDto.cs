using System.Collections.Generic;

namespace BlazorApp.Shared.CoreDto
{
    public class LogInfoDto : DtoBase
    {
        public List<LogInfoItemDto> Logs { get; set; }

        public string Symbol { get; set; }

        public LogInfoDto()
        {
            Logs = new List<LogInfoItemDto>();
            Symbol = string.Empty;
        }
    }
}

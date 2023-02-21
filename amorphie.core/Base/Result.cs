using amorphie.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.Base
{
    public sealed class Result
    {
        public string Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MessageDetail { get; set; } = string.Empty;

        public Result(Status status, string message)
        {
            Status = status.ToString();
            Message = message;
        }

        public Result(Status status, string message, string messageDetail) : this(status, message)
        {
            MessageDetail = messageDetail;
        }
    }
}

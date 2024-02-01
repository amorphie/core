using amorphie.core.Enums;
using amorphie.core.IBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amorphie.core.Base
{
    public sealed class Response : IResponse
    {
        public Result Result { get; set; }
        public Response()
        {
            Result = new(Status.Error, "");
        }
        public object Data { get; set; } = default!;
    }
    public sealed class Response<T> : IResponse<T>
    {
        public Response()
        {
            Result = new(Status.Error, "");
        }
        public Result Result { get; set; }
        public T Data { get; set; } = default!;
    }
    public sealed class NoDataResponse : IResponse
    {
        public NoDataResponse()
        {
            Result = new(Status.Error, "");
        }
        public Result Result { get; set; }
    }
}

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
        public static Response Error(string message)
        {
            return new Response
            {
                Result = new Result(Status.Error, message)
            };
        }
        public static Response Error(string message, object data)
        {
            return new Response
            {
                Result = new Result(Status.Error, message),
                Data = data
            };
        }
        public static Response Success(string message)
        {
            return new Response
            {
                Result = new Result(Status.Success, message)
            };
        }
        public static Response Success(string message, object data)
        {
            return new Response
            {
                Result = new Result(Status.Success, message),
                Data = data
            };
        }
    }
    public sealed class Response<T> : IResponse<T>
    {
        public Response()
        {
            Result = new(Status.Error, "");
        }
        public Result Result { get; set; }
        public T Data { get; set; } = default!;
        public static Response<T> Error(string message)
        {
            return new Response<T>
            {
                Result = new Result(Status.Error, message)
            };
        }
        public static Response<T> Error(string message, T data)
        {
            return new Response<T>
            {
                Result = new Result(Status.Error, message),
                Data = data
            };
        }
        public static Response<T> Success(string message)
        {
            return new Response<T>
            {
                Result = new Result(Status.Success, message)
            };
        }
        public static Response<T> Success(string message, T data)
        {
            return new Response<T>
            {
                Result = new Result(Status.Success, message),
                Data = data
            };
        }
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

using amorphie.core.Base;
using amorphie.core.Enums;
using amorphie.core.IBase;
using System.Xml.Linq;
using System.Xml.Schema;

namespace amorphie.core
{
    public class example2
    {
        public IResponse Result()
        {
            return new Response
            {
                Data = "Example object data",
                Result = new Result(Status.Success, "Message")
            };
        }

        public IResponse Resultx()
        {
            return new Response
            {
                Data = "Example object data",
                Result = new Result(Status.Success, "Message", "Message detail")
            };
        }

        public IResponse<Staff> Result2()
        {
            return new Response<Staff>
            {
                Data = new Staff() { Id=1,Name="Alice"},
                Result = new Result(Status.Success, "Listing successful")
            };
        }

        public IResponse<List<Staff>> Result3()
        {
            var list = new List<Staff>() { new Staff() { Id = 1, Name = "Tom" }, new Staff() { Id = 1, Name = "Alice" } };

            return new Response<List<Staff>>
            {
                Data = list,
                Result = new Result(Status.Success, "Getirme başarılı")
            };
        }

        public IResponse<NoData2> Result4()
        {
            return new Response<NoData2>
            {
                Result = new Result(Enums.Status.Success, "Delete successful")
            };
        }
        public IResponse Result5()
        {
            return new NoDataResponse
            {
                Result = new Result(Enums.Status.Success, "Delete successful")
            };
        }
    }

    public class Staff
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class NoData2
    {

    }
}

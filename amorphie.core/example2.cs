using amorphie.core.Base;
using amorphie.core.Enums;
using amorphie.core.IBase;
using System.Xml.Schema;

namespace amorphie.core
{
    public class example2
    {
        public IResponse Result()
        {
            return new Response
            {
                Data = "Mehmet az daha patlayacaktık.",
                Result = new Result(Status.Success, "Kayıt başarılı","ddfdfdf")
            };
        }

        public IResponse<Personel2> Result2()
        {
            return new Response<Personel2>
            {
                Data = new Personel2() { Id=1,Name="elma"},
                Result = new Result(Status.Success, "Getirme başarılı")
            };
        }

        public IResponse<List<Personel2>> Result3()
        {
            var list = new List<Personel2>() { new Personel2() { Id = 1, Name = "elma" } };

            return new Response<List<Personel2>>
            {
                Data = list,
                Result = new Result(Status.Success, "Getirme başarılı")
            };

            //return new Response
            //{
            //    Result = new Result(Enums.Status.Success, "Getirme başarılı")
            //};
        }

        public IResponse<NoData2> Result4()
        {
            return new Response<NoData2>
            {
                Result = new Result(Enums.Status.Success, "Kayıt başarılı")
            };
        }
        public IResponse Result5()
        {
            return new NoDataResponse
            {
                Result = new Result(Enums.Status.Success, "Kayıt başarılı")
            };
        }
    }

    public class Personel2
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class NoData2
    {

    }
}

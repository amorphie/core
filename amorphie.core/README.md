
# amorphie.core

Simple tag management service for developers..

Features;

1. Response types
2. Enum structures
3. Basic classes

### Response types

```c#
// When you want to return object data and message..
public IResponse Result()
{
    return new Response
    {
       Data = "Example object data",
       Result = new Result(Status.Success, "Message")
    };
}

// When you want to return object data, message and message detail.
public IResponse Result()
{
    return new Response
    {
       Data = "Example object data",
       Result = new Result(Status.Success, "Message", "Message detail")
    };
}

// To return 1 entity.
public IResponse<Staff> Result()
{
    return new Response<Staff>
    {
        Data = new Staff() { Id=1,Name="Alice"},
        Result = new Result(Status.Success, "Listing successful")
    };
}

// To list.
public IResponse<List<Staff>> Result()
{
    var list = new List<Staff>() { new Staff() { Id = 1, Name = "Tom" }, new Staff() { Id = 1, Name = "Alice" } };

    return new Response<List<Staff>>
    {
       Data = list,
       Result = new Result(Status.Success, "Getirme başarılı")
     };
}

//Only when you want to return the status and explanation.
public IResponse Result()
{
    return new NoDataResponse
    {
       Result = new Result(Status.Success, "Delete successful")
    };
}
```

### Enums

1. Status

```
public enum Status
{
  [Description("Used for all successful transactions")]
  Success = 1,
  [Description("Used for all faulty and warning operations")]
  Error = 2,
}
```

2. StatusType

```
[Flags]
public enum StatusType
{
  New = 2,
  Active = 4,
  InProgress = 8,
  Passive = 16,
  Completed = 32,
  LockedInFlow = 256,
  All = New | Active | InProgress | Passive
}
```

3. StatusType

```
public enum HttpMethodType : byte 
{ 
    CONNECT, 
    DELETE, 
    GET, 
    HEAD, 
    OPTIONS, 
    POST, 
    PUT, 
    TRACE 
}
```

### Base Classes

- EntityBase //Base class includes id and other common proporties
- EntityBaseWithOutId //Covers common properties only
- DtoBase //Base class includes id and other common proporties
- DtoEntityBaseWithOutId //Covers common properties only
- Translation //This table can be used directly for the table, table class where the language values ​​are kept.
- EntityBaseLog //Base class for log tables

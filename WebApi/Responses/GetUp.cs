namespace WebApi.Responses;

record GetUp()
{
    public int Code => 200;
    public string Message => "pong";
}
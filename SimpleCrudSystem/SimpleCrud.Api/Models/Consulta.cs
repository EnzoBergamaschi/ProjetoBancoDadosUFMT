using System.ComponentModel.DataAnnotations;

namespace SimpleCrud.Api.Models;

public class Consulta
{
    public int codm { get; set; }
    public int codp { get; set; }
    public DateTime data { get; set; }
    public TimeSpan hora { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleCrud.Api.Models;

public class Paciente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int codp { get; set; }
    public string nome { get; set; } = string.Empty;
    public int idade { get; set; }
    public string cidade { get; set; } = string.Empty;
    public string RG { get; set; } = string.Empty;
    public string problema { get; set; } = string.Empty;
}

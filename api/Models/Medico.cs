using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleCrud.Api.Models;

public class Medico
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int codm { get; set; }
    public string nome { get; set; } = string.Empty;
    public int idade { get; set; }
    public string especialidade { get; set; } = string.Empty;
    public string RG { get; set; } = string.Empty;
    public string cidade { get; set; } = string.Empty;
    public int? nroa { get; set; }
}

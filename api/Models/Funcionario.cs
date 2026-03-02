using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleCrud.Api.Models;

public class Funcionario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int codf { get; set; }
    public string nome { get; set; } = string.Empty;
    public int idade { get; set; }
    public string RG { get; set; } = string.Empty;
    public decimal salario { get; set; }
    public string depto { get; set; } = string.Empty;
    public int tempoServico { get; set; }
}

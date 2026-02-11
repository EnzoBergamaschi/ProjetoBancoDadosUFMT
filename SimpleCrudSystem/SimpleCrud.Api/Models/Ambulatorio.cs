using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleCrud.Api.Models;

public class Ambulatorio
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int nroa { get; set; }
    public int andar { get; set; }
    public int capacidade { get; set; }
}

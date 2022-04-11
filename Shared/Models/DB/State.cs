using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DB;

[Table("States")]
public class State
{
    public int ID { get; set; }
    [Column(TypeName = "VARCHAR(5)")]
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
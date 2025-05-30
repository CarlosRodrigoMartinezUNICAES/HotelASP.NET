// HotelASP.NET/Models/HabitacionModel.cs
using System.ComponentModel.DataAnnotations;

namespace HotelASP.NET.Models
{
    public class HabitacionModel
    {
        public int IdHabitacion { get; set; }

        [Required(ErrorMessage = "El número es obligatorio")]
        [StringLength(10, ErrorMessage = "Máximo 10 caracteres")]
        public string NumeroHabitacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione un tipo")]
        public int IdTipoHabitacion { get; set; }

        [Required(ErrorMessage = "Seleccione un estado")]
        public string Estado { get; set; } = "disponible";

        [Required(ErrorMessage = "El piso es obligatorio")]
        [Range(1, 20, ErrorMessage = "Piso entre 1 y 20")]
        public int Piso { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string Detalles { get; set; } = string.Empty;
    }
}
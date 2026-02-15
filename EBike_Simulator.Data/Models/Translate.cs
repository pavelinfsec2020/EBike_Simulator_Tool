using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EBike_Simulator.Data.Models
{
    public class Translation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Key { get; set; } = string.Empty;     

        public string? RuString { get; set; }              

        public string? EnString { get; set; }               
    }
}

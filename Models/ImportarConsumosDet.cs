using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImplementadorCUAD.Models
{
    public class ImportarConsumosDet
    {
        public int Icd_Id { get; set; }
        public string? Entidad { get; set; }
        public int CodigoConsumo { get; set; }
        public int NroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Monto { get; set; }
    }
}


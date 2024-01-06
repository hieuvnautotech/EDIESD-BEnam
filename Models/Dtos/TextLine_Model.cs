using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESD_EDI_BE.Models.Dtos
{
    public class TextLine_Model
    {
        public decimal Qty_No_2 { get; set; }
        public decimal Qty_No_3 { get; set; }
        public decimal Qty_No_9 { get; set; }
        public decimal Qty_No_10 { get; set; }
        public decimal Qty_No_11 { get; set; }
        public decimal Qty_No_12 { get; set; }
        public decimal Qty_No_13 { get; set; }
        public decimal Qty_No_14 { get; set; }

        public decimal HMI_Qty_1 { get; set; }
        public decimal HMI_Qty_4 { get; set; }

        public decimal Loadcell_Qty_1 { get; set; }
        public decimal? Loadcell_Qty_2 { get; set; }
        public decimal? Loadcell_Qty_3 { get; set; }

        public string? Item_Code { get; set; }
        public string? PrcQ01 { get; set; }
        public string? DateTimeStr { get; set; }
        public string? YYYYMMDDHH { get; set; }
        public string? File_Nm { get; set; }
        public bool IsNotProcess { get; set; }

        public object this[string propertyName]
        {
            get => GetType().GetProperty(propertyName).GetValue(this, null);
            set => GetType().GetProperty(propertyName).SetValue(this, value, null);
        }
    }
}
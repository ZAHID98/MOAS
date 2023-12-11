namespace MOAS.Models
{
    public class Equipment
    {
        public int EquipmentID { get; set; }
        public long Kunnr { get; set; }
        public string EQUIPMENT { get; set; }
        public string? EQUIPMENT_MAKE { get; set; }
        public string? EQUIPMENT_MODEL { get; set; }
        public string? EQUIPMENT_SERIAL { get; set; }

        public string DisplayText
        {
            get
            {
                return  EQUIPMENT + "-" + EQUIPMENT_SERIAL;
            }
        }



        public string SearchText
        {
            get
            {
                return $"{EQUIPMENT} {EQUIPMENT_MAKE} {EQUIPMENT_MODEL} {EQUIPMENT_SERIAL}";
            }
        }



    }
}

namespace Master_pice.ViewModel
{
    public class ProductViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageURL { get; set; }  // الصورة الرئيسية

        public string Type { get; set; }

        // الحقول الإضافية المطلوبة للـ Laptop
        public string Brand { get; set; }
        public string Processor { get; set; }
        public string RAM { get; set; }
        public string Storage { get; set; }
        public string GPU { get; set; }
        public decimal ScreenSize { get; set; }
        public int Stock { get; set; }

        public string Category { get; set; }

        // الحقل الجديد لدعم الصور الإضافية
        public List<string> AdditionalImageURLs { get; set; } = new List<string>();  // صور إضافية
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Manufactures.Application.GarmentExpenditureGoods.Queries.GetReceiptFinishedGoods
{
    public class GarmentReceiptFinishedGoodDto
    {
        public GarmentReceiptFinishedGoodDto()
        {
        }

        public string BonMasuk { get; internal set; }
        public DateTimeOffset TglBonMasuk { get; internal set; }
        public string KodeBarang { get; internal set; }
        public string NamaBarang { get; internal set; }
        public string UnitQtyName { get; internal set; }
        public double QtyProcess { get; internal set; }
        public double QtySubcon { get; internal set; }
        public string Gudang { get; internal set; }

        public GarmentReceiptFinishedGoodDto(GarmentReceiptFinishedGoodDto garmentGoodsReceipt)
        {
            BonMasuk = garmentGoodsReceipt.BonMasuk;
            TglBonMasuk = garmentGoodsReceipt.TglBonMasuk;
            KodeBarang = garmentGoodsReceipt.KodeBarang;
            NamaBarang = garmentGoodsReceipt.NamaBarang;
            UnitQtyName = garmentGoodsReceipt.UnitQtyName;
            QtyProcess = garmentGoodsReceipt.QtyProcess;
            QtySubcon = garmentGoodsReceipt.QtySubcon;
            Gudang = garmentGoodsReceipt.Gudang;
        }

    }
}

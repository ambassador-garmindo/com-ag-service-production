using System;
using System.Collections.Generic;
using System.Text;

namespace Manufactures.Application.GarmentExpenditureGoods.Queries.GetReceiptFinishedGoods
{
    public class GarmentReceiptFinishedGoodListViewModel
    {
        public List<GarmentReceiptFinishedGoodDto> garmentGoodsReceipt { get; set; }
        public int count { get; set; }
    }
}

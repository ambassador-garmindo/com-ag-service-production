using Manufactures.Application.GarmentExpenditureGoods.Queries.GetMutationExpenditureGoods;
using Manufactures.Application.GarmentExpenditureGoods.Queries.GetReceiptFinishedGoods;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Manufactures.Tests.Queries.GarmentExpenditureGoods.GarmentMutationExpenditureGood
{
    public class GarmentReceiptFinishedGoodDtoTest
    {
        [Fact]
        public void ShouldSucces_Instantiate()
        {
            GarmentReceiptFinishedGoodDto wIPDto = new GarmentReceiptFinishedGoodDto();
            GarmentReceiptFinishedGoodDto dto = new GarmentReceiptFinishedGoodDto(wIPDto);
            Assert.NotNull(dto);

        }
    }
}

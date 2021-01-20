// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ExtCore.Data.EntityFramework;
using Manufactures.Data.EntityFrameworkCore.GarmentAdjustments.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentAvalComponents.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentAvalProducts.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentBalanceStockProductions.Config;
using Manufactures.Data.EntityFrameworkCore.GarmentComodityPrices.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentCuttingIns.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentCuttingOuts.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentDeliveryReturns.Config;
using Manufactures.Data.EntityFrameworkCore.GarmentExpenditureGoodReturns.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentExpenditureGoods.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentFinishedGoodStocks.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentFinishingIns.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentFinishingOuts.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentLoadings.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentPreparings.Config;
using Manufactures.Data.EntityFrameworkCore.GarmentScrapClassifications.Config;
using Manufactures.Data.EntityFrameworkCore.GarmentScrapTransactions.Config;
using Manufactures.Data.EntityFrameworkCore.GarmentSewingDOs.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentSewingIns.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentSewingOuts.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentSubconCuttingOuts.Configs;
using Microsoft.EntityFrameworkCore;

namespace Manufactures.Data.EntityFrameworkCore
{
	public class EntityRegistrar : IEntityRegistrar
	{
		public void RegisterEntities(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new GarmentPreparingConfig());
			modelBuilder.ApplyConfiguration(new GarmentPreparingItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentAvalProductConfig());
			modelBuilder.ApplyConfiguration(new GarmentAvalProductItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentCuttingInConfig());
			modelBuilder.ApplyConfiguration(new GarmentCuttingInItemConfig());
			modelBuilder.ApplyConfiguration(new GarmentCuttingInDetailConfig());

			modelBuilder.ApplyConfiguration(new GarmentDeliveryReturnConfig());
			modelBuilder.ApplyConfiguration(new GarmentDeliveryReturnItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentCuttingOutConfig());
			modelBuilder.ApplyConfiguration(new GarmentCuttingOutItemConfig());
			modelBuilder.ApplyConfiguration(new GarmentCuttingOutDetailConfig());

			modelBuilder.ApplyConfiguration(new GarmentSewingDOConfig());
			modelBuilder.ApplyConfiguration(new GarmentSewingDOItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentLoadingConfig());
			modelBuilder.ApplyConfiguration(new GarmentLoadingItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentSewingInConfig());
			modelBuilder.ApplyConfiguration(new GarmentSewingInItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentSewingOutConfig());
			modelBuilder.ApplyConfiguration(new GarmentSewingOutItemConfig());
			modelBuilder.ApplyConfiguration(new GarmentSewingOutDetailConfig());

			modelBuilder.ApplyConfiguration(new GarmentFinishingInConfig());
			modelBuilder.ApplyConfiguration(new GarmentFinishingInItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentComodityPriceConfig());

			modelBuilder.ApplyConfiguration(new GarmentAvalComponentConfig());
			modelBuilder.ApplyConfiguration(new GarmentAvalComponentItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentSubconCuttingConfig());
			modelBuilder.ApplyConfiguration(new GarmentSubconCuttingRelationConfig());

			modelBuilder.ApplyConfiguration(new GarmentFinishingOutConfig());
			modelBuilder.ApplyConfiguration(new GarmentFinishingOutItemConfig());
			modelBuilder.ApplyConfiguration(new GarmentFinishingOutDetailConfig());


			modelBuilder.ApplyConfiguration(new GarmentFinishedGoodStockConfig());
			modelBuilder.ApplyConfiguration(new GarmentFinishedGoodStockHistoryConfig());


			modelBuilder.ApplyConfiguration(new GarmentScrapClassificationConfig());
			modelBuilder.ApplyConfiguration(new GarmentScrapTransactionConfig());
			modelBuilder.ApplyConfiguration(new GarmentScrapTransactionItemConfig());
			modelBuilder.ApplyConfiguration(new GarmentScrapDestinationConfig());
			modelBuilder.ApplyConfiguration(new GarmentScrapSourceConfig());
			modelBuilder.ApplyConfiguration(new GarmentScrapStockConfig());

			modelBuilder.ApplyConfiguration(new GarmentAdjustmentConfig());
			modelBuilder.ApplyConfiguration(new GarmentAdjustmentItemConfig());

            modelBuilder.ApplyConfiguration(new GarmentExpenditureGoodConfig());
            modelBuilder.ApplyConfiguration(new GarmentExpenditureGoodItemConfig());

            modelBuilder.ApplyConfiguration(new GarmentExpenditureGoodReturnConfig());
            modelBuilder.ApplyConfiguration(new GarmentExpenditureGoodReturnItemConfig());

			modelBuilder.ApplyConfiguration(new GarmentBalanceStockProductionConfig());
        }
	}
}
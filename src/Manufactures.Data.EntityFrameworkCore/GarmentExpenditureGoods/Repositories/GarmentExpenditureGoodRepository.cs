using Infrastructure.Data.EntityFrameworkCore;
using Infrastructure.Data.EntityFrameworkCore.Utilities;
using Manufactures.Domain.GarmentExpenditureGoods;
using Manufactures.Domain.GarmentExpenditureGoods.ReadModels;
using Manufactures.Domain.GarmentExpenditureGoods.Repositories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manufactures.Data.EntityFrameworkCore.GarmentExpenditureGoods.Repositories
{
    public class GarmentExpenditureGoodRepository : AggregateRepostory<GarmentExpenditureGood, GarmentExpenditureGoodReadModel>, IGarmentExpenditureGoodRepository
    {
        public IQueryable<GarmentExpenditureGoodReadModel> Read(int page, int size, string order, string keyword, string filter)
        {
            var data = Query;

            Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
            data = QueryHelper<GarmentExpenditureGoodReadModel>.Filter(data, FilterDictionary);

            List<string> SearchAttributes = new List<string>
            {
                "ExpenditureGoodNo",
                "ExpenditureType",
                "Article",
                "RONo",
                "UnitCode",
                "UnitName",
                "ContractNo",
                "Invoice",
                "BuyerName"
            };
            data = QueryHelper<GarmentExpenditureGoodReadModel>.Search(data, SearchAttributes, keyword);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            data = OrderDictionary.Count == 0 ? data.OrderByDescending(o => o.ModifiedDate) : QueryHelper<GarmentExpenditureGoodReadModel>.Order(data, OrderDictionary);

            //data = data.Skip((page - 1) * size).Take(size);

            return data;
        }

        public IQueryable<GarmentExpenditureGoodReadModel> ReadComplete(int page, int size, string order, string keyword, string filter)
        {
            var data = Query;

            Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
            data = QueryHelper<GarmentExpenditureGoodReadModel>.Filter(data, FilterDictionary);

            List<string> SearchAttributes = new List<string>
            {
                "RONo",
            };
            data = QueryHelper<GarmentExpenditureGoodReadModel>.Search(data, SearchAttributes, keyword);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            data = OrderDictionary.Count == 0 ? data.OrderByDescending(o => o.ModifiedDate) : QueryHelper<GarmentExpenditureGoodReadModel>.Order(data, OrderDictionary);

            //data = data.Skip((page - 1) * size).Take(size);

            return data;
        }

        public double BasicPriceByRO(string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            long unitId = 0;
            bool hasUnitFilter = FilterDictionary.ContainsKey("UnitId") && long.TryParse(FilterDictionary["UnitId"], out unitId);
            bool hasRONoFilter = FilterDictionary.ContainsKey("RONo");
            bool hasCustomsFilter = FilterDictionary.ContainsKey("CustomsCategory");
            string RONo = hasRONoFilter ? (FilterDictionary["RONo"] ?? "").Trim() : "";
            string CustomsCategory = hasCustomsFilter ? (FilterDictionary["CustomsCategory"] ?? "").Trim() : "";
            var dataHeader = Query.Where(a => a.RONo == RONo && a.UnitId == unitId).Include(a => a.Items).Where(a => a.Items.Any(x => x.CustomsCategory == CustomsCategory));

            double priceTotal = 0;
            double qtyTotal = 0;

            foreach (var data in dataHeader)
            {
                priceTotal += data.Items.Sum(a => a.Price);
                qtyTotal += data.Items.Sum(a => a.Quantity);
            }

            double basicPrice = priceTotal / qtyTotal;

            return basicPrice;
        }

        protected override GarmentExpenditureGood Map(GarmentExpenditureGoodReadModel readModel)
        {
            return new GarmentExpenditureGood(readModel);
        }
    }
}

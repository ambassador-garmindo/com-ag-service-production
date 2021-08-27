using ExtCore.Data.Abstractions;
using Infrastructure.Domain.Queries;
using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using Manufactures.Domain.GarmentAdjustments.Repositories;
using Manufactures.Domain.GarmentExpenditureGoodReturns.Repositories;
using Manufactures.Domain.GarmentExpenditureGoods.Repositories;
using Manufactures.Domain.GarmentFinishedGoodStocks.Repositories;
using Manufactures.Domain.GarmentFinishingOuts.Repositories;
using Manufactures.Domain.GarmentFinishingIns.Repositories;
using Manufactures.Domain.GarmentSewingOuts.Repositories;
using Manufactures.Domain.GarmentSewingIns.Repositories;
using Manufactures.Domain.GarmentLoadings.Repositories;
using Manufactures.Domain.GarmentSewingDOs.Repositories;
using Manufactures.Domain.GarmentCuttingOuts.Repositories;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentPreparings.Repositories;
using Manufactures.Domain.MonitoringProductionStockFlow;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Manufactures.Domain.GarmentComodityPrices.Repositories;
using Infrastructure.External.DanLirisClient.Microservice;
using Infrastructure.External.DanLirisClient.Microservice.MasterResult;
using Newtonsoft.Json;
using System.Net.Http;

namespace Manufactures.Application.GarmentExpenditureGoods.Queries.GetMutationExpenditureGoods
{
    public class GarmentMutationExpenditureGoodQueryHandler : IQueryHandler<GetMutationExpenditureGoodsQuery, GarmentMutationExpenditureGoodListViewModel>
    {
        private readonly IStorage _storage;
        private readonly IGarmentBalanceMonitoringProductionStockFlowRepository garmentBalanceMonitoringProductionStockFlowRepository;
        private readonly IGarmentAdjustmentRepository garmentAdjustmentRepository;
        private readonly IGarmentAdjustmentItemRepository garmentAdjustmentItemRepository;
        private readonly IGarmentFinishedGoodStockRepository garmentFinishedGoodStockRepository;
        private readonly IGarmentFinishedGoodStockHistoryRepository garmentFinishedGoodStockHistoryRepository;
        private readonly IGarmentExpenditureGoodRepository garmentExpenditureGoodRepository;
        private readonly IGarmentExpenditureGoodItemRepository garmentExpenditureGoodItemRepository;
        private readonly IGarmentExpenditureGoodReturnRepository garmentExpenditureGoodReturnRepository;
        private readonly IGarmentExpenditureGoodReturnItemRepository garmentExpenditureGoodReturnItemRepository;
        private readonly IGarmentFinishingOutRepository garmentFinishingOutRepository;
        private readonly IGarmentFinishingOutItemRepository garmentFinishingOutItemRepository;
        private readonly IGarmentFinishingInRepository garmentFinishingInRepository;
        private readonly IGarmentFinishingInItemRepository garmentFinishingInItemRepository;
        private readonly IGarmentSewingOutRepository garmentSewingOutRepository;
        private readonly IGarmentSewingOutItemRepository garmentSewingOutItemRepository;
        private readonly IGarmentSewingOutDetailRepository garmentSewingOutDetailRepository;
        private readonly IGarmentSewingInItemRepository garmentSewingInItemRepository;
        private readonly IGarmentSewingInRepository garmentSewingInRepository;
        private readonly IGarmentLoadingRepository garmentLoadingRepository;
        private readonly IGarmentLoadingItemRepository garmentLoadingItemRepository;
        private readonly IGarmentSewingDORepository garmentSewingDORepository;
        private readonly IGarmentSewingDOItemRepository garmentSewingDOItemRepository;
        private readonly IGarmentCuttingOutRepository garmentCuttingOutRepository;
        private readonly IGarmentCuttingOutItemRepository garmentCuttingOutItemRepository;
        private readonly IGarmentCuttingOutDetailRepository garmentCuttingOutDetailRepository;
        private readonly IGarmentCuttingInRepository garmentCuttingInRepository;
        private readonly IGarmentCuttingInItemRepository garmentCuttingInItemRepository;
        private readonly IGarmentCuttingInDetailRepository garmentCuttingInDetailRepository;
        private readonly IGarmentPreparingRepository garmentPreparingRepository;
        private readonly IGarmentPreparingItemRepository garmentPreparingItemRepository;
        protected readonly IHttpClientService _http;

        public GarmentMutationExpenditureGoodQueryHandler(IStorage storage, IServiceProvider serviceProvider)
        {
            _storage = storage;
            garmentBalanceMonitoringProductionStockFlowRepository = storage.GetRepository<IGarmentBalanceMonitoringProductionStockFlowRepository>();
            garmentPreparingRepository = storage.GetRepository<IGarmentPreparingRepository>();
            garmentPreparingItemRepository = storage.GetRepository<IGarmentPreparingItemRepository>();
            garmentCuttingInRepository = storage.GetRepository<IGarmentCuttingInRepository>();
            garmentCuttingInItemRepository = storage.GetRepository<IGarmentCuttingInItemRepository>();
            garmentCuttingInDetailRepository = storage.GetRepository<IGarmentCuttingInDetailRepository>();
            garmentCuttingOutRepository = storage.GetRepository<IGarmentCuttingOutRepository>();
            garmentCuttingOutItemRepository = storage.GetRepository<IGarmentCuttingOutItemRepository>();
            garmentCuttingOutDetailRepository = storage.GetRepository<IGarmentCuttingOutDetailRepository>();
            garmentSewingDORepository = storage.GetRepository<IGarmentSewingDORepository>();
            garmentSewingDOItemRepository = storage.GetRepository<IGarmentSewingDOItemRepository>();
            garmentLoadingRepository = storage.GetRepository<IGarmentLoadingRepository>();
            garmentLoadingItemRepository = storage.GetRepository<IGarmentLoadingItemRepository>();
            garmentSewingInRepository = storage.GetRepository<IGarmentSewingInRepository>();
            garmentSewingInItemRepository = storage.GetRepository<IGarmentSewingInItemRepository>();
            garmentSewingOutRepository = storage.GetRepository<IGarmentSewingOutRepository>();
            garmentSewingOutItemRepository = storage.GetRepository<IGarmentSewingOutItemRepository>();
            garmentSewingOutDetailRepository = storage.GetRepository<IGarmentSewingOutDetailRepository>();
            garmentFinishingInRepository = storage.GetRepository<IGarmentFinishingInRepository>();
            garmentFinishingInItemRepository = storage.GetRepository<IGarmentFinishingInItemRepository>();
            garmentFinishingOutRepository = storage.GetRepository<IGarmentFinishingOutRepository>();
            garmentFinishingOutItemRepository = storage.GetRepository<IGarmentFinishingOutItemRepository>();
            garmentExpenditureGoodRepository = storage.GetRepository<IGarmentExpenditureGoodRepository>();
            garmentExpenditureGoodItemRepository = storage.GetRepository<IGarmentExpenditureGoodItemRepository>();
            garmentFinishedGoodStockRepository = storage.GetRepository<IGarmentFinishedGoodStockRepository>();
            garmentFinishedGoodStockHistoryRepository = storage.GetRepository<IGarmentFinishedGoodStockHistoryRepository>();
            garmentExpenditureGoodReturnRepository = storage.GetRepository<IGarmentExpenditureGoodReturnRepository>();
            garmentExpenditureGoodReturnItemRepository = storage.GetRepository<IGarmentExpenditureGoodReturnItemRepository>();

            garmentAdjustmentRepository = storage.GetRepository<IGarmentAdjustmentRepository>();
            garmentAdjustmentItemRepository = storage.GetRepository<IGarmentAdjustmentItemRepository>();

            _http = serviceProvider.GetService<IHttpClientService>();
        }

        class mutationView
        {
            public double SaldoQtyFin { get; internal set; }
            public double QtyFin { get; internal set; }
            public double AdjFin { get; internal set; }
            public double Retur { get; internal set; }
            public double QtyExpend { get; internal set; }
            public string ComodityCode { get; internal set; }
            public string ComodityName { get; internal set; }
        }

        public async Task<GarmentProductResult> GetProducts(string codes, string token)
        {
            GarmentProductResult garmentProduct = new GarmentProductResult();

            var httpContent = new StringContent(JsonConvert.SerializeObject(codes), Encoding.UTF8, "application/json");

            var garmentProductionUri = MasterDataSettings.Endpoint + $"master/garmentProducts/byCode";
            var httpResponse = await _http.SendAsync(HttpMethod.Get, garmentProductionUri, token, httpContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                var contentString = await httpResponse.Content.ReadAsStringAsync();
                Dictionary<string, object> content = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentString);
                var dataString = content.GetValueOrDefault("data").ToString();

                var listdata = JsonConvert.DeserializeObject<List<GarmentProductViewModel>>(dataString);

                foreach (var i in listdata)
                {
                    garmentProduct.data.Add(i);
                }
            }

            return garmentProduct;
        }

        //async Task<CustomsCategory> GetCustomsCategory(List<int> UENItemId, string token)
        //{
        //    CustomsCategory UENItemList = new CustomsCategory();

        //    var listUENItemId = string.Join(",", UENItemId.Distinct());
        //    var uenItemUri = SalesDataSettings.Endpoint + $"unit-expenditure-notes/items/{listUENItemId}";
        //    var httpResponse = await _http.GetAsync(uenItemUri, token);

        //    if (httpResponse.IsSuccessStatusCode)
        //    {
        //        var contentString = await httpResponse.Content.ReadAsStringAsync();
        //        Dictionary<string, object> content = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentString);
        //        var dataString = content.GetValueOrDefault("data").ToString();
        //        var listData = JsonConvert.DeserializeObject<List<UENItemViewModel>>(dataString);

        //        foreach (var item in UENItemId)
        //        {
        //            var data = listData.SingleOrDefault(s => s.CustomsCategory == "LOKAL FASILITAS" || s.CustomsCategory == "IMPORT FASILITAS");
        //            if (data != null)
        //            {
        //                UENItemList.data.Add(data);
        //            }
        //        }
        //    }

        //    return UENItemList;
        //}

        public async Task<GarmentMutationExpenditureGoodListViewModel> Handle(GetMutationExpenditureGoodsQuery request, CancellationToken cancellationToken)
        {
            GarmentMutationExpenditureGoodListViewModel expenditureGoodListViewModel = new GarmentMutationExpenditureGoodListViewModel();
            List<GarmentMutationExpenditureGoodDto> mutationExpenditureGoodDto = new List<GarmentMutationExpenditureGoodDto>();

            DateTimeOffset dateFrom = new DateTimeOffset(request.dateFrom, new TimeSpan(7, 0, 0));
            DateTimeOffset dateTo = new DateTimeOffset(request.dateTo, new TimeSpan(7, 0, 0));

            var finishingbarangjadiid = (from a in (from aa in garmentFinishingOutRepository.Query
                                                    where aa.FinishingOutDate.AddHours(7).Date <= dateTo.Date
                                                    && aa.FinishingTo == "GUDANG JADI"
                                                    && aa.Deleted == false
                                                    select new
                                                    {
                                                       aa.RONo,
                                                       aa.Identity,
                                                       aa.FinishingOutDate,
                                                       aa.FinishingOutNo
                                                    })
                                         join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId
                                         join c in garmentCuttingOutItemRepository.Query on b.ProductCode equals c.ProductCode
                                         join d in (from dd in garmentCuttingOutRepository.Query
                                                    where dd.CuttingOutType == "SEWING"
                                                    select new
                                                    {
                                                       dd.RONo,
                                                       dd.Identity,
                                                       dd.UnitCode,
                                                       dd.CutOutNo
                                                    }) on c.CutOutId equals d.Identity
                                         join e in garmentCuttingInDetailRepository.Query on b.ProductCode equals e.ProductCode
                                         join f in garmentCuttingInItemRepository.Query on e.CutInItemId equals f.Identity
                                         join g in (from gg in garmentCuttingInRepository.Query
                                                    where gg.CuttingFrom == "PREPARING"
                                                    select new
                                                    {
                                                       gg.RONo,
                                                       gg.Identity,
                                                       gg.UnitCode,
                                                       gg.CutInNo
                                                    }) on f.CutInId equals g.Identity
                                         join h in (from hh in garmentPreparingItemRepository.Query
                                                    where hh.CustomsCategory == "LOKAL FASILITAS" || hh.CustomsCategory == "IMPORT FASILITAS"
                                                    select hh) on e.PreparingItemId equals h.Identity
                                         join i in garmentPreparingRepository.Query on h.GarmentPreparingId equals i.Identity
                                         where a.RONo == d.RONo && a.RONo == g.RONo && a.RONo == i.RONo
                                         select b.Identity).Distinct().ToList();

            var finishingbarangjadi = from a in (from aa in garmentFinishingOutRepository.Query
                                                 where aa.FinishingTo == "GUDANG JADI"
                                                 && aa.Deleted == false
                                                 select new
                                                 {
                                                     aa.RONo,
                                                     aa.Identity,
                                                     aa.ComodityCode,
                                                     aa.ComodityName,
                                                     aa.FinishingOutDate,
                                                     aa.FinishingOutNo
                                                 })
                                      join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId
                                      where finishingbarangjadiid.Contains(b.Identity)
                                      && b.Deleted == false
                                      select new mutationView
                                      {
                                         SaldoQtyFin = a.FinishingOutDate.AddHours(7).Date < dateFrom.Date ? b.Quantity : 0,
                                         AdjFin = 0,
                                         //ComodityCode = b.ProductCode,
                                         //ComodityName = b.ProductName,
                                         ComodityCode = a.ComodityCode,
                                         ComodityName = a.ComodityName,
                                         QtyExpend = 0,
                                         QtyFin = a.FinishingOutDate.AddHours(7).Date >= dateFrom.Date && a.FinishingOutDate.AddHours(7).Date <= dateTo.Date ? b.Quantity : 0,
                                         Retur = 0,
                                      };

            var adjustinid = (from a in (from aa in garmentAdjustmentRepository.Query
                                         where aa.AdjustmentDate.AddHours(7).Date <= dateTo.Date
                                         && aa.AdjustmentType == "FINISHING"
                                         select aa)
                              join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
                              join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                              join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                              join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                              from f in (from ff in garmentFinishingOutRepository.Query
                                         where ff.FinishingTo == "GUDANG JADI"
                                         select new
                                         {
                                             ff.RONo,
                                             ff.Identity,
                                             ff.FinishingOutDate,
                                             ff.FinishingOutNo
                                         })
                              join g in garmentCuttingOutItemRepository.Query on b.ProductCode equals g.ProductCode
                              join h in (from hh in garmentCuttingOutRepository.Query
                                         where hh.CuttingOutType == "SEWING"
                                         select new
                                         {
                                             hh.RONo,
                                             hh.Identity,
                                             hh.UnitCode,
                                             hh.CutOutNo
                                         }) on g.CutOutId equals h.Identity
                              join i in garmentCuttingInDetailRepository.Query on b.ProductCode equals i.ProductCode
                              join j in garmentCuttingInItemRepository.Query on i.CutInItemId equals j.Identity
                              join k in (from kk in garmentCuttingInRepository.Query
                                         where kk.CuttingFrom == "PREPARING"
                                         select new
                                         {
                                             kk.RONo,
                                             kk.Identity,
                                             kk.UnitCode,
                                             kk.CutInNo
                                         }) on j.CutInId equals k.Identity
                              join l in (from ll in garmentPreparingItemRepository.Query
                                         where ll.CustomsCategory == "LOKAL FASILITAS" || ll.CustomsCategory == "IMPORT FASILITAS"
                                         select ll) on i.PreparingItemId equals l.Identity
                              join m in garmentPreparingRepository.Query on l.GarmentPreparingId equals m.Identity
                              where a.RONo == c.RONo && a.RONo == h.RONo && a.RONo == k.RONo && a.RONo == m.RONo
                              select b.Identity).Distinct().ToList();

            //var adjustoutid = (from a in (from aa in garmentAdjustmentRepository.Query
            //                              where aa.AdjustmentDate.AddHours(7).Date <= dateTo.Date
            //                              && aa.AdjustmentType == "BARANG JADI"
            //                              select aa)
            //                   join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            //                   join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
            //                   join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
            //                   join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
            //                   from f in (from ff in garmentFinishingOutRepository.Query
            //                              where ff.FinishingTo == "GUDANG JADI"
            //                              select new
            //                              {
            //                                  ff.RONo,
            //                                  ff.Identity,
            //                                  ff.FinishingOutDate,
            //                                  ff.FinishingOutNo
            //                              })
            //                   join g in garmentCuttingOutItemRepository.Query on b.ProductCode equals g.ProductCode
            //                   join h in (from hh in garmentCuttingOutRepository.Query
            //                              where hh.CuttingOutType == "SEWING"
            //                              select new
            //                              {
            //                                  hh.RONo,
            //                                  hh.Identity,
            //                                  hh.UnitCode,
            //                                  hh.CutOutNo
            //                              }) on g.CutOutId equals h.Identity
            //                   join i in garmentCuttingInDetailRepository.Query on b.ProductCode equals i.ProductCode
            //                   join j in garmentCuttingInItemRepository.Query on i.CutInItemId equals j.Identity
            //                   join k in (from kk in garmentCuttingInRepository.Query
            //                              where kk.CuttingFrom == "PREPARING"
            //                              select new
            //                              {
            //                                  kk.RONo,
            //                                  kk.Identity,
            //                                  kk.UnitCode,
            //                                  kk.CutInNo
            //                              }) on j.CutInId equals k.Identity
            //                   join l in (from ll in garmentPreparingItemRepository.Query
            //                              where ll.CustomsCategory == "LOKAL FASILITAS" || ll.CustomsCategory == "IMPORT FASILITAS"
            //                              select ll) on i.PreparingItemId equals l.Identity
            //                   join m in garmentPreparingRepository.Query on l.GarmentPreparingId equals m.Identity
            //                   where a.RONo == c.RONo && a.RONo == h.RONo && a.RONo == k.RONo && a.RONo == m.RONo
            //                   select b.Identity).Distinct().ToList();

            var adjustin = from a in (from aa in garmentAdjustmentRepository.Query
                                      where aa.AdjustmentDate.AddHours(7).Date <= dateTo.Date
                                      && aa.AdjustmentType == "FINISHING"
                                      select aa)
                           join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
                           where adjustinid.Contains(b.Identity)
                           select new mutationView
                           {
                               SaldoQtyFin = a.AdjustmentDate.AddHours(7).Date < dateFrom.Date ? b.Quantity : 0,
                               AdjFin = a.AdjustmentDate.AddHours(7).Date >= dateFrom.Date && a.AdjustmentDate.AddHours(7).Date <= dateTo.Date ? b.Quantity : 0,
                               ComodityCode = a.ComodityCode,
                               ComodityName = a.ComodityName,
                               QtyExpend = 0,
                               QtyFin = a.AdjustmentDate.AddHours(7).Date >= dateFrom.Date && a.AdjustmentDate.AddHours(7).Date <= dateTo.Date ? b.Quantity : 0,
                               Retur = 0,
                           };

            //var adjustout = from a in (from aa in garmentAdjustmentRepository.Query
            //                           where aa.AdjustmentDate.AddHours(7).Date <= dateTo.Date
            //                           && aa.AdjustmentType == "BARANG JADI"
            //                           select aa)
            //                join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            //                where adjustoutid.Contains(b.Identity)
            //                select new mutationView
            //                {
            //                    SaldoQtyFin = a.AdjustmentDate.AddHours(7).Date < dateFrom.Date ? b.Quantity : 0,
            //                    AdjFin = a.AdjustmentDate.AddHours(7).Date >= dateFrom.Date && a.AdjustmentDate.AddHours(7).Date <= dateTo.Date ? b.Quantity : 0,
            //                    ComodityCode = b.ProductCode,
            //                    ComodityName = b.ProductName,
            //                    QtyExpend = a.AdjustmentDate.AddHours(7).Date >= dateFrom.Date && a.AdjustmentDate.AddHours(7).Date <= dateTo.Date ? b.Quantity : 0,
            //                    QtyFin = 0,
            //                    Retur = 0,
            //                };

            var returexpendid = (from a in (from aa in garmentExpenditureGoodReturnRepository.Query
                                            where aa.ReturDate.AddHours(7).Date <= dateTo.Date
                                            select aa)
                                 join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
                                 join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                                 join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                                 join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                                 join f in (from ff in garmentFinishingOutRepository.Query
                                            where ff.FinishingTo == "GUDANG JADI"
                                            select new
                                            {
                                                ff.RONo,
                                                ff.Identity,
                                                ff.FinishingOutDate,
                                                ff.FinishingOutNo
                                            }) on e.FinishingOutId equals f.Identity
                                 join g in garmentCuttingOutItemRepository.Query on e.ProductCode equals g.ProductCode
                                 join h in (from hh in garmentCuttingOutRepository.Query
                                            where hh.CuttingOutType == "SEWING"
                                            select new
                                            {
                                                hh.RONo,
                                                hh.Identity,
                                                hh.UnitCode,
                                                hh.CutOutNo
                                            }) on g.CutOutId equals h.Identity
                                 join i in garmentCuttingInDetailRepository.Query on e.ProductCode equals i.ProductCode
                                 join j in garmentCuttingInItemRepository.Query on i.CutInItemId equals j.Identity
                                 join k in (from kk in garmentCuttingInRepository.Query
                                            where kk.CuttingFrom == "PREPARING"
                                            select new
                                            {
                                                kk.RONo,
                                                kk.Identity,
                                                kk.UnitCode,
                                                kk.CutInNo
                                            }) on j.CutInId equals k.Identity
                                 join l in (from ll in garmentPreparingItemRepository.Query
                                            where ll.CustomsCategory == "LOKAL FASILITAS" || ll.CustomsCategory == "IMPORT FASILITAS"
                                            select ll) on e.ProductCode equals l.ProductCode
                                 join m in garmentPreparingRepository.Query on l.GarmentPreparingId equals m.Identity
                                 where a.RONo == c.RONo && a.RONo == h.RONo && a.RONo == k.RONo && a.RONo == m.RONo
                                 select b.Identity).Distinct().ToList();

            var returexpend = from a in garmentExpenditureGoodReturnRepository.Query
                              join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
                              //join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                              //join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                              //join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                              where returexpendid.Contains(b.Identity)
                              select new mutationView
                              {
                                  SaldoQtyFin = a.ReturDate.AddHours(7).Date < dateFrom.Date ? b.Quantity : 0,
                                  AdjFin = 0,
                                  ComodityCode = a.ComodityCode,
                                  ComodityName = a.ComodityName,
                                  QtyExpend = 0,
                                  QtyFin = 0,
                                  Retur = a.ReturDate.AddHours(7).Date >= dateFrom.Date && a.ReturDate.AddHours(7).Date <= dateTo.Date ? b.Quantity : 0
                              };

            var factexpendid = (from a in (from aa in garmentExpenditureGoodRepository.Query
                                           where aa.ExpenditureDate.AddHours(7).Date <= dateTo.Date
                                           select aa)
                                join b in garmentExpenditureGoodItemRepository.Query on a.Identity equals b.ExpenditureGoodId
                                join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                                join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                                join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                                from f in (from ff in garmentFinishingOutRepository.Query
                                           where ff.FinishingTo == "GUDANG JADI"
                                           select new
                                           {
                                              ff.RONo,
                                              ff.Identity,
                                              ff.FinishingOutDate,
                                              ff.FinishingOutNo
                                           })
                                join g in garmentCuttingOutItemRepository.Query on e.ProductCode equals g.ProductCode
                                join h in (from hh in garmentCuttingOutRepository.Query
                                           where hh.CuttingOutType == "SEWING"
                                           select new
                                           {
                                              hh.RONo,
                                              hh.Identity,
                                              hh.UnitCode,
                                              hh.CutOutNo
                                           }) on g.CutOutId equals h.Identity
                                join i in garmentCuttingInDetailRepository.Query on e.ProductCode equals i.ProductCode
                                join j in garmentCuttingInItemRepository.Query on i.CutInItemId equals j.Identity
                                join k in (from kk in garmentCuttingInRepository.Query
                                           where kk.CuttingFrom == "PREPARING"
                                           select new
                                           {
                                              kk.RONo,
                                              kk.Identity,
                                              kk.UnitCode,
                                              kk.CutInNo
                                           }) on j.CutInId equals k.Identity
                                join l in (from ll in garmentPreparingItemRepository.Query
                                           where ll.CustomsCategory == "LOKAL FASILITAS" || ll.CustomsCategory == "IMPORT FASILITAS"
                                           select ll) on e.ProductCode equals l.ProductCode
                                join m in garmentPreparingRepository.Query on l.GarmentPreparingId equals m.Identity
                                where a.RONo == c.RONo && a.RONo == h.RONo && a.RONo == k.RONo && a.RONo == m.RONo
                                select b.Identity).Distinct().ToList();

            var factexpend = from a in garmentExpenditureGoodRepository.Query
                             join b in garmentExpenditureGoodItemRepository.Query on a.Identity equals b.ExpenditureGoodId
                             //join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                             //join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                             //join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                             where factexpendid.Contains(b.Identity)
                             select new mutationView
                             {
                                 SaldoQtyFin = a.ExpenditureDate.AddHours(7).Date < dateFrom.Date ? -b.Quantity : 0,
                                 AdjFin = 0,
                                 ComodityCode = a.ComodityCode,
                                 ComodityName = a.ComodityName,
                                 QtyExpend = a.ExpenditureDate.AddHours(7).Date >= dateFrom.Date && a.ExpenditureDate.AddHours(7).Date <= dateTo.Date ? b.Quantity : 0,
                                 QtyFin = 0,
                                 Retur = 0,
                             };

            //var queryNow = returexpend.Union(finishingbarangjadi).Union(factexpend).Union(adjustin).Union(adjustout).AsEnumerable();
            var queryNow = returexpend.Union(finishingbarangjadi).Union(factexpend).Union(adjustin).AsEnumerable();

            var mutationTemp = queryNow.GroupBy(x => new { x.ComodityCode, x.ComodityName }, (key, group) => new
            {
                kodeBarang = key.ComodityCode,
                namaBarang = key.ComodityName,
                pemasukan = group.Sum(x => x.Retur + x.QtyFin),
                pengeluaran = group.Sum(x => x.QtyExpend),
                penyesuaian = group.Sum(x => x.AdjFin),
                saldoAwal = group.Sum(x => x.SaldoQtyFin),
                saldoBuku = group.Sum(x => x.SaldoQtyFin) + group.Sum(x => x.Retur + x.QtyFin) - group.Sum(x => x.QtyExpend),
                selisih = 0,
                stockOpname = 0,
                unitQtyName = "PCS",
                gudang = "-"
            });

            //var productCodeList = string.Join(",", mutationTemp.Select(x => x.kodeBarang).ToList());

            //GarmentProductResult productList = await GetProducts(productCodeList, request.token);

            foreach (var i in mutationTemp/*.Where(x => x.saldoAwal != 0 || x.pemasukan != 0 || x.pengeluaran != 0 || x.penyesuaian != 0 || x.stockOpname != 0 || x.saldoBuku != 0)*/)
            {
                var comodity = (from a in garmentCuttingOutRepository.Query
                                where a.ComodityCode == i.kodeBarang
                                select a.ComodityName).FirstOrDefault();

                //var productName = (from a in productList.data
                //                   where a.Code == i.kodeBarang
                //                   select a.Name).FirstOrDefault();

                GarmentMutationExpenditureGoodDto dto = new GarmentMutationExpenditureGoodDto
                {
                    KodeBarang = i.kodeBarang,
                    NamaBarang = comodity,
                    Pemasukan = i.pemasukan,
                    Pengeluaran = i.pengeluaran,
                    Penyesuaian = i.penyesuaian,
                    SaldoAwal = i.saldoAwal,
                    SaldoBuku = i.saldoBuku,
                    Selisih = i.selisih,
                    StockOpname = i.stockOpname,
                    UnitQtyName = i.unitQtyName,
                    Gudang = i.gudang,
                };

                mutationExpenditureGoodDto.Add(dto);
            }

            expenditureGoodListViewModel.garmentMutations = mutationExpenditureGoodDto.OrderBy(x => x.KodeBarang).ToList();
            return expenditureGoodListViewModel;



        }

    }
}

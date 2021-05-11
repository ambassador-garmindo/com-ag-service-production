using ExtCore.Data.Abstractions;
using Infrastructure.Domain.Queries;
using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using Manufactures.Domain.GarmentAdjustments.Repositories;
using Manufactures.Domain.GarmentExpenditureGoods.Repositories;
using Manufactures.Domain.GarmentFinishingIns.Repositories;
using Manufactures.Domain.GarmentFinishingOuts.Repositories;
using Manufactures.Domain.GarmentSewingOuts.Repositories;
using Manufactures.Domain.MonitoringProductionStockFlow;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Manufactures.Domain.GarmentExpenditureGoodReturns.Repositories;
using Manufactures.Domain.GarmentCuttingOuts.Repositories;
using Manufactures.Domain.GarmentComodityPrices.Repositories;
using Manufactures.Domain.GarmentPreparings.Repositories;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentLoadings.Repositories;
using Manufactures.Domain.GarmentSewingIns.Repositories;
using Manufactures.Domain.GarmentSewingDOs.Repositories;
using Infrastructure.External.DanLirisClient.Microservice;
using Infrastructure.External.DanLirisClient.Microservice.MasterResult;
using Newtonsoft.Json;

namespace Manufactures.Application.GarmentExpenditureGoods.Queries.GetMutationExpenditureGoods
{
    public class GarmentMutationExpenditureGoodQueryHandler : IQueryHandler<GetMutationExpenditureGoodsQuery, GarmentMutationExpenditureGoodListViewModel>
    {
        private readonly IStorage _storage;
        private readonly IGarmentBalanceMonitoringProductionStockFlowRepository garmentBalanceMonitoringProductionStockFlowRepository;
        private readonly IGarmentAdjustmentRepository garmentAdjustmentRepository;
        private readonly IGarmentAdjustmentItemRepository garmentAdjustmentItemRepository;
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
        private readonly IGarmentSewingInRepository garmentSewingInRepository;
        private readonly IGarmentSewingInItemRepository garmentSewingInItemRepository;
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
            garmentCuttingOutRepository = storage.GetRepository<IGarmentCuttingOutRepository>();
            garmentCuttingOutItemRepository = storage.GetRepository<IGarmentCuttingOutItemRepository>();
            garmentCuttingOutDetailRepository = storage.GetRepository<IGarmentCuttingOutDetailRepository>();
            garmentCuttingInRepository = storage.GetRepository<IGarmentCuttingInRepository>();
            garmentCuttingInItemRepository = storage.GetRepository<IGarmentCuttingInItemRepository>();
            garmentCuttingInDetailRepository = storage.GetRepository<IGarmentCuttingInDetailRepository>();
            garmentSewingInRepository = storage.GetRepository<IGarmentSewingInRepository>();
            garmentSewingInItemRepository = storage.GetRepository<IGarmentSewingInItemRepository>();
            garmentLoadingRepository = storage.GetRepository<IGarmentLoadingRepository>();
            garmentLoadingItemRepository = storage.GetRepository<IGarmentLoadingItemRepository>();
            garmentAdjustmentRepository = storage.GetRepository<IGarmentAdjustmentRepository>();
            garmentAdjustmentItemRepository = storage.GetRepository<IGarmentAdjustmentItemRepository>();
            garmentSewingOutRepository = storage.GetRepository<IGarmentSewingOutRepository>();
            garmentSewingOutItemRepository = storage.GetRepository<IGarmentSewingOutItemRepository>();
            garmentFinishingOutRepository = storage.GetRepository<IGarmentFinishingOutRepository>();
            garmentFinishingOutItemRepository = storage.GetRepository<IGarmentFinishingOutItemRepository>();
            garmentFinishingInRepository = storage.GetRepository<IGarmentFinishingInRepository>();
            garmentFinishingInItemRepository = storage.GetRepository<IGarmentFinishingInItemRepository>();
            garmentExpenditureGoodRepository = storage.GetRepository<IGarmentExpenditureGoodRepository>();
            garmentExpenditureGoodItemRepository = storage.GetRepository<IGarmentExpenditureGoodItemRepository>();
            garmentExpenditureGoodReturnRepository = storage.GetRepository<IGarmentExpenditureGoodReturnRepository>();
            garmentExpenditureGoodReturnItemRepository = storage.GetRepository<IGarmentExpenditureGoodReturnItemRepository>();
            garmentSewingDORepository = storage.GetRepository<IGarmentSewingDORepository>();
            garmentSewingDOItemRepository = storage.GetRepository<IGarmentSewingDOItemRepository>();
            garmentPreparingRepository = storage.GetRepository<IGarmentPreparingRepository>();
            garmentPreparingItemRepository = storage.GetRepository<IGarmentPreparingItemRepository>();
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
        }

        class mutationTempView
        {
            public string RONo { get; internal set; }
            public string ProductCode { get; internal set; }
            public string CustomsCategory { get; internal set; }
            public DateTimeOffset Date { get; internal set; }
            public double Quantity { get; internal set; }
            public int UENId { get; internal set; }
            public int UENItemId { get; internal set; }
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
            //DateTimeOffset dateBalance = (from a in garmentBalanceMonitoringProductionStockFlowRepository.Query
            //                              select a.CreatedDate).FirstOrDefault();

            //var querybalance = from a in (from aa in garmentBalanceMonitoringProductionStockFlowRepository.Query
            //                              where aa.CreatedDate < dateFrom
            //                              select new { aa.BeginingBalanceExpenditureGood, aa.Ro, aa.Comodity })
            //                   join b in garmentCuttingOutRepository.Query on a.Ro equals b.RONo
            //                   //&& a.Comodity == "BOYS SHORTS"
            //                   select new mutationView
            //                   {
            //                       SaldoQtyFin = a.BeginingBalanceExpenditureGood,
            //                       AdjFin = 0,
            //                       //Comodity = a.Comodity,
            //                       ComodityCode = b.ComodityCode,
            //                       QtyExpend = 0,
            //                       QtyFin = 0,
            //                       Retur = 0,
            //                   };

            //var adjust = from a in (from aa in garmentAdjustmentRepository.Query
            //                               where aa.AdjustmentDate >= dateBalance && aa.AdjustmentDate <= dateTo
            //                               && aa.AdjustmentType == "FINISHING"
            //                               select aa)
            //                    join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            //                    select new mutationView
            //                    {
            //                        SaldoQtyFin = a.AdjustmentDate < dateFrom && a.AdjustmentDate > dateBalance ? b.Quantity : 0,
            //                        AdjFin = a.AdjustmentDate >= dateFrom ? b.Quantity : 0,
            //                        ComodityCode = a.ComodityCode,
            //                        QtyExpend = 0,
            //                        QtyFin = 0,
            //                        Retur = 0,
            //                    };

            var finishingbarangjadi = from a in (from aa in garmentFinishingOutRepository.Query
                                 where aa.FinishingOutDate <= dateTo
                                 && aa.FinishingTo == "GUDANG JADI"
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
                                 select new {
                                     dd.RONo,
                                     dd.Identity,
                                     dd.UnitCode,
                                     dd.CutOutNo
                                 }) on c.CutOutId equals d.Identity
                      join e in garmentCuttingInDetailRepository.Query on c.ProductCode equals e.ProductCode
                      join f in garmentCuttingInItemRepository.Query on e.CutInItemId equals f.Identity
                      join g in (from gg in garmentCuttingInRepository.Query
                                 where gg.CuttingFrom == "PREPARING"
                                 select new {
                                     gg.RONo,
                                     gg.Identity,
                                     gg.UnitCode,
                                     gg.CutInNo
                                 }) on f.CutInId equals g.Identity
                      join h in garmentPreparingItemRepository.Query on e.ProductCode equals h.ProductCode
                      join i in garmentPreparingRepository.Query on h.GarmentPreparingId equals i.Identity
                      where a.RONo == d.RONo && a.RONo == g.RONo && a.RONo == i.RONo
                      select new mutationTempView
                      {
                          RONo = a.RONo,
                          ProductCode = b.ProductCode,
                          Date = a.FinishingOutDate,
                          Quantity = b.Quantity,
                          UENId = i.UENId,
                          UENItemId = h.UENItemId
                      };

            //var finishingbarangjadi = from a in (from aa in garmentFinishingOutRepository.Query
            //                                     where aa.FinishingOutDate <= dateTo
            //                                     && aa.FinishingTo == "GUDANG JADI"
            //                                     select new
            //                                     {
            //                                         aa.RONo,
            //                                         aa.Identity,
            //                                         aa.FinishingOutDate,
            //                                         aa.FinishingOutNo
            //                                     })
            //                          join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId

            //                          join c in garmentFinishingInItemRepository.Query on b.FinishingInItemId equals c.Identity
            //                          join d in (from dd in garmentFinishingInRepository.Query
            //                                     where dd.FinishingInType == "SEWING"
            //                                     select new
            //                                     {
            //                                         dd.RONo,
            //                                         dd.Identity,
            //                                         dd.FinishingInType,
            //                                         dd.FinishingInNo
            //                                     }) on c.FinishingInId equals d.Identity

            //                          join e in garmentSewingOutItemRepository.Query on c.SewingOutItemId equals e.Identity
            //                          join f in (from ff in garmentSewingOutRepository.Query
            //                                     where ff.SewingTo == "FINISHING"
            //                                     select new
            //                                     {
            //                                         ff.RONo,
            //                                         ff.Identity,
            //                                         ff.SewingTo,
            //                                         ff.SewingOutNo
            //                                     }) on e.SewingOutId equals f.Identity

            //                          join g in garmentSewingInItemRepository.Query on e.SewingInItemId equals g.Identity
            //                          join h in (from hh in garmentSewingInRepository.Query
            //                                     where hh.SewingFrom == "CUTTING"
            //                                     select new
            //                                     {
            //                                         hh.RONo,
            //                                         hh.Identity,
            //                                         hh.SewingFrom,
            //                                         hh.SewingInNo
            //                                     }) on g.SewingInId equals h.Identity

            //                          join i in garmentLoadingItemRepository.Query on g.LoadingItemId equals i.Identity
            //                          join j in (from jj in garmentLoadingRepository.Query
            //                                     select new
            //                                     {
            //                                         jj.RONo,
            //                                         jj.Identity,
            //                                         jj.UnitCode,
            //                                         jj.LoadingNo
            //                                     }) on i.LoadingId equals j.Identity

            //                          join k in garmentSewingDOItemRepository.Query on i.SewingDOItemId equals k.Identity
            //                          join l in (from ll in garmentSewingDORepository.Query
            //                                     select new
            //                                     {
            //                                         ll.RONo,
            //                                         ll.Identity,
            //                                         ll.UnitCode,
            //                                         ll.SewingDONo
            //                                     }) on k.SewingDOId equals l.Identity

            //                          join m in garmentCuttingOutItemRepository.Query on k.CuttingOutItemId equals m.Identity
            //                          join n in (from nn in garmentCuttingOutRepository.Query
            //                                     where nn.CuttingOutType == "SEWING"
            //                                     select new
            //                                     {
            //                                         nn.RONo,
            //                                         nn.Identity,
            //                                         nn.UnitCode,
            //                                         nn.CutOutNo
            //                                     }) on m.CutOutId equals n.Identity

            //                          join o in garmentCuttingInDetailRepository.Query on m.CuttingInDetailId equals o.Identity
            //                          join p in garmentCuttingInItemRepository.Query on o.CutInItemId equals p.Identity
            //                          join q in (from pp in garmentCuttingInRepository.Query
            //                                     where pp.CuttingFrom == "PREPARING"
            //                                     select new
            //                                     {
            //                                         pp.RONo,
            //                                         pp.Identity,
            //                                         pp.UnitCode,
            //                                         pp.CutInNo
            //                                     }) on p.CutInId equals q.Identity

            //                          join r in garmentPreparingItemRepository.Query on o.PreparingItemId equals r.Identity
            //                          join s in garmentPreparingRepository.Query on r.GarmentPreparingId equals s.Identity

            //                          select new mutationTempView
            //                          {
            //                              RONo = a.RONo,
            //                              ProductCode = b.ProductCode,
            //                              Date = a.FinishingOutDate,
            //                              Quantity = b.Quantity,
            //                              UENId = s.UENId,
            //                              UENItemId = r.UENItemId
            //                          };

            var mutationTemp = finishingbarangjadi.GroupBy(x => new { x.RONo, x.ProductCode, x.UENId, x.UENItemId }, (key, group) => new mutationTempView
            {
                RONo = key.RONo,
                ProductCode = key.ProductCode,
                UENId = key.UENId,
                UENItemId = key.UENItemId

            }).AsEnumerable();

            //select new mutationView
            //{
            //    SaldoQtyFin = a.FinishingOutDate.Date < dateFrom.Date ? b.Quantity : 0,
            //    AdjFin = 0,
            //    QtyExpend = 0,
            //    QtyFin = a.FinishingOutDate >= dateFrom ? b.Quantity : 0,
            //    Retur = 0,
            //};

            var adjust = from a in (from aa in garmentAdjustmentRepository.Query
                                    where aa.AdjustmentDate <= dateTo
                                    && aa.AdjustmentType == "FINISHING"
                                    select aa)
                         join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
                         select new mutationView
                         {
                             SaldoQtyFin = a.AdjustmentDate < dateFrom ? b.Quantity : 0,
                             AdjFin = a.AdjustmentDate >= dateFrom ? b.Quantity : 0,
                             ComodityCode = a.ComodityCode,
                             QtyExpend = 0,
                             QtyFin = 0,
                             Retur = 0,
                         };

            //var returexpend = from a in (from aa in garmentExpenditureGoodReturnRepository.Query
            //                             where aa.ReturDate >= dateBalance && aa.ReturDate <= dateTo //&& aa.ComodityCode == "BR"
            //                             select aa)
            //                  join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
            //                  select new mutationView
            //                  {
            //                      SaldoQtyFin = a.ReturDate < dateFrom && a.ReturDate > dateBalance ? b.Quantity : 0,
            //                      AdjFin =  0,
            //                      ComodityCode = a.ComodityCode,
            //                      QtyExpend = 0,
            //                      QtyFin = 0,
            //                      Retur = a.ReturDate >= dateFrom ? b.Quantity : 0
            //                  };

            var returexpend = from a in (from aa in garmentExpenditureGoodReturnRepository.Query
                                         where aa.ReturDate <= dateTo
                                         select aa)
                              join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
                              select new mutationView
                              {
                                  SaldoQtyFin = a.ReturDate < dateFrom ? b.Quantity : 0,
                                  AdjFin = 0,
                                  ComodityCode = a.ComodityCode,
                                  QtyExpend = 0,
                                  QtyFin = 0,
                                  Retur = a.ReturDate >= dateFrom ? b.Quantity : 0
                              };

            //var finishingbarangjadi = from a in (from aa in garmentFinishingOutRepository.Query
            //                                     where aa.FinishingOutDate >= dateBalance && aa.FinishingOutDate <= dateTo
            //                                     && aa.FinishingTo == "GUDANG JADI" //&& aa.ComodityCode == "BR"
            //                                     select aa)
            //                          join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId
            //                          select new mutationView
            //                          {
            //                              SaldoQtyFin = a.FinishingOutDate.Date < dateFrom.Date && a.FinishingOutDate > dateBalance ? b.Quantity : 0,
            //                              AdjFin = 0,
            //                              ComodityCode = a.ComodityCode,
            //                              QtyExpend = 0,
            //                              QtyFin = a.FinishingOutDate>= dateFrom ? b.Quantity : 0,
            //                              Retur = 0,
            //                          };

            //var factexpend = from a in (from aa in garmentExpenditureGoodRepository.Query
            //                            where aa.ExpenditureDate >= dateBalance && aa.ExpenditureDate <= dateTo //&& aa.ComodityCode == "BR"
            //                            select aa)
            //                 join b in garmentExpenditureGoodItemRepository.Query on a.Identity equals b.ExpenditureGoodId
            //                 select new mutationView
            //                 {
            //                     SaldoQtyFin = a.ExpenditureDate < dateFrom && a.ExpenditureDate > dateBalance ? -b.Quantity : 0,
            //                     AdjFin = 0,
            //                     ComodityCode = a.ComodityCode,
            //                     QtyExpend = a.ExpenditureDate >= dateFrom ? b.Quantity : 0,
            //                     QtyFin = 0,
            //                     Retur = 0,
            //                 };

            var factexpend = from a in (from aa in garmentExpenditureGoodRepository.Query
                                        where aa.ExpenditureDate <= dateTo
                                        select aa)
                             join b in garmentExpenditureGoodItemRepository.Query on a.Identity equals b.ExpenditureGoodId
                             select new mutationView
                             {
                                 SaldoQtyFin = a.ExpenditureDate < dateFrom ? -b.Quantity : 0,
                                 AdjFin = 0,
                                 ComodityCode = a.ComodityCode,
                                 QtyExpend = a.ExpenditureDate >= dateFrom ? b.Quantity : 0,
                                 QtyFin = 0,
                                 Retur = 0,
                             };


            //var queryNow = adjust.Union(querybalance).Union(returexpend).Union(finishingbarangjadi).Union(factexpend).AsEnumerable();
            //var queryNow = adjust.Union(returexpend).Union(finishingbarangjadi).Union(factexpend).AsEnumerable();

            //var mutationTemp = queryNow.GroupBy(x => new { x.ComodityCode }, (key, group) => new
            //{
            //    kodeBarang = key.ComodityCode,
            //    //namaBarang = group.FirstOrDefault().Comodity,
            //    pemasukan = group.Sum(x => x.Retur + x.QtyFin),
            //    pengeluaran = group.Sum(x=>x.QtyExpend),
            //    penyesuaian = group.Sum(x=>x.AdjFin),
            //    saldoAwal = group.Sum(x=>x.SaldoQtyFin),
            //    saldoBuku = group.Sum(x => x.SaldoQtyFin) + group.Sum(x => x.Retur + x.QtyFin) - group.Sum(x => x.QtyExpend),
            //    selisih = 0,
            //    stockOpname = 0,
            //    unitQtyName = "PCS"


            //});

            //foreach (var i in mutationTemp.Where(x => x.saldoAwal != 0 || x.pemasukan != 0 || x.pengeluaran != 0 || x.penyesuaian != 0 || x.stockOpname != 0 || x.saldoBuku != 0))
            //{
            //    var comodity = (from a in garmentCuttingOutRepository.Query
            //                    where a.ComodityCode == i.kodeBarang
            //                    select a.ComodityName).FirstOrDefault();

            //    GarmentMutationExpenditureGoodDto dto = new GarmentMutationExpenditureGoodDto
            //    {
            //        KodeBarang = i.kodeBarang,
            //        NamaBarang = comodity,
            //        Pemasukan = i.pemasukan,
            //        Pengeluaran = i.pengeluaran,
            //        Penyesuaian = i.penyesuaian,
            //        SaldoAwal = i.saldoAwal,
            //        SaldoBuku = i.saldoBuku,
            //        Selisih = i.selisih,
            //        StockOpname = i.stockOpname,
            //        UnitQtyName = i.unitQtyName
            //    };

            //    mutationExpenditureGoodDto.Add(dto);
            //}

            expenditureGoodListViewModel.garmentMutations = mutationExpenditureGoodDto.OrderBy(x=>x.KodeBarang).ToList();
            return expenditureGoodListViewModel;



        }

    }
}

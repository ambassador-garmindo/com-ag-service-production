using ExtCore.Data.Abstractions;
using Infrastructure.Domain.Queries;
using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using Manufactures.Domain.GarmentExpenditureGoodReturns.Repositories;
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
using Manufactures.Domain.GarmentAdjustments.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Infrastructure.External.DanLirisClient.Microservice;
using Infrastructure.External.DanLirisClient.Microservice.MasterResult;
using Newtonsoft.Json;
using System.Net.Http;


namespace Manufactures.Application.GarmentExpenditureGoods.Queries.GetReceiptFinishedGoods
{
    public class GarmentReceiptFinishedGoodQueryHandler : IQueryHandler<GetReceiptFinishedGoodsQuery, GarmentReceiptFinishedGoodListViewModel>
    {
        private readonly IStorage _storage;
        private readonly IGarmentAdjustmentRepository garmentAdjustmentRepository;
        private readonly IGarmentAdjustmentItemRepository garmentAdjustmentItemRepository;
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
        private readonly IGarmentFinishedGoodStockRepository garmentFinishedGoodStockRepository;
        private readonly IGarmentFinishedGoodStockHistoryRepository garmentFinishedGoodStockHistoryRepository;
        private readonly IGarmentExpenditureGoodReturnRepository garmentExpenditureGoodReturnRepository;
        private readonly IGarmentExpenditureGoodReturnItemRepository garmentExpenditureGoodReturnItemRepository;
        private readonly IGarmentCuttingOutRepository garmentCuttingOutRepository;
        private readonly IGarmentCuttingOutItemRepository garmentCuttingOutItemRepository;
        private readonly IGarmentCuttingOutDetailRepository garmentCuttingOutDetailRepository;
        private readonly IGarmentCuttingInRepository garmentCuttingInRepository;
        private readonly IGarmentCuttingInItemRepository garmentCuttingInItemRepository;
        private readonly IGarmentCuttingInDetailRepository garmentCuttingInDetailRepository;
        private readonly IGarmentPreparingRepository garmentPreparingRepository;
        private readonly IGarmentPreparingItemRepository garmentPreparingItemRepository;
        protected readonly IHttpClientService _http;

        public GarmentReceiptFinishedGoodQueryHandler(IStorage storage, IServiceProvider serviceProvider)
        {
            _storage = storage;
            
            garmentPreparingRepository = storage.GetRepository<IGarmentPreparingRepository>();
            garmentPreparingItemRepository = storage.GetRepository<IGarmentPreparingItemRepository>();
            garmentCuttingOutRepository = storage.GetRepository<IGarmentCuttingOutRepository>();
            garmentCuttingOutItemRepository = storage.GetRepository<IGarmentCuttingOutItemRepository>();
            garmentCuttingOutDetailRepository = storage.GetRepository<IGarmentCuttingOutDetailRepository>();
            garmentCuttingInRepository = storage.GetRepository<IGarmentCuttingInRepository>();
            garmentCuttingInItemRepository = storage.GetRepository<IGarmentCuttingInItemRepository>();
            garmentCuttingInDetailRepository = storage.GetRepository<IGarmentCuttingInDetailRepository>();
            garmentSewingDORepository = storage.GetRepository<IGarmentSewingDORepository>();
            garmentSewingDOItemRepository = storage.GetRepository<IGarmentSewingDOItemRepository>();
            garmentLoadingRepository = storage.GetRepository<IGarmentLoadingRepository>();
            garmentLoadingItemRepository = storage.GetRepository<IGarmentLoadingItemRepository>();
            garmentSewingInRepository = storage.GetRepository<IGarmentSewingInRepository>();
            garmentSewingInItemRepository = storage.GetRepository<IGarmentSewingInItemRepository>();
            garmentSewingOutRepository = storage.GetRepository<IGarmentSewingOutRepository>();
            garmentSewingOutItemRepository = storage.GetRepository<IGarmentSewingOutItemRepository>();
            garmentSewingOutDetailRepository = storage.GetRepository<IGarmentSewingOutDetailRepository>();
            garmentFinishingOutRepository = storage.GetRepository<IGarmentFinishingOutRepository>();
            garmentFinishingOutItemRepository = storage.GetRepository<IGarmentFinishingOutItemRepository>();
            garmentFinishingInRepository = storage.GetRepository<IGarmentFinishingInRepository>();
            garmentFinishingInItemRepository = storage.GetRepository<IGarmentFinishingInItemRepository>();
            garmentFinishedGoodStockRepository = storage.GetRepository<IGarmentFinishedGoodStockRepository>();
            garmentFinishedGoodStockHistoryRepository = storage.GetRepository<IGarmentFinishedGoodStockHistoryRepository>();
            garmentExpenditureGoodReturnRepository = storage.GetRepository<IGarmentExpenditureGoodReturnRepository>();
            garmentExpenditureGoodReturnItemRepository = storage.GetRepository<IGarmentExpenditureGoodReturnItemRepository>();
            garmentAdjustmentRepository = storage.GetRepository<IGarmentAdjustmentRepository>();
            garmentAdjustmentItemRepository = storage.GetRepository<IGarmentAdjustmentItemRepository>();
            _http = serviceProvider.GetService<IHttpClientService>();
        }

        class receiptView
        {
            public string Identity { get; internal set; }
            public string BonNo { get; internal set; }
            public DateTimeOffset BonDate { get; internal set; }
            public string ComodityCode { get; internal set; }
            public string ComodityName { get; internal set; }
            public double QtyProcess { get; internal set; }
            public double QtySubcon { get; internal set; }
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

        public async Task<GarmentReceiptFinishedGoodListViewModel> Handle(GetReceiptFinishedGoodsQuery request, CancellationToken cancellationToken)
        {
            GarmentReceiptFinishedGoodListViewModel receiptGoodListViewModel = new GarmentReceiptFinishedGoodListViewModel();
            List<GarmentReceiptFinishedGoodDto> receiptGoodDto = new List<GarmentReceiptFinishedGoodDto>();

            DateTimeOffset dateFrom = new DateTimeOffset(request.dateFrom, new TimeSpan(7, 0, 0));
            DateTimeOffset dateTo = new DateTimeOffset(request.dateTo, new TimeSpan(7, 0, 0));

            #region sc lama

            //#region finishing
            //var finishingbarangjadiid = (from a in (from aa in garmentFinishingOutRepository.Query
            //                                        where aa.FinishingOutDate.AddHours(7).Date <= dateTo.Date
            //                                        && aa.FinishingOutDate.AddHours(7).Date >= dateFrom.Date
            //                                        && aa.FinishingTo == "GUDANG JADI"
            //                                        && aa.Deleted == false
            //                                        select new
            //                                        {
            //                                            aa.RONo,
            //                                            aa.Identity,
            //                                            aa.UnitCode,
            //                                            aa.FinishingOutNo
            //                                        })
            //                             join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId
            //                             join c in garmentFinishingInItemRepository.Query on b.FinishingInItemId equals c.Identity
            //                             join d in (from dd in garmentFinishingInRepository.Query
            //                                        where dd.Deleted == false
            //                                        select new
            //                                        {
            //                                            dd.RONo,
            //                                            dd.Identity,
            //                                            dd.UnitCode,
            //                                            dd.FinishingInNo
            //                                        }) on c.FinishingInId equals d.Identity
            //                             join e in garmentSewingOutItemRepository.Query on c.SewingOutItemId equals e.Identity
            //                             join f in (from ff in garmentSewingOutRepository.Query
            //                                        where ff.SewingTo == "FINISHING"
            //                                        && ff.Deleted == false
            //                                        select new
            //                                        {
            //                                            ff.RONo,
            //                                            ff.Identity,
            //                                            ff.UnitCode,
            //                                            ff.SewingOutNo
            //                                        }) on e.SewingOutId equals f.Identity
            //                             join g in garmentSewingInItemRepository.Query on e.SewingInItemId equals g.Identity
            //                             join h in (from hh in garmentSewingInRepository.Query
            //                                        where hh.SewingFrom == "CUTTING"
            //                                        && hh.Deleted == false
            //                                        select new
            //                                        {
            //                                            hh.RONo,
            //                                            hh.Identity,
            //                                            hh.UnitCode,
            //                                            hh.SewingInNo
            //                                        }) on g.SewingInId equals h.Identity
            //                             join i in garmentLoadingItemRepository.Query on g.LoadingItemId equals i.Identity
            //                             join j in (from jj in garmentLoadingRepository.Query
            //                                        where jj.Deleted == false
            //                                        select new
            //                                        {
            //                                            jj.RONo,
            //                                            jj.Identity,
            //                                            jj.UnitCode,
            //                                            jj.LoadingNo
            //                                        }) on i.LoadingId equals j.Identity
            //                             join k in garmentSewingDOItemRepository.Query on i.SewingDOItemId equals k.Identity
            //                             join l in (from ll in garmentSewingDORepository.Query
            //                                        where ll.Deleted == false
            //                                        select new
            //                                        {
            //                                            ll.RONo,
            //                                            ll.Identity,
            //                                            ll.UnitCode,
            //                                            ll.SewingDONo
            //                                        }) on k.SewingDOId equals l.Identity
            //                             join m in garmentCuttingOutItemRepository.Query on k.CuttingOutItemId equals m.Identity
            //                             join n in (from nn in garmentCuttingOutRepository.Query
            //                                        where nn.CuttingOutType == "SEWING"
            //                                        && nn.Deleted == false
            //                                        select new
            //                                        {
            //                                            nn.RONo,
            //                                            nn.Identity,
            //                                            nn.UnitCode,
            //                                            nn.CutOutNo
            //                                        }) on m.CutOutId equals n.Identity
            //                             join o in garmentCuttingInDetailRepository.Query on m.CuttingInDetailId equals o.Identity
            //                             join p in garmentCuttingInItemRepository.Query on o.CutInItemId equals p.Identity
            //                             join q in (from qq in garmentCuttingInRepository.Query
            //                                        where qq.CuttingFrom == "PREPARING"
            //                                        && qq.Deleted == false
            //                                        select new
            //                                        {
            //                                            qq.RONo,
            //                                            qq.Identity,
            //                                            qq.UnitCode,
            //                                            qq.CutInNo
            //                                        }) on p.CutInId equals q.Identity
            //                             join r in (from rr in garmentPreparingItemRepository.Query
            //                                        where rr.CustomsCategory == "LOKAL FASILITAS" || rr.CustomsCategory == "IMPORT FASILITAS"
            //                                        select rr) on o.PreparingItemId equals r.Identity
            //                             join s in garmentPreparingRepository.Query on r.GarmentPreparingId equals s.Identity
            //                             where a.RONo == d.RONo && a.RONo == f.RONo && a.RONo == h.RONo && a.RONo == j.RONo && a.RONo == l.RONo && a.RONo == n.RONo && a.RONo == q.RONo
            //                             select b.Identity).Distinct().ToList();

            ////var finishingbarangjadiid = (from a in (from aa in garmentFinishingOutRepository.Query
            ////                                        where aa.FinishingOutDate.AddHours(7).Date <= dateTo.Date
            ////                                        && aa.FinishingOutDate.AddHours(7).Date >= dateFrom.Date
            ////                                        && aa.FinishingTo == "GUDANG JADI"
            ////                                        && aa.Deleted == false
            ////                                        select new
            ////                                        {
            ////                                            aa.RONo,
            ////                                            aa.Identity,
            ////                                            aa.FinishingOutDate,
            ////                                            aa.FinishingOutNo
            ////                                        })
            ////                             join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId
            ////                             join c in garmentCuttingOutItemRepository.Query on b.ProductCode equals c.ProductCode
            ////                             join d in (from dd in garmentCuttingOutRepository.Query
            ////                                        where dd.CuttingOutType == "SEWING"
            ////                                        select new
            ////                                        {
            ////                                            dd.RONo,
            ////                                            dd.Identity,
            ////                                            dd.UnitCode,
            ////                                            dd.CutOutNo
            ////                                        }) on c.CutOutId equals d.Identity
            ////                             join e in garmentCuttingInDetailRepository.Query on b.ProductCode equals e.ProductCode
            ////                             join f in garmentCuttingInItemRepository.Query on e.CutInItemId equals f.Identity
            ////                             join g in (from gg in garmentCuttingInRepository.Query
            ////                                        where gg.CuttingFrom == "PREPARING"
            ////                                        select new
            ////                                        {
            ////                                            gg.RONo,
            ////                                            gg.Identity,
            ////                                            gg.UnitCode,
            ////                                            gg.CutInNo
            ////                                        }) on f.CutInId equals g.Identity
            ////                             join h in (from hh in garmentPreparingItemRepository.Query
            ////                                        where hh.CustomsCategory == "LOKAL FASILITAS" || hh.CustomsCategory == "IMPORT FASILITAS"
            ////                                        select hh) on e.PreparingItemId equals h.Identity
            ////                             join i in garmentPreparingRepository.Query on h.GarmentPreparingId equals i.Identity
            ////                             where a.RONo == d.RONo && a.RONo == g.RONo && a.RONo == i.RONo
            ////                             select b.Identity).Distinct().ToList();

            //var finishingbarangjadi = from a in (from aa in garmentFinishingOutRepository.Query
            //                                     where aa.FinishingTo == "GUDANG JADI"
            //                                     && aa.Deleted == false
            //                                     select new
            //                                     {
            //                                         aa.RONo,
            //                                         aa.Identity,
            //                                         aa.FinishingOutDate,
            //                                         aa.FinishingOutNo,
            //                                         aa.ComodityCode,
            //                                         aa.ComodityName
            //                                     })
            //                          join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId
            //                          join c in garmentFinishingInItemRepository.Query on b.FinishingInItemId equals c.Identity
            //                          join d in garmentFinishingInRepository.Query on c.FinishingInId equals d.Identity
            //                          where finishingbarangjadiid.Contains(b.Identity)
            //                          && b.Deleted == false
            //                          && c.Deleted == false
            //                          && (d.FinishingInType == "SEWING" || d.FinishingInType == "PEMBELIAN")
            //                          select new receiptView
            //                          {
            //                              BonNo = a.FinishingOutNo,
            //                              BonDate =  a.FinishingOutDate,
            //                              ComodityCode = a.ComodityCode,
            //                              //ComodityName = a.ComodityName,
            //                              QtyProcess = d.FinishingInType == "SEWING" ? b.Quantity : 0,
            //                              QtySubcon = d.FinishingInType == "PEMBELIAN" ? b.Quantity : 0

            //                          };
            //#endregion

            //#region adjustment
            ////var adjustinid = (from a in (from aa in garmentAdjustmentRepository.Query
            ////                             where aa.AdjustmentDate.AddHours(7).Date <= dateTo.Date
            ////                             && aa.AdjustmentType == "FINISHING"
            ////                             select aa)
            ////                  join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            ////                  join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
            ////                  join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
            ////                  join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
            ////                  from f in (from ff in garmentFinishingOutRepository.Query
            ////                             where ff.FinishingTo == "GUDANG JADI"
            ////                             select new
            ////                             {
            ////                                 ff.RONo,
            ////                                 ff.Identity,
            ////                                 ff.FinishingOutDate,
            ////                                 ff.FinishingOutNo
            ////                             })
            ////                  join g in garmentCuttingOutItemRepository.Query on b.ProductCode equals g.ProductCode
            ////                  join h in (from hh in garmentCuttingOutRepository.Query
            ////                             where hh.CuttingOutType == "SEWING"
            ////                             select new
            ////                             {
            ////                                 hh.RONo,
            ////                                 hh.Identity,
            ////                                 hh.UnitCode,
            ////                                 hh.CutOutNo
            ////                             }) on g.CutOutId equals h.Identity
            ////                  join i in garmentCuttingInDetailRepository.Query on b.ProductCode equals i.ProductCode
            ////                  join j in garmentCuttingInItemRepository.Query on i.CutInItemId equals j.Identity
            ////                  join k in (from kk in garmentCuttingInRepository.Query
            ////                             where kk.CuttingFrom == "PREPARING"
            ////                             select new
            ////                             {
            ////                                 kk.RONo,
            ////                                 kk.Identity,
            ////                                 kk.UnitCode,
            ////                                 kk.CutInNo
            ////                             }) on j.CutInId equals k.Identity
            ////                  join l in (from ll in garmentPreparingItemRepository.Query
            ////                             where ll.CustomsCategory == "LOKAL FASILITAS" || ll.CustomsCategory == "IMPORT FASILITAS"
            ////                             select ll) on i.PreparingItemId equals l.Identity
            ////                  join m in garmentPreparingRepository.Query on l.GarmentPreparingId equals m.Identity
            ////                  where a.RONo == c.RONo && a.RONo == h.RONo && a.RONo == k.RONo && a.RONo == m.RONo
            ////                  select b.Identity).Distinct().ToList();

            ////var adjustin = (from a in (from aa in garmentAdjustmentRepository.Query
            ////                           where aa.AdjustmentType == "FINISHING"
            ////                           select aa)
            ////                join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            ////                join c in garmentFinishingInItemRepository.Query on b.FinishingInItemId equals c.Identity
            ////                join d in garmentFinishingInRepository.Query on c.Identity equals d.Identity
            ////                join e in garmentFinishingOutItemRepository.Query on c.Identity equals e.FinishingInItemId
            ////                join f in garmentFinishingOutRepository.Query on e.FinishingOutId equals f.Identity
            ////                where adjustinid.Contains(b.Identity)
            ////                && a.Deleted == false
            ////                && b.Deleted == false
            ////                && (d.FinishingInType == "SEWING" || d.FinishingInType == "PEMBELIAN")
            ////                select new receiptView
            ////                {
            ////                    BonNo = f.FinishingOutNo,
            ////                    BonDate = f.FinishingOutDate,
            ////                    ComodityCode = a.ComodityCode,
            ////                    //ComodityName = a.ComodityName,
            ////                    QtyProcess = d.FinishingInType == "SEWING" ? b.Quantity : 0,
            ////                    QtySubcon = d.FinishingInType == "PEMBELIAN" ? b.Quantity : 0,
            ////                });

            ////var adjustoutid = (from a in (from aa in garmentAdjustmentRepository.Query
            ////                              where aa.AdjustmentDate.AddHours(7).Date <= dateTo.Date
            ////                              && aa.AdjustmentType == "BARANG JADI"
            ////                              select aa)
            ////                   join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            ////                   join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
            ////                   join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
            ////                   join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
            ////                   from f in (from ff in garmentFinishingOutRepository.Query
            ////                              where ff.FinishingTo == "GUDANG JADI"
            ////                              select new
            ////                              {
            ////                                  ff.RONo,
            ////                                  ff.Identity,
            ////                                  ff.FinishingOutDate,
            ////                                  ff.FinishingOutNo
            ////                              })
            ////                   join g in garmentCuttingOutItemRepository.Query on b.ProductCode equals g.ProductCode
            ////                   join h in (from hh in garmentCuttingOutRepository.Query
            ////                              where hh.CuttingOutType == "SEWING"
            ////                              select new
            ////                              {
            ////                                  hh.RONo,
            ////                                  hh.Identity,
            ////                                  hh.UnitCode,
            ////                                  hh.CutOutNo
            ////                              }) on g.CutOutId equals h.Identity
            ////                   join i in garmentCuttingInDetailRepository.Query on b.ProductCode equals i.ProductCode
            ////                   join j in garmentCuttingInItemRepository.Query on i.CutInItemId equals j.Identity
            ////                   join k in (from kk in garmentCuttingInRepository.Query
            ////                              where kk.CuttingFrom == "PREPARING"
            ////                              select new
            ////                              {
            ////                                  kk.RONo,
            ////                                  kk.Identity,
            ////                                  kk.UnitCode,
            ////                                  kk.CutInNo
            ////                              }) on j.CutInId equals k.Identity
            ////                   join l in (from ll in garmentPreparingItemRepository.Query
            ////                              where ll.CustomsCategory == "LOKAL FASILITAS" || ll.CustomsCategory == "IMPORT FASILITAS"
            ////                              select ll) on i.PreparingItemId equals l.Identity
            ////                   join m in garmentPreparingRepository.Query on l.GarmentPreparingId equals m.Identity
            ////                   where a.RONo == c.RONo && a.RONo == h.RONo && a.RONo == k.RONo && a.RONo == m.RONo
            ////                   select b.Identity).Distinct().ToList();

            ////var adjustouttemp = (from a in garmentAdjustmentRepository.Query
            ////                     join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            ////                     where adjustoutid.Contains(b.Identity)
            ////                     select new
            ////                     {
            ////                         adjustmentItemId = b.Identity,
            ////                         finishedGoodStockId = b.FinishedGoodStockId
            ////                     }).Distinct();

            ////var finishedgoodstockId = adjustouttemp.Select(x => x.finishedGoodStockId).ToList();

            ////var finishedgoodstock = (from a in garmentFinishedGoodStockRepository.Query
            ////                         join b in garmentFinishedGoodStockHistoryRepository.Query on a.Identity equals b.FinishedGoodStockId
            ////                         where b.StockType == "IN"
            ////                         && finishedgoodstockId.Contains(a.Identity)
            ////                         select new
            ////                         {
            ////                             FinishedGoodStockId = a.Identity,
            ////                             FinishingOutItemId = b.FinishingOutItemId
            ////                         }).Distinct();

            ////finishedgoodstock.GroupBy(x => new { x.FinishedGoodStockId, x.FinishingOutItemId }, (key, group) => new
            ////{
            ////    FinishedGoodStockId = key.FinishedGoodStockId,
            ////    FinishedOutItemId = key.FinishingOutItemId
            ////});

            ////var adjustout = (from a in garmentAdjustmentRepository.Query
            ////                 join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            ////                 join c in adjustouttemp on b.Identity equals c.adjustmentItemId
            ////                 join d in finishedgoodstock on c.finishedGoodStockId equals d.FinishedGoodStockId
            ////                 join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
            ////                 join f in garmentFinishingOutRepository.Query on e.FinishingOutId equals f.Identity
            ////                 join g in garmentFinishingInItemRepository.Query on e.FinishingInItemId equals g.Identity
            ////                 join h in garmentFinishingInRepository.Query on g.FinishingInId equals h.Identity
            ////                 where adjustoutid.Contains(b.Identity)
            ////                 && a.Deleted == false
            ////                 && b.Deleted == false
            ////                 && (h.FinishingInType == "SEWING" || h.FinishingInType == "PEMBELIAN")  
            ////                 select new receiptView
            ////                 {
            ////                     BonNo = f.FinishingOutNo,
            ////                     BonDate = f.FinishingOutDate,
            ////                     ComodityCode = a.ComodityCode,
            ////                     //ComodityName = a.ComodityName,
            ////                     QtyProcess = h.FinishingInType == "SEWING" ? b.Quantity : 0,
            ////                     QtySubcon = h.FinishingInType == "PEMBELIAN" ? b.Quantity : 0,
            ////                 });
            //#endregion

            //#region return
            //var returexpendid = (from z in (from zz in garmentExpenditureGoodReturnRepository.Query
            //                                where zz.ReturDate.AddHours(7).Date <= dateTo.Date
            //                                && zz.ReturDate.AddHours(7).Date >= dateFrom.Date
            //                                select zz)
            //                     join y in garmentExpenditureGoodReturnItemRepository.Query on z.Identity equals y.ReturId
            //                     join x in garmentFinishedGoodStockRepository.Query on y.FinishedGoodStockId equals x.Identity
            //                     join w in garmentFinishedGoodStockHistoryRepository.Query on x.Identity equals w.FinishedGoodStockId
            //                     // -------------------------------------------------------------------------------------------------
            //                     join b in garmentFinishingOutItemRepository.Query on w.FinishingOutItemId equals b.Identity
            //                     join a in (from aa in garmentFinishingOutRepository.Query
            //                                where aa.FinishingTo == "GUDANG JADI"
            //                                && aa.Deleted == false
            //                                select new
            //                                {
            //                                    aa.RONo,
            //                                    aa.Identity,
            //                                    aa.UnitCode,
            //                                    aa.FinishingOutNo
            //                                }) on b.FinishingOutId equals a.Identity
            //                     join c in garmentFinishingInItemRepository.Query on b.FinishingInItemId equals c.Identity
            //                     join d in (from dd in garmentFinishingInRepository.Query
            //                                where dd.Deleted == false
            //                                select new
            //                                {
            //                                    dd.RONo,
            //                                    dd.Identity,
            //                                    dd.UnitCode,
            //                                    dd.FinishingInNo
            //                                }) on c.FinishingInId equals d.Identity
            //                     join e in garmentSewingOutItemRepository.Query on c.SewingOutItemId equals e.Identity
            //                     join f in (from ff in garmentSewingOutRepository.Query
            //                                where ff.SewingTo == "FINISHING"
            //                                && ff.Deleted == false
            //                                select new
            //                                {
            //                                    ff.RONo,
            //                                    ff.Identity,
            //                                    ff.UnitCode,
            //                                    ff.SewingOutNo
            //                                }) on e.SewingOutId equals f.Identity
            //                     join g in garmentSewingInItemRepository.Query on e.SewingInItemId equals g.Identity
            //                     join h in (from hh in garmentSewingInRepository.Query
            //                                where hh.SewingFrom == "CUTTING"
            //                                && hh.Deleted == false
            //                                select new
            //                                {
            //                                    hh.RONo,
            //                                    hh.Identity,
            //                                    hh.UnitCode,
            //                                    hh.SewingInNo
            //                                }) on g.SewingInId equals h.Identity
            //                     join i in garmentLoadingItemRepository.Query on g.LoadingItemId equals i.Identity
            //                     join j in (from jj in garmentLoadingRepository.Query
            //                                where jj.Deleted == false
            //                                select new
            //                                {
            //                                    jj.RONo,
            //                                    jj.Identity,
            //                                    jj.UnitCode,
            //                                    jj.LoadingNo
            //                                }) on i.LoadingId equals j.Identity
            //                      join k in garmentSewingDOItemRepository.Query on i.SewingDOItemId equals k.Identity
            //                      join l in (from ll in garmentSewingDORepository.Query
            //                                 where ll.Deleted == false
            //                                 select new
            //                                 {
            //                                     ll.RONo,
            //                                     ll.Identity,
            //                                     ll.UnitCode,
            //                                     ll.SewingDONo
            //                                 }) on k.SewingDOId equals l.Identity
            //                     join m in garmentCuttingOutItemRepository.Query on k.CuttingOutItemId equals m.Identity
            //                     join n in (from nn in garmentCuttingOutRepository.Query
            //                                where nn.CuttingOutType == "SEWING"
            //                                && nn.Deleted == false
            //                                select new
            //                                {
            //                                    nn.RONo,
            //                                    nn.Identity,
            //                                    nn.UnitCode,
            //                                    nn.CutOutNo
            //                                }) on m.CutOutId equals n.Identity
            //                     join o in garmentCuttingInDetailRepository.Query on m.CuttingInDetailId equals o.Identity
            //                     join p in garmentCuttingInItemRepository.Query on o.CutInItemId equals p.Identity
            //                     join q in (from qq in garmentCuttingInRepository.Query
            //                                where qq.CuttingFrom == "PREPARING"
            //                                && qq.Deleted == false
            //                                select new
            //                                {
            //                                    qq.RONo,
            //                                    qq.Identity,
            //                                    qq.UnitCode,
            //                                    qq.CutInNo
            //                                }) on p.CutInId equals q.Identity
            //                     join r in (from rr in garmentPreparingItemRepository.Query
            //                                where rr.CustomsCategory == "LOKAL FASILITAS" || rr.CustomsCategory == "IMPORT FASILITAS"
            //                                select rr) on o.PreparingItemId equals r.Identity
            //                     join s in garmentPreparingRepository.Query on r.GarmentPreparingId equals s.Identity
            //                     where z.RONo == a.RONo && z.RONo == d.RONo && z.RONo == f.RONo && z.RONo == h.RONo && z.RONo == j.RONo && z.RONo == l.RONo && z.RONo == n.RONo && z.RONo == q.RONo
            //                     select y.Identity).Distinct().ToList();

            ////var returexpendid = (from a in (from aa in garmentExpenditureGoodReturnRepository.Query
            ////                                where aa.ReturDate.AddHours(7).Date <= dateTo.Date
            ////                                && aa.ReturDate.AddHours(7).Date >= dateFrom.Date
            ////                                select aa)
            ////                     join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
            ////                     join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
            ////                     join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
            ////                     join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
            ////                     join f in (from ff in garmentFinishingOutRepository.Query
            ////                                where ff.FinishingTo == "GUDANG JADI"
            ////                                select new
            ////                                {
            ////                                    ff.RONo,
            ////                                    ff.Identity,
            ////                                    ff.FinishingOutDate,
            ////                                    ff.FinishingOutNo
            ////                                }) on e.FinishingOutId equals f.Identity
            ////                     join g in garmentCuttingOutItemRepository.Query on e.ProductCode equals g.ProductCode
            ////                     join h in (from hh in garmentCuttingOutRepository.Query
            ////                                where hh.CuttingOutType == "SEWING"
            ////                                select new
            ////                                {
            ////                                    hh.RONo,
            ////                                    hh.Identity,
            ////                                    hh.UnitCode,
            ////                                    hh.CutOutNo
            ////                                }) on g.CutOutId equals h.Identity
            ////                     join i in garmentCuttingInDetailRepository.Query on e.ProductCode equals i.ProductCode
            ////                     join j in garmentCuttingInItemRepository.Query on i.CutInItemId equals j.Identity
            ////                     join k in (from kk in garmentCuttingInRepository.Query
            ////                                where kk.CuttingFrom == "PREPARING"
            ////                                select new
            ////                                {
            ////                                    kk.RONo,
            ////                                    kk.Identity,
            ////                                    kk.UnitCode,
            ////                                    kk.CutInNo
            ////                                }) on j.CutInId equals k.Identity
            ////                     join l in (from ll in garmentPreparingItemRepository.Query
            ////                                where ll.CustomsCategory == "LOKAL FASILITAS" || ll.CustomsCategory == "IMPORT FASILITAS"
            ////                                select ll) on e.ProductCode equals l.ProductCode
            ////                     join m in garmentPreparingRepository.Query on l.GarmentPreparingId equals m.Identity
            ////                     where a.RONo == c.RONo && a.RONo == h.RONo && a.RONo == k.RONo && a.RONo == m.RONo
            ////                     select b.Identity).Distinct().ToList();

            //var returexpend = (from a in garmentExpenditureGoodReturnRepository.Query
            //                   join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
            //                   where returexpendid.Contains(b.Identity)
            //                   && a.Deleted == false
            //                   && b.Deleted == false
            //                   select new receiptView
            //                   {
            //                       BonNo = a.ReturNo,
            //                       BonDate = a.ReturDate,
            //                       ComodityCode = a.ComodityCode,
            //                       //ComodityName = a.ComodityName,
            //                       QtyProcess = b.Quantity,
            //                       QtySubcon = 0
            //                   });
            ////var returexpend = (from a in garmentExpenditureGoodReturnRepository.Query
            ////                   join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
            ////                   join c in finishedgoodstock on b.FinishedGoodStockId equals c.FinishedGoodStockId
            ////                   join e in garmentFinishingOutItemRepository.Query on c.FinishingOutItemId equals e.Identity
            ////                   join f in garmentFinishingInItemRepository.Query on e.FinishingInItemId equals f.Identity
            ////                   join g in garmentFinishingInRepository.Query on f.FinishingInId equals g.Identity
            ////                   where returexpendid.Contains(b.Identity)
            ////                   && a.Deleted == false
            ////                   && b.Deleted == false
            ////                   && (g.FinishingInType == "SEWING" || g.FinishingInType == "PEMBELIAN")
            ////                   select new receiptView
            ////                   {
            ////                      BonNo = a.ReturNo,
            ////                      BonDate = a.ReturDate,
            ////                      ComodityCode = a.ComodityCode,
            ////                      //ComodityName = a.ComodityName,
            ////                      QtyProcess = g.FinishingInType == "SEWING" ? b.Quantity : 0,
            ////                      QtySubcon = g.FinishingInType == "PEMBELIAN" ? b.Quantity : 0
            ////                   });
            //#endregion

            #endregion

            #region sc baru

            #region finishing
            var finishingbarangjadi = from a in (from aa in garmentFinishingOutRepository.Query
                                                 where aa.FinishingTo == "GUDANG JADI"
                                                 && aa.FinishingOutDate.AddHours(7).Date <= dateTo.Date
                                                 && aa.FinishingOutDate.AddHours(7).Date >= dateFrom.Date
                                                 && aa.Deleted == false
                                                 select new
                                                 {
                                                     aa.RONo,
                                                     aa.Identity,
                                                     aa.FinishingOutDate,
                                                     aa.FinishingOutNo,
                                                     aa.ComodityCode,
                                                     aa.ComodityName
                                                 })
                                      join b in (from bb in garmentFinishingOutItemRepository.Query
                                                 where bb.CustomsCategory == "LOKAL FASILITAS" || bb.CustomsCategory == "IMPORT FASILITAS"
                                                 select bb) on a.Identity equals b.FinishingOutId
                                      join c in garmentFinishingInItemRepository.Query on b.FinishingInItemId equals c.Identity
                                      join d in garmentFinishingInRepository.Query on c.FinishingInId equals d.Identity
                                      where b.Deleted == false
                                      && c.Deleted == false
                                      && d.Deleted == false
                                      && (d.FinishingInType == "SEWING" || d.FinishingInType == "PEMBELIAN")
                                      select new receiptView
                                      {
                                          BonNo = a.FinishingOutNo,
                                          BonDate = a.FinishingOutDate,
                                          ComodityCode = a.ComodityCode,
                                          //ComodityName = a.ComodityName,
                                          QtyProcess = d.FinishingInType == "SEWING" ? b.Quantity : 0,
                                          QtySubcon = d.FinishingInType == "PEMBELIAN" ? b.Quantity : 0

                                      };
            #endregion

            #region return
            var returexpend = (from a in garmentExpenditureGoodReturnRepository.Query
                               join b in (from bb in garmentExpenditureGoodReturnItemRepository.Query
                                          where bb.CustomsCategory == "LOKAL FASILITAS" || bb.CustomsCategory == "IMPORT FASILITAS"
                                          select bb) on a.Identity equals b.ReturId
                               where a.ReturDate.AddHours(7).Date <= dateTo.Date
                               && a.ReturDate.AddHours(7).Date >= dateFrom.Date
                               && a.Deleted == false
                               && b.Deleted == false
                               select new receiptView
                               {
                                   BonNo = a.ReturNo,
                                   BonDate = a.ReturDate,
                                   ComodityCode = a.ComodityCode,
                                   //ComodityName = a.ComodityName,
                                   QtyProcess = b.Quantity,
                                   QtySubcon = 0
                               });
            #endregion

            #endregion
            //var queryNow = returexpend.Union(finishingbarangjadi).Union(adjustin).Union(adjustout).AsEnumerable();
            var queryNow = finishingbarangjadi.Union(returexpend).AsEnumerable();

            var queryTemp = queryNow.GroupBy(x => new { x.BonNo, x.ComodityCode }, (key, group) => new
            {
                kodeBarang = key.ComodityCode,
                //namaBarang = key.ComodityName,
                bonNo = key.BonNo,
                bonDate = group.FirstOrDefault().BonDate,
                qtyProcess = group.Sum(x => x.QtyProcess),
                qtySubcon = group.Sum(x => x.QtySubcon),
                //unitQtyName = "PCS",
                //gudang = "-"
            });

            foreach (var i in queryTemp/*.Where(x => x.qtyProcess != 0 || x.qtySubcon != 0)*/)
            {
                var comodity = (from a in garmentCuttingOutRepository.Query
                                where a.ComodityCode == i.kodeBarang
                                select a.ComodityName).FirstOrDefault();

                GarmentReceiptFinishedGoodDto dto = new GarmentReceiptFinishedGoodDto
                {
                    KodeBarang = i.kodeBarang,
                    NamaBarang = comodity,
                    BonMasuk = i.bonNo,
                    TglBonMasuk = i.bonDate,
                    QtyProcess = i.qtyProcess,
                    QtySubcon = i.qtySubcon,
                    UnitQtyName = "PCS",
                    Gudang = "-",
                };

                receiptGoodDto.Add(dto);
            }

            receiptGoodListViewModel.garmentGoodsReceipt = receiptGoodDto.OrderBy(x => x.TglBonMasuk).ToList();
            return receiptGoodListViewModel;
        }
    }
}
           


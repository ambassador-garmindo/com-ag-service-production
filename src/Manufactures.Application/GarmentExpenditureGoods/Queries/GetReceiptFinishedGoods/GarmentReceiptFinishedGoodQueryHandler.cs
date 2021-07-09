using ExtCore.Data.Abstractions;
using Infrastructure.Domain.Queries;
using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentCuttingOuts.Repositories;
using Manufactures.Domain.GarmentExpenditureGoodReturns.Repositories;
using Manufactures.Domain.GarmentFinishedGoodStocks.Repositories;
using Manufactures.Domain.GarmentFinishingIns.Repositories;
using Manufactures.Domain.GarmentFinishingOuts.Repositories;
using Manufactures.Domain.GarmentPreparings.Repositories;
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
        private readonly IGarmentFinishingOutRepository garmentFinishingOutRepository;
        private readonly IGarmentFinishingOutItemRepository garmentFinishingOutItemRepository;
        private readonly IGarmentFinishingInRepository garmentFinishingInRepository;
        private readonly IGarmentFinishingInItemRepository garmentFinishingInItemRepository;
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
            garmentCuttingOutRepository = storage.GetRepository<IGarmentCuttingOutRepository>();
            garmentCuttingOutItemRepository = storage.GetRepository<IGarmentCuttingOutItemRepository>();
            garmentCuttingOutDetailRepository = storage.GetRepository<IGarmentCuttingOutDetailRepository>();
            garmentCuttingInRepository = storage.GetRepository<IGarmentCuttingInRepository>();
            garmentCuttingInItemRepository = storage.GetRepository<IGarmentCuttingInItemRepository>();
            garmentCuttingInDetailRepository = storage.GetRepository<IGarmentCuttingInDetailRepository>();
            garmentFinishingOutRepository = storage.GetRepository<IGarmentFinishingOutRepository>();
            garmentFinishingOutItemRepository = storage.GetRepository<IGarmentFinishingOutItemRepository>();
            garmentFinishingInRepository = storage.GetRepository<IGarmentFinishingInRepository>();
            garmentFinishingInItemRepository = storage.GetRepository<IGarmentFinishingInItemRepository>();
            garmentFinishedGoodStockRepository = storage.GetRepository<IGarmentFinishedGoodStockRepository>();
            garmentFinishedGoodStockHistoryRepository = storage.GetRepository<IGarmentFinishedGoodStockHistoryRepository>();
            garmentExpenditureGoodReturnRepository = storage.GetRepository<IGarmentExpenditureGoodReturnRepository>();
            garmentExpenditureGoodReturnItemRepository = storage.GetRepository<IGarmentExpenditureGoodReturnItemRepository>();
            garmentPreparingRepository = storage.GetRepository<IGarmentPreparingRepository>();
            garmentPreparingItemRepository = storage.GetRepository<IGarmentPreparingItemRepository>();
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

            var finishingbarangjadiid = (from a in (from aa in garmentFinishingOutRepository.Query
                                                    where aa.FinishingOutDate.AddHours(7).Date <= dateTo.Date
                                                    && aa.FinishingOutDate.AddHours(7).Date >= dateFrom.Date
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
                                                     aa.FinishingOutDate,
                                                     aa.FinishingOutNo,
                                                     aa.ComodityCode,
                                                     aa.ComodityName
                                                 })
                                      join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId
                                      join c in garmentFinishingInItemRepository.Query on b.FinishingInItemId equals c.Identity
                                      join d in garmentFinishingInRepository.Query on c.FinishingInId equals d.Identity
                                      where finishingbarangjadiid.Contains(b.Identity)
                                      && b.Deleted == false
                                      && c.Deleted == false
                                      && (d.FinishingInType == "SEWING" || d.FinishingInType == "PEMBELIAN")
                                      select new receiptView
                                      {
                                          BonNo = a.FinishingOutNo,
                                          BonDate =  a.FinishingOutDate,
                                          ComodityCode = a.ComodityCode,
                                          //ComodityName = a.ComodityName,
                                          QtyProcess = d.FinishingInType == "SEWING" ? b.Quantity : 0,
                                          QtySubcon = d.FinishingInType == "PEMBELIAN" ? b.Quantity : 0

                                      };

            var returexpendid = (from a in (from aa in garmentExpenditureGoodReturnRepository.Query
                                            where aa.ReturDate.AddHours(7).Date <= dateTo.Date
                                            && aa.ReturDate.AddHours(7).Date >= dateFrom.Date
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

            //var finishedgoodstock = (from a in garmentFinishedGoodStockRepository.Query
            //                         join b in garmentFinishedGoodStockHistoryRepository.Query on a.Identity equals b.FinishedGoodStockId
            //                         where b.StockType == "IN"
            //                         select new {
            //                             FinishedGoodStockId = a.Identity,
            //                             FinishingOutItemId = b.FinishingOutItemId
            //                         });

            //finishedgoodstock.GroupBy(x => new { x.FinishedGoodStockId, x.FinishingOutItemId }, (key, group) => new
            //{
            //    FinishedGoodStockId = key.FinishedGoodStockId,
            //    FinishedOutItemId = key.FinishingOutItemId
            //});

            var returexpend = (from a in garmentExpenditureGoodReturnRepository.Query
                               join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
                               where returexpendid.Contains(b.Identity)
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
            //var returexpend = (from a in garmentExpenditureGoodReturnRepository.Query
            //                   join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
            //                   join c in finishedgoodstock on b.FinishedGoodStockId equals c.FinishedGoodStockId
            //                   join e in garmentFinishingOutItemRepository.Query on c.FinishingOutItemId equals e.Identity
            //                   join f in garmentFinishingInItemRepository.Query on e.FinishingInItemId equals f.Identity
            //                   join g in garmentFinishingInRepository.Query on f.FinishingInId equals g.Identity
            //                   where returexpendid.Contains(b.Identity)
            //                   && a.Deleted == false
            //                   && b.Deleted == false
            //                   && (g.FinishingInType == "SEWING" || g.FinishingInType == "PEMBELIAN")
            //                   select new receiptView
            //                   {
            //                      BonNo = a.ReturNo,
            //                      BonDate = a.ReturDate,
            //                      ComodityCode = a.ComodityCode,
            //                      //ComodityName = a.ComodityName,
            //                      QtyProcess = g.FinishingInType == "SEWING" ? b.Quantity : 0,
            //                      QtySubcon = g.FinishingInType == "PEMBELIAN" ? b.Quantity : 0
            //                   });

            var queryNow = returexpend.Union(finishingbarangjadi).AsEnumerable();
            //var queryNow = finishingbarangjadi.AsEnumerable();

            var queryTemp = queryNow.GroupBy(x => new { x.BonNo, x.BonDate, x.ComodityCode }, (key, group) => new
            {
                kodeBarang = key.ComodityCode,
                //namaBarang = key.ComodityName,
                bonNo = key.BonNo,
                bonDate = key.BonDate,
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

            receiptGoodListViewModel.garmentGoodsReceipt = receiptGoodDto.OrderBy(x => x.KodeBarang).ToList();
            return receiptGoodListViewModel;
        }
    }
}
           


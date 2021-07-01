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
using System.IO;
using System.Data;
using OfficeOpenXml;
using Manufactures.Domain.GarmentFinishedGoodStocks.Repositories;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentPreparings.Repositories;

namespace Manufactures.Application.GarmentExpenditureGoods.Queries.GetMutationExpenditureGoods
{
    public class GetXlsMutationExpenditureGoodsQueryHandler : IQueryHandler<GetXlsMutationExpenditureGoodsQuery, MemoryStream>
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
        private readonly IGarmentCuttingOutRepository garmentCuttingOutRepository;
        private readonly IGarmentCuttingOutItemRepository garmentCuttingOutItemRepository;
        private readonly IGarmentCuttingOutDetailRepository garmentCuttingOutDetailRepository;
        private readonly IGarmentCuttingInRepository garmentCuttingInRepository;
        private readonly IGarmentCuttingInItemRepository garmentCuttingInItemRepository;
        private readonly IGarmentCuttingInDetailRepository garmentCuttingInDetailRepository;
        private readonly IGarmentPreparingRepository garmentPreparingRepository;
        private readonly IGarmentPreparingItemRepository garmentPreparingItemRepository;

        public GetXlsMutationExpenditureGoodsQueryHandler(IStorage storage, IServiceProvider serviceProvider)
        {
            _storage = storage;
            garmentBalanceMonitoringProductionStockFlowRepository = storage.GetRepository<IGarmentBalanceMonitoringProductionStockFlowRepository>();
            garmentCuttingOutRepository = storage.GetRepository<IGarmentCuttingOutRepository>();
            garmentCuttingOutItemRepository = storage.GetRepository<IGarmentCuttingOutItemRepository>();
            garmentCuttingOutDetailRepository = storage.GetRepository<IGarmentCuttingOutDetailRepository>();
            garmentCuttingInRepository = storage.GetRepository<IGarmentCuttingInRepository>();
            garmentCuttingInItemRepository = storage.GetRepository<IGarmentCuttingInItemRepository>();
            garmentCuttingInDetailRepository = storage.GetRepository<IGarmentCuttingInDetailRepository>();
            garmentAdjustmentRepository = storage.GetRepository<IGarmentAdjustmentRepository>();
            garmentAdjustmentItemRepository = storage.GetRepository<IGarmentAdjustmentItemRepository>();
            garmentFinishingOutRepository = storage.GetRepository<IGarmentFinishingOutRepository>();
            garmentFinishingOutItemRepository = storage.GetRepository<IGarmentFinishingOutItemRepository>();
            garmentFinishingInRepository = storage.GetRepository<IGarmentFinishingInRepository>();
            garmentFinishingInItemRepository = storage.GetRepository<IGarmentFinishingInItemRepository>();
            garmentExpenditureGoodRepository = storage.GetRepository<IGarmentExpenditureGoodRepository>();
            garmentExpenditureGoodItemRepository = storage.GetRepository<IGarmentExpenditureGoodItemRepository>();
            garmentFinishedGoodStockRepository = storage.GetRepository<IGarmentFinishedGoodStockRepository>();
            garmentFinishedGoodStockHistoryRepository = storage.GetRepository<IGarmentFinishedGoodStockHistoryRepository>();
            garmentExpenditureGoodReturnRepository = storage.GetRepository<IGarmentExpenditureGoodReturnRepository>();
            garmentExpenditureGoodReturnItemRepository = storage.GetRepository<IGarmentExpenditureGoodReturnItemRepository>();
            garmentPreparingRepository = storage.GetRepository<IGarmentPreparingRepository>();
            garmentPreparingItemRepository = storage.GetRepository<IGarmentPreparingItemRepository>();
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

        public async Task<MemoryStream> Handle(GetXlsMutationExpenditureGoodsQuery request, CancellationToken cancellationToken)
        {
            GarmentMutationExpenditureGoodListViewModel expenditureGoodListViewModel = new GarmentMutationExpenditureGoodListViewModel();
            List<GarmentMutationExpenditureGoodDto> mutationExpenditureGoodDto = new List<GarmentMutationExpenditureGoodDto>();

            DateTimeOffset dateFrom = new DateTimeOffset(request.dateFrom, new TimeSpan(7, 0, 0));
            DateTimeOffset dateTo = new DateTimeOffset(request.dateTo, new TimeSpan(7, 0, 0));
            //DateTimeOffset dateBalance = (from a in garmentBalanceMonitoringProductionStockFlowRepository.Query select a.CreatedDate).FirstOrDefault();

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
            //                        where aa.AdjustmentDate >= dateBalance && aa.AdjustmentDate <= dateTo
            //                        && aa.AdjustmentType == "FINISHING"
            //                        select aa)
            //             join b in garmentAdjustmentItemRepository.Query on a.Identity equals b.AdjustmentId
            //             select new mutationView
            //             {
            //                 SaldoQtyFin = a.AdjustmentDate < dateFrom && a.AdjustmentDate > dateBalance ? b.Quantity : 0,
            //                 AdjFin = a.AdjustmentDate >= dateFrom ? b.Quantity : 0,
            //                 ComodityCode = a.ComodityCode,
            //                 QtyExpend = 0,
            //                 QtyFin = 0,
            //                 Retur = 0,
            //             };

            //var returexpend = from a in (from aa in garmentExpenditureGoodReturnRepository.Query
            //                             where aa.ReturDate >= dateBalance && aa.ReturDate <= dateTo //&& aa.ComodityCode == "BR"
            //                             select aa)
            //                  join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
            //                  select new mutationView
            //                  {
            //                      SaldoQtyFin = a.ReturDate < dateFrom && a.ReturDate > dateBalance ? b.Quantity : 0,
            //                      AdjFin = 0,
            //                      ComodityCode = a.ComodityCode,
            //                      QtyExpend = 0,
            //                      QtyFin = 0,
            //                      Retur = a.ReturDate >= dateFrom ? b.Quantity : 0
            //                  };

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
            //                              QtyFin = a.FinishingOutDate >= dateFrom ? b.Quantity : 0,
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
                                                 where aa.FinishingOutDate.AddHours(7).Date <= dateTo.Date
                                                 && aa.FinishingTo == "GUDANG JADI"
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
                                      where finishingbarangjadiid.Contains(b.Identity)
                                      && b.Deleted == false
                                      select new mutationView
                                      {
                                          SaldoQtyFin = a.FinishingOutDate.AddHours(7).Date < dateFrom.Date ? b.Quantity : 0,
                                          AdjFin = 0,
                                          ComodityCode = a.ComodityCode,
                                          ComodityName = a.ComodityName,
                                          QtyExpend = 0,
                                          QtyFin = a.FinishingOutDate.AddHours(7).Date >= dateFrom.Date ? b.Quantity : 0,
                                          Retur = 0,
                                      };

            var returexpendid = (from a in (from aa in garmentExpenditureGoodReturnRepository.Query
                                            where aa.ReturDate.AddHours(7).Date <= dateTo.Date
                                            select aa)
                                 join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
                                 join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                                 join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                                 join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                                 from f in (from ff in garmentFinishingOutRepository.Query
                                            where 
                                            //ff.FinishingOutDate.AddHours(7).Date <= dateTo.Date
                                            ff.FinishingTo == "GUDANG JADI"
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

            var returexpend = from a in (from aa in garmentExpenditureGoodReturnRepository.Query
                                         where aa.ReturDate.AddHours(7).Date <= dateTo.Date
                                         select aa)
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
                                  Retur = a.ReturDate.AddHours(7).Date >= dateFrom.Date ? b.Quantity : 0
                              };

            var factexpendid = (from a in (from aa in garmentExpenditureGoodRepository.Query
                                           where aa.ExpenditureDate.AddHours(7).Date <= dateTo.Date
                                           select aa)
                                join b in garmentExpenditureGoodItemRepository.Query on a.Identity equals b.ExpenditureGoodId
                                join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                                join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                                join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                                from f in (from ff in garmentFinishingOutRepository.Query
                                           where 
                                           //ff.FinishingOutDate.AddHours(7).Date <= dateTo.Date
                                           ff.FinishingTo == "GUDANG JADI"
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

            var factexpend = from a in (from aa in garmentExpenditureGoodRepository.Query
                                        where aa.ExpenditureDate.AddHours(7).Date <= dateTo.Date
                                        select aa)
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
                                 QtyExpend = a.ExpenditureDate.AddHours(7).Date >= dateFrom.Date ? b.Quantity : 0,
                                 QtyFin = 0,
                                 Retur = 0,
                             };

            //var queryNow = adjustin.Union(returexpend).Union(finishingbarangjadi).Union(factexpend).AsEnumerable();
            var queryNow = returexpend.Union(finishingbarangjadi).Union(factexpend).AsEnumerable();
            var mutationTemp = queryNow.GroupBy(x => new { x.ComodityCode, x.ComodityName }, (key, group) => new
            {
                kodeBarang = key.ComodityCode,
                //namaBarang = group.FirstOrDefault().Comodity,
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

            foreach (var i in mutationTemp.Where(x => x.saldoAwal != 0 || x.pemasukan != 0 || x.pengeluaran != 0 || x.penyesuaian != 0 || x.stockOpname != 0 || x.saldoBuku != 0))
            {
                var comodity = (from a in garmentCuttingOutRepository.Query
                                where a.ComodityCode == i.kodeBarang
                                select a.ComodityName).FirstOrDefault();

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
                    Gudang = i.gudang
                };

                mutationExpenditureGoodDto.Add(dto);
            }
        
            expenditureGoodListViewModel.garmentMutations = mutationExpenditureGoodDto.OrderBy(x => x.KodeBarang).ToList();

            var reportDataTable = new DataTable();
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Sat", DataType = typeof(string) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Saldo Awal", DataType = typeof(double) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Pemasukan", DataType = typeof(double) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Pengeluaran", DataType = typeof(double) });
            reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Saldo Buku", DataType = typeof(double) });
            int counter = 1;
            foreach (var report in expenditureGoodListViewModel.garmentMutations)
            {
                reportDataTable.Rows.Add(counter, report.KodeBarang, report.NamaBarang, report.UnitQtyName, report.SaldoAwal, report.Pemasukan, report.Pengeluaran, report.SaldoBuku);
                counter++;
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");

                worksheet.Cells["A2"].LoadFromDataTable(reportDataTable, true, OfficeOpenXml.Table.TableStyles.Light16);

                var stream = new MemoryStream();

                package.SaveAs(stream);

                return stream;

            }



        }
    }
}

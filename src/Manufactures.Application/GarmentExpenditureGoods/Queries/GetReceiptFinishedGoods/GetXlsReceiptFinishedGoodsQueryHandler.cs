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
using System.IO;
using Manufactures.Application.GarmentExpenditureGoods.Queries.GetReceiptFinishedGoods;
using System.Data;
using OfficeOpenXml;
using System.Globalization;
using OfficeOpenXml.Style;

namespace Manufactures.Application.GarmentExpenditureGoods.Queries.GetMutationExpenditureGoods
{
    public class GetXlsReceiptFinishedGoodsQueryHandler : IQueryHandler<GetXlsReceiptFinishedGoodsQuery, MemoryStream>
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

        public GetXlsReceiptFinishedGoodsQueryHandler(IStorage storage, IServiceProvider serviceProvider)
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

        public async Task<MemoryStream> Handle(GetXlsReceiptFinishedGoodsQuery request, CancellationToken cancellationToken)
        {
            GarmentReceiptFinishedGoodListViewModel receiptGoodListViewModel = new GarmentReceiptFinishedGoodListViewModel();
            List<GarmentReceiptFinishedGoodDto> receiptGoodDto = new List<GarmentReceiptFinishedGoodDto>();

            DateTimeOffset dateFrom = new DateTimeOffset(request.dateFrom, new TimeSpan(7, 0, 0));
            DateTimeOffset dateTo = new DateTimeOffset(request.dateTo, new TimeSpan(7, 0, 0));

            var finishingbarangjadiid = (from a in (from aa in garmentFinishingOutRepository.Query
                                                    where aa.FinishingOutDate <= dateTo
                                                    && aa.FinishingOutDate >= dateFrom
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
                                                 where aa.FinishingOutDate <= dateTo
                                                 && aa.FinishingOutDate >= dateFrom
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
                                      join c in garmentFinishingInItemRepository.Query on b.FinishingInItemId equals c.Identity
                                      join d in garmentFinishingInRepository.Query on c.FinishingInId equals d.Identity
                                      where finishingbarangjadiid.Contains(b.Identity)
                                      && b.Deleted == false
                                      && c.Deleted == false
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

            var returexpendid = (from a in (from aa in garmentExpenditureGoodReturnRepository.Query
                                            where aa.ReturDate <= dateTo
                                            && aa.ReturDate >= dateFrom
                                            select aa)
                                 join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
                                 join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                                 join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                                 join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                                 join f in (from ff in garmentFinishingOutRepository.Query
                                            where ff.FinishingOutDate <= dateTo
                                            && ff.FinishingTo == "GUDANG JADI"
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

            var returexpend = from a in (from aa in garmentExpenditureGoodReturnRepository.Query
                                         where aa.ReturDate <= dateTo
                                         select aa)
                              join b in garmentExpenditureGoodReturnItemRepository.Query on a.Identity equals b.ReturId
                              join c in garmentFinishedGoodStockRepository.Query on b.FinishedGoodStockId equals c.Identity
                              join d in garmentFinishedGoodStockHistoryRepository.Query on c.Identity equals d.FinishedGoodStockId
                              join e in garmentFinishingOutItemRepository.Query on d.FinishingOutItemId equals e.Identity
                              join f in garmentFinishingInItemRepository.Query on e.FinishingInItemId equals f.Identity
                              join g in garmentFinishingInRepository.Query on f.FinishingInId equals g.Identity
                              where returexpendid.Contains(b.Identity)
                              && a.Deleted == false
                              && b.Deleted == false
                              && (g.FinishingInType == "SEWING" || g.FinishingInType == "PEMBELIAN")
                              select new receiptView
                              {
                                  BonNo = a.ReturNo,
                                  BonDate = a.ReturDate,
                                  ComodityCode = a.ComodityCode,
                                  //ComodityName = a.ComodityName,
                                  QtyProcess = g.FinishingInType == "SEWING" ? b.Quantity : 0,
                                  QtySubcon = g.FinishingInType == "PEMBELIAN" ? b.Quantity : 0
                              };

            var queryNow = returexpend.Union(finishingbarangjadi).AsEnumerable();

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

            foreach (var i in queryTemp.Where(x => x.qtyProcess != 0 || x.qtySubcon != 0))
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

            //excel excel

            #region new

            DataTable result = new DataTable();

            var headers = new string[] { "No", "Bukti Penerimaan", "Bukti Penerimaan1", "Kode Barang", "Nama Barang", "Satuan", "Jumlah", "Jumlah1", "Gudang" };
            var subheaders = new string[] { "Nomor", "Tanggal", "Dari Produksi", "Dari Subkontrak" };

            double ProcessQtyTotal = 0;
            double SubconQtyTotal = 0;

            for (int i = 0; i < 6; i++)
            {
                result.Columns.Add(new DataColumn() { ColumnName = headers[i], DataType = typeof(String) });
            }

            result.Columns.Add(new DataColumn() { ColumnName = headers[6], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[7], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[8], DataType = typeof(String) });

            if (receiptGoodListViewModel.garmentGoodsReceipt.Count == 0)
            {
                result.Rows.Add("", "", "", "", "", "", "", "", "");
            }

            else
            {
                var docNo = receiptGoodListViewModel.garmentGoodsReceipt.ToArray();
                var q = receiptGoodListViewModel.garmentGoodsReceipt.ToList();
                var length = 0;

                foreach (GarmentReceiptFinishedGoodDto temp in q)
                {
                    GarmentReceiptFinishedGoodDto dup = Array.Find(docNo, o => o.BonMasuk == temp.BonMasuk);
                    if(dup != null)
                    {
                        if(dup.Tot == 0)
                        {
                            length++;
                            dup.Tot = length;
                        }
                    }
                    temp.Tot = dup.Tot;
                }

                foreach(var item in q)
                {
                    ProcessQtyTotal += item.QtyProcess;
                    SubconQtyTotal += item.QtySubcon;

                    result.Rows.Add(item.Tot, item.BonMasuk, item.TglBonMasuk.ToString("dd MMM yyyy", new CultureInfo("id-ID")), item.KodeBarang, item.NamaBarang, item.UnitQtyName, item.QtyProcess, item.QtySubcon, item.Gudang);
                }
            }

            ExcelPackage package = new ExcelPackage();
            bool styling = true;

            foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") })
            {
                var worksheet = package.Workbook.Worksheets.Add(item.Value);

                Dictionary<string, int> counts = new Dictionary<string, int>();

                var col = (char)('A' + result.Columns.Count);
                string tglawal = dateFrom.ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                string tglakhir = dateTo.ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                worksheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN PEMASUKAN HASIL PRODUKSI");
                worksheet.Cells[$"A1:{col}1"].Merge = true;
                worksheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;

                worksheet.Cells[$"A2:{col}2"].Value = string.Format("Periode {0} - {1}", tglawal, tglakhir);
                worksheet.Cells[$"A2:{col}2"].Merge = true;
                worksheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
                worksheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                worksheet.Cells["A7"].LoadFromDataTable(item.Key, false, (styling == true) ? OfficeOpenXml.Table.TableStyles.Light16 : OfficeOpenXml.Table.TableStyles.None);

                worksheet.Cells["A5"].Value = headers[0];
                worksheet.Cells["A5:A6"].Merge = true;

                worksheet.Cells["B5"].Value = headers[1];
                worksheet.Cells["B5:C5"].Merge = true;

                worksheet.Cells["B6"].Value = subheaders[0];
                worksheet.Cells["C6"].Value = subheaders[1];

                worksheet.Cells["D5"].Value = headers[3];
                worksheet.Cells["D5:D6"].Merge = true;

                worksheet.Cells["E5"].Value = headers[4];
                worksheet.Cells["E5:E6"].Merge = true;

                worksheet.Cells["F5"].Value = headers[5];
                worksheet.Cells["F5:F6"].Merge = true;

                worksheet.Cells["G5"].Value = headers[6];
                worksheet.Cells["G5:H5"].Merge = true;

                worksheet.Cells["G6"].Value = subheaders[2];
                worksheet.Cells["H6"].Value = subheaders[3];

                worksheet.Cells["I5"].Value = headers[8];
                worksheet.Cells["I5:I6"].Merge = true;

                worksheet.Cells["A5:I6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A5:I6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A5:I6"].Style.Font.Bold = true;

                var widths = new int[] { 10, 20, 20, 15, 15, 15, 20, 20, 20 };

                foreach (var i in Enumerable.Range(0, headers.Length))
                {
                    worksheet.Column(i + 1).Width = widths[i];
                }

                //var docNo = receiptGoodListViewModel.garmentGoodsReceipt.ToArray();
                int value;

                foreach(var b in receiptGoodListViewModel.garmentGoodsReceipt)
                {
                    if(counts.TryGetValue(b.BonMasuk, out value))
                    {
                        counts[b.BonMasuk]++;
                    }
                    else
                    {
                        counts[b.BonMasuk] = 1;
                    }
                }

                int index = 7;
                foreach (KeyValuePair<string, int> b in counts)
                {
                    worksheet.Cells["A" + index + ":A" + (index + b.Value - 1)].Merge = true;
                    worksheet.Cells["A" + index + ":A" + (index + b.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    worksheet.Cells["B" + index + ":B" + (index + b.Value - 1)].Merge = true;
                    worksheet.Cells["B" + index + ":B" + (index + b.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    worksheet.Cells["C" + index + ":C" + (index + b.Value - 1)].Merge = true;
                    worksheet.Cells["C" + index + ":C" + (index + b.Value - 1)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                    index += b.Value;
                }

                var a = receiptGoodListViewModel.garmentGoodsReceipt.Count();

                worksheet.Cells[$"A{8 + a}"].Value = "T O T A L  . . . . . . . . . . . . . . .";
                worksheet.Cells[$"A{8 + a}:F{8 + a}"].Merge = true;
                worksheet.Cells[$"A{8 + a}:F{8 + a}"].Style.Font.Bold = true;
                worksheet.Cells[$"A{8 + a}:F{8 + a}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A{8 + a}:F{8 + a}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[$"G{8 + a}"].Value = ProcessQtyTotal;
                worksheet.Cells[$"H{8 + a}"].Value = SubconQtyTotal;
            }

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;

            #endregion


            #region old
            //var reportDataTable = new DataTable();

            //var headers = new string[] { "No", "Bukti Penerimaan", "Bukti Penerimaan1", "Kode Barang", "Nama Barang", "Satuan", "Jumlah", "Jumlah1", "Gudang" };
            //var subheaders = new string[] { "Nomor", "Tanggal", "Dari Produksi", "Dari Subkontrak" };

            //for (int i = 0; i < 6; i++)
            //{
            //    reportDataTable.Columns.Add(new DataColumn() { ColumnName = headers[i], DataType = typeof(String) });
            //}

            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = headers[6], DataType = typeof(Double) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = headers[7], DataType = typeof(Double) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = headers[8], DataType = typeof(String) });

            //int counter = 1;
            //double ProcessQtyTotal = 0;
            //double SubconQtyTotal = 0;

            //foreach (var report in receiptGoodListViewModel.garmentGoodsReceipt)
            //{
            //    ProcessQtyTotal += report.QtyProcess;
            //    SubconQtyTotal += report.QtySubcon;

            //    reportDataTable.Rows.Add(counter, report.BonMasuk, report.TglBonMasuk.ToString("dd MMM yyyy", new CultureInfo("id-ID")), report.KodeBarang, report.NamaBarang, report.UnitQtyName, report.QtyProcess, report.QtySubcon, report.Gudang);
            //    counter++;
            //}

            //using (var package = new ExcelPackage())
            //{
            //    var worksheet = package.Workbook.Worksheets.Add("Sheet 1");

            //    var col = (char)('A' + reportDataTable.Columns.Count);
            //    string tglawal = dateFrom.ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            //    string tglakhir = dateTo.ToString("dd MMM yyyy", new CultureInfo("id-ID"));

            //    worksheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN PEMASUKAN HASIL PRODUKSI");
            //    worksheet.Cells[$"A1:{col}1"].Merge = true;
            //    worksheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            //    worksheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            //    worksheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;

            //    worksheet.Cells[$"A2:{col}2"].Value = string.Format("Periode {0} - {1}", tglawal, tglakhir);
            //    worksheet.Cells[$"A2:{col}2"].Merge = true;
            //    worksheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
            //    worksheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            //    worksheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            //    worksheet.Cells["A7"].LoadFromDataTable(reportDataTable, false, OfficeOpenXml.Table.TableStyles.Light16);

            //    worksheet.Cells["A5"].Value = headers[0];
            //    worksheet.Cells["A5:A6"].Merge = true;

            //    worksheet.Cells["B5"].Value = headers[1];
            //    worksheet.Cells["B5:C5"].Merge = true;

            //    worksheet.Cells["B6"].Value = subheaders[0];
            //    worksheet.Cells["C6"].Value = subheaders[1];

            //    worksheet.Cells["D5"].Value = headers[3];
            //    worksheet.Cells["D5:D6"].Merge = true;

            //    worksheet.Cells["E5"].Value = headers[4];
            //    worksheet.Cells["E5:E6"].Merge = true;

            //    worksheet.Cells["F5"].Value = headers[5];
            //    worksheet.Cells["F5:F6"].Merge = true;

            //    worksheet.Cells["G5"].Value = headers[6];
            //    worksheet.Cells["G5:H5"].Merge = true;

            //    worksheet.Cells["G6"].Value = subheaders[2];
            //    worksheet.Cells["H6"].Value = subheaders[3];

            //    worksheet.Cells["I5"].Value = headers[8];
            //    worksheet.Cells["I5:I6"].Merge = true;

            //    worksheet.Cells["A5:I6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //    worksheet.Cells["A5:I6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //    worksheet.Cells["A5:I6"].Style.Font.Bold = true;

            //    var widths = new int[] { 10, 20, 20, 15, 15, 15, 20, 20, 20 };

            //    foreach (var i in Enumerable.Range(0, headers.Length))
            //    {
            //        worksheet.Column(i + 1).Width = widths[i];
            //    }

            //    var a = receiptGoodListViewModel.garmentGoodsReceipt.Count();

            //    worksheet.Cells[$"A{8 + a}"].Value = "T O T A L  . . . . . . . . . . . . . . .";
            //    worksheet.Cells[$"A{8 + a}:F{8 + a}"].Merge = true;
            //    worksheet.Cells[$"A{8 + a}:F{8 + a}"].Style.Font.Bold = true;
            //    worksheet.Cells[$"A{8 + a}:F{8 + a}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //    worksheet.Cells[$"A{8 + a}:F{8 + a}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //    worksheet.Cells[$"G{8 + a}"].Value = ProcessQtyTotal;
            //    worksheet.Cells[$"H{8 + a}"].Value = SubconQtyTotal;

            //    MemoryStream stream = new MemoryStream();
            //    package.SaveAs(stream);
            //    return stream;
            //}
            #endregion
        }
    }
}

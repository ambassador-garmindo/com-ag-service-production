using ExtCore.Data.Abstractions;
using Infrastructure.Domain.Queries;
using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using static Infrastructure.External.DanLirisClient.Microservice.MasterResult.CostCalculationGarmentDataProductionReport;
using System.IO;
using Manufactures.Domain.GarmentSewingOuts.Repositories;
using Manufactures.Domain.GarmentFinishingOuts.Repositories;
using Infrastructure.External.DanLirisClient.Microservice.MasterResult;
using Infrastructure.External.DanLirisClient.Microservice;
using System.Data;
using OfficeOpenXml;
using System.Threading.Tasks;
using static Infrastructure.External.DanLirisClient.Microservice.MasterResult.HOrderDataProductionReport;
using OfficeOpenXml.Style;
using Manufactures.Domain.GarmentPreparings.Repositories;
using System.Net.Http;
using System.Text;

namespace Manufactures.Application.GarmentFinishingOuts.Queries
{
	public class GetXlsFinishingQueryHandler : IQueryHandler<GetXlsFinishingQuery, MemoryStream>
	{
		protected readonly IHttpClientService _http;
		private readonly IStorage _storage;
		private readonly IGarmentSewingOutRepository garmentSewingOutRepository;
		private readonly IGarmentSewingOutItemRepository garmentSewingOutItemRepository;
		private readonly IGarmentFinishingOutRepository garmentFinishingOutRepository;
		private readonly IGarmentFinishingOutItemRepository garmentFinishingOutItemRepository;
		private readonly IGarmentPreparingRepository garmentPreparingRepository;
		private readonly IGarmentPreparingItemRepository garmentPreparingItemRepository;
		public GetXlsFinishingQueryHandler(IStorage storage, IServiceProvider serviceProvider)
		{
			_storage = storage;
			garmentSewingOutRepository = storage.GetRepository<IGarmentSewingOutRepository>();
			garmentSewingOutItemRepository = storage.GetRepository<IGarmentSewingOutItemRepository>();
			garmentPreparingRepository = storage.GetRepository<IGarmentPreparingRepository>();
			garmentPreparingItemRepository = storage.GetRepository<IGarmentPreparingItemRepository>();
			garmentFinishingOutRepository = storage.GetRepository<IGarmentFinishingOutRepository>();
			garmentFinishingOutItemRepository = storage.GetRepository<IGarmentFinishingOutItemRepository>();
			_http = serviceProvider.GetService<IHttpClientService>();
		}

        async Task<HOrderDataProductionReport> GetDataHOrder(List<string> ro, string token)
        {
            HOrderDataProductionReport hOrderDataProductionReport = new HOrderDataProductionReport();

            var listRO = string.Join(",", ro.Distinct());
            var costCalculationUri = SalesDataSettings.Endpoint + $"local-merchandiser/horders/data-production-report-by-no/{listRO}";
            var httpResponse = await _http.GetAsync(costCalculationUri, token);

            if (httpResponse.IsSuccessStatusCode)
            {
                var contentString = await httpResponse.Content.ReadAsStringAsync();
                Dictionary<string, object> content = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentString);
                var dataString = content.GetValueOrDefault("data").ToString();
                var listData = JsonConvert.DeserializeObject<List<HOrderViewModel>>(dataString);

                foreach (var item in ro)
                {
                    var data = listData.SingleOrDefault(s => s.No == item);
                    if (data != null)
                    {
                        hOrderDataProductionReport.data.Add(data);
                    }
                }
            }

            return hOrderDataProductionReport;
        }

        public async Task<CostCalculationGarmentDataProductionReport> GetDataCostCal(List<string> ro, string token)
        {
            CostCalculationGarmentDataProductionReport costCalculationGarmentDataProductionReport = new CostCalculationGarmentDataProductionReport();

            var listRO = string.Join(",", ro.Distinct());
            var costCalculationUri = SalesDataSettings.Endpoint + $"cost-calculation-garments/data/";

            var httpContent = new StringContent(JsonConvert.SerializeObject(listRO), Encoding.UTF8, "application/json");

            var httpResponse = await _http.SendAsync(HttpMethod.Get, costCalculationUri, token, httpContent);

            var freeRO = new List<string>();

            if (httpResponse.IsSuccessStatusCode)
            {
                var contentString = await httpResponse.Content.ReadAsStringAsync();
                Dictionary<string, object> content = JsonConvert.DeserializeObject<Dictionary<string, object>>(contentString);
                var dataString = content.GetValueOrDefault("data").ToString();
                var listData = JsonConvert.DeserializeObject<List<CostCalViewModel>>(dataString);

                foreach (var item in ro)
                {
                    var data = listData.SingleOrDefault(s => s.ro == item);
                    if (data != null)
                    {
                        costCalculationGarmentDataProductionReport.data.Add(data);
                    }
                    else
                    {
                        freeRO.Add(item);
                    }
                }
            }

            HOrderDataProductionReport hOrderDataProductionReport = await GetDataHOrder(freeRO, token);

            Dictionary<string, string> comodities = new Dictionary<string, string>();
            if (hOrderDataProductionReport.data.Count > 0)
            {
                var comodityCodes = hOrderDataProductionReport.data.Select(s => s.Kode).Distinct().ToList();
                var filter = "{\"(" + string.Join(" || ", comodityCodes.Select(s => "Code==" + "\\\"" + s + "\\\"")) + ")\" : \"true\"}";

                var masterGarmentComodityUri = MasterDataSettings.Endpoint + $"master/garment-comodities?filter=" + filter;
                var garmentComodityResponse = _http.GetAsync(masterGarmentComodityUri).Result;
                var garmentComodityResult = new GarmentComodityResult();
                if (garmentComodityResponse.IsSuccessStatusCode)
                {
                    garmentComodityResult = JsonConvert.DeserializeObject<GarmentComodityResult>(garmentComodityResponse.Content.ReadAsStringAsync().Result);
                    //comodities = garmentComodityResult.data.ToDictionary(d => d.Code, d => d.Name);
                    foreach (var comodity in garmentComodityResult.data)
                    {
                        comodities[comodity.Code] = comodity.Name;
                    }
                }
            }

            foreach (var hOrder in hOrderDataProductionReport.data)
            {
                costCalculationGarmentDataProductionReport.data.Add(new CostCalViewModel
                {
                    ro = hOrder.No,
                    buyerCode = hOrder.Codeby,
                    comodityName = comodities.GetValueOrDefault(hOrder.Kode),
                    hours = (double)hOrder.Sh_Cut,
                    qtyOrder = (double)hOrder.Qty
                });
            }

            return costCalculationGarmentDataProductionReport;
        }

        class monitoringView
		{
			public string roJob { get; internal set; }
			public string article { get; internal set; }
			public string buyerCode { get; internal set; }
			public double qtyOrder { get; internal set; }
			public double stock { get; internal set; }
			public string style { get; internal set; }
			public double sewingQtyPcs { get; internal set; }
			public double finishingQtyPcs { get; internal set; }
			public string uomUnit { get; internal set; }
			public double remainQty { get; internal set; }
			public decimal price { get; internal set; }
		}
		class ViewBasicPrices
		{
			public string RO { get; internal set; }
			public decimal BasicPrice { get; internal set; }
			public int Count { get; internal set; }
		}
		public async Task<MemoryStream> Handle(GetXlsFinishingQuery request, CancellationToken cancellationToken)
		{
			DateTimeOffset dateFrom = new DateTimeOffset(request.dateFrom, new TimeSpan(7, 0, 0));
			DateTimeOffset dateTo = new DateTimeOffset(request.dateTo, new TimeSpan(7, 0, 0));

			var QueryRoFinishing = (from a in garmentFinishingOutRepository.Query
									where a.UnitId == request.unit && a.FinishingOutDate <= dateTo
									select a.RONo).Distinct();
			var QueryRoSewingOut = (from a in garmentSewingOutRepository.Query
									where a.UnitToId == request.unit && a.SewingOutDate <= dateTo && a.SewingTo == "FINISHING"
									select a.RONo).Distinct();
			var QueryRo = QueryRoSewingOut.Union(QueryRoFinishing).Distinct();
			List<string> _ro = new List<string>();
			foreach (var item in QueryRo)
			{
				_ro.Add(item);
			}
			var _unitName = (from a in garmentFinishingOutRepository.Query
							 where a.UnitId == request.unit
							 select a.UnitName).FirstOrDefault();
			CostCalculationGarmentDataProductionReport costCalculation = await GetDataCostCal(_ro, request.token);
			var sumbasicPrice = (from a in garmentPreparingRepository.Query
								 join b in garmentPreparingItemRepository.Query on a.Identity equals b.GarmentPreparingId
								 where /*(request.ro == null || (request.ro != null && request.ro != "" && a.RONo == request.ro)) &&*/
								 a.UnitId == request.unit
								 select new { a.RONo, b.BasicPrice })
						.GroupBy(x => new { x.RONo }, (key, group) => new ViewBasicPrices
						{
							RO = key.RONo,
							BasicPrice = Convert.ToDecimal(group.Sum(s => s.BasicPrice)),
							Count = group.Count()
						});
			GarmentMonitoringFinishingListViewModel listViewModel = new GarmentMonitoringFinishingListViewModel();
			List<GarmentMonitoringFinishingDto> monitoringDtos = new List<GarmentMonitoringFinishingDto>();
			var QueryFinishing = from a in (from aa in garmentFinishingOutRepository.Query
											where aa.UnitId == request.unit && aa.FinishingOutDate <= dateTo
											select aa)
								 join b in garmentFinishingOutItemRepository.Query on a.Identity equals b.FinishingOutId
								 select new monitoringView { price = Convert.ToDecimal((from aa in sumbasicPrice where aa.RO == a.RONo select aa.BasicPrice / aa.Count).FirstOrDefault()), buyerCode = (from cost in costCalculation.data where cost.ro == a.RONo select cost.buyerCode).FirstOrDefault(), finishingQtyPcs = a.FinishingOutDate >= dateFrom ? b.Quantity : 0, sewingQtyPcs = 0, uomUnit = "PCS", remainQty = 0, stock = a.FinishingOutDate < dateFrom ? -b.Quantity : 0, roJob = a.RONo, article = a.Article, qtyOrder = (from cost in costCalculation.data where cost.ro == a.RONo select cost.qtyOrder).FirstOrDefault(), style = (from cost in costCalculation.data where cost.ro == a.RONo select cost.comodityName).FirstOrDefault() };

			var QuerySewingOut = from a in (from aa in garmentSewingOutRepository.Query
											where aa.UnitToId == request.unit && aa.SewingOutDate <= dateTo && aa.SewingTo == "FINISHING"
											select aa)
								 join b in garmentSewingOutItemRepository.Query on a.Identity equals b.SewingOutId
								 select new monitoringView { price = Convert.ToDecimal((from aa in sumbasicPrice where aa.RO == a.RONo select aa.BasicPrice / aa.Count).FirstOrDefault()), buyerCode = (from cost in costCalculation.data where cost.ro == a.RONo select cost.buyerCode).FirstOrDefault(), finishingQtyPcs = 0, sewingQtyPcs = a.SewingOutDate >= dateFrom ? b.Quantity : 0, uomUnit = "PCS", remainQty = 0, stock = a.SewingOutDate < dateFrom ? b.Quantity : 0, roJob = a.RONo, article = a.Article, qtyOrder = (from cost in costCalculation.data where cost.ro == a.RONo select cost.qtyOrder).FirstOrDefault(), style = (from cost in costCalculation.data where cost.ro == a.RONo select cost.comodityName).FirstOrDefault() };

			var queryNow = QuerySewingOut.Union(QueryFinishing);
			var querySum = queryNow.ToList().GroupBy(x => new { x.buyerCode, x.qtyOrder, x.roJob, x.article, x.uomUnit, x.style }, (key, group) => new
			{
				QtyOrder = key.qtyOrder,
				RoJob = key.roJob,
				Style = key.style,
				Stock = group.Sum(s => s.stock),
				UomUnit = key.uomUnit,
				Article = key.article,
				buyer = key.buyerCode,
				price = group.Sum(s => s.price),
				SewingQtyPcs = group.Sum(s => s.sewingQtyPcs),
				Finishing = group.Sum(s => s.finishingQtyPcs)
			}).OrderBy(s => s.RoJob);
			foreach (var item in querySum)
			{
				GarmentMonitoringFinishingDto dto = new GarmentMonitoringFinishingDto
				{
					roJob = item.RoJob,
					article = item.Article,
					uomUnit = item.UomUnit,
					qtyOrder = item.QtyOrder,
					sewingOutQtyPcs = Math.Round(item.SewingQtyPcs,2),
					finishingOutQtyPcs = Math.Round(item.Finishing,2),
					stock = Math.Round(item.Stock,2),
					style = item.Style,
					buyerCode = item.buyer,
					price = Math.Round(item.price, 2),
					remainQty = Math.Round(item.Stock + item.SewingQtyPcs - item.Finishing,2),
					nominal = Math.Round(Convert.ToDecimal(item.Stock + item.SewingQtyPcs - item.Finishing) * item.price)
				};
				monitoringDtos.Add(dto);

			}
			var data = from a in monitoringDtos
					   where a.stock > 0 || a.sewingOutQtyPcs > 0 || a.finishingOutQtyPcs > 0 || a.remainQty > 0
					   select a;
			double stocks = 0;
			double finishing = 0;
			double sewingOutQtyPcs = 0;
			decimal nominals = 0;
			foreach (var item in data)
			{
				stocks += item.stock;
				finishing += item.finishingOutQtyPcs;
				sewingOutQtyPcs += item.sewingOutQtyPcs;
				nominals += item.nominal;
			}
			GarmentMonitoringFinishingDto dtos = new GarmentMonitoringFinishingDto
			{
				roJob = "",
				article = "",
				buyerCode = "",
				uomUnit = "",
				qtyOrder = 0,
				sewingOutQtyPcs = sewingOutQtyPcs,
				finishingOutQtyPcs = finishing,
				stock = stocks,
				style = "",
				price = 0,
				remainQty = stocks + sewingOutQtyPcs - finishing,
				nominal = nominals

			};
			monitoringDtos.Add(dtos);
			listViewModel.garmentMonitorings = monitoringDtos;
			var reportDataTable = new DataTable();
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "RO JOB", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Article", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kode Buyer", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Qty Order", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Style", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Harga(M)", DataType = typeof(decimal) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Stok Masuk", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Barang Awal", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Barang Keluar", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Sisa", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Nominal Sisa", DataType = typeof(decimal) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(string) });
			int counter = 5;
			if (listViewModel.garmentMonitorings.Count > 0)
			{
				foreach (var report in listViewModel.garmentMonitorings)
				{
					reportDataTable.Rows.Add(report.roJob, report.article, report.buyerCode, report.qtyOrder, report.style, report.price, report.stock, report.sewingOutQtyPcs, report.finishingOutQtyPcs, report.remainQty,report.nominal, report.uomUnit);
					counter++;
				}
			}
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
				worksheet.Cells["A" + 5 + ":L" + 5 + ""].Style.Font.Bold = true;
				worksheet.Cells["A1"].Value = "Report Finishing "; worksheet.Cells["A" + 1 + ":L" + 1 + ""].Merge = true;
				worksheet.Cells["A2"].Value = "Periode " + dateFrom.ToString("dd-MM-yyyy") + " s/d " + dateTo.ToString("dd-MM-yyyy");
				worksheet.Cells["A3"].Value = "Konfeksi " + _unitName;
				worksheet.Cells["A" + 1 + ":L" + 1 + ""].Merge = true;
				worksheet.Cells["A" + 2 + ":L" + 2 + ""].Merge = true;
				worksheet.Cells["A" + 3 + ":L" + 3 + ""].Merge = true;
				worksheet.Cells["A" + 1 + ":L" + 3 + ""].Style.Font.Size = 15;
				worksheet.Cells["A" + 1 + ":L" + 5 + ""].Style.Font.Bold = true;
				worksheet.Cells["A" + 1 + ":L" + 5 + ""].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				worksheet.Cells["A" + 1 + ":L" + 5 + ""].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				worksheet.Cells["A5"].LoadFromDataTable(reportDataTable, true);
				worksheet.Cells["D" + 2 + ":D" + counter + ""].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
				worksheet.Cells["D" + 2 + ":D" + counter + ""].Style.Numberformat.Format = "#,##0.00";
				worksheet.Cells["F" + 6 + ":k" + counter + ""].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
				worksheet.Cells["F" + 6 + ":k" + counter + ""].Style.Numberformat.Format = "#,##0.00";
				worksheet.Cells["A" + 5 + ":L" + counter + ""].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
				worksheet.Cells["A" + 5 + ":L" + counter + ""].Style.Border.Top.Style = ExcelBorderStyle.Thin;
				worksheet.Cells["A" + 5 + ":L" + counter + ""].Style.Border.Left.Style = ExcelBorderStyle.Thin;
				worksheet.Cells["A" + 5 + ":L" + counter + ""].Style.Border.Right.Style = ExcelBorderStyle.Thin;
				worksheet.Cells["F" + (counter) + ":K" + (counter) + ""].Style.Font.Bold = true;
				worksheet.Cells["A" + 1 + ":L" + 1 + ""].Style.Font.Bold = true;

				var stream = new MemoryStream();
				if (request.type != "bookkeeping")
				{
					worksheet.Cells["A" + (counter) + ":E" + (counter) + ""].Merge = true;
					worksheet.Column(3).Hidden = true;
					worksheet.Column(6).Hidden = true;
				}else
				{
					worksheet.Cells["A" + (counter) + ":F" + (counter) + ""].Merge = true;
				}
				package.SaveAs(stream);

				return stream;
			}
		}
	}
}

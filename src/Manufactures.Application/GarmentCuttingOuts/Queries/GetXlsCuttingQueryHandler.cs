using ExtCore.Data.Abstractions;
using Infrastructure.Domain.Queries;
using Infrastructure.External.DanLirisClient.Microservice;
using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using Infrastructure.External.DanLirisClient.Microservice.MasterResult;
using Manufactures.Domain.GarmentAvalComponents.Repositories;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentCuttingOuts.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using static Infrastructure.External.DanLirisClient.Microservice.MasterResult.CostCalculationGarmentDataProductionReport;
using System.Data;
using OfficeOpenXml;
using static Infrastructure.External.DanLirisClient.Microservice.MasterResult.HOrderDataProductionReport;
using OfficeOpenXml.Style;
using Manufactures.Domain.GarmentPreparings.Repositories;
using System.Net.Http;
using System.Text;

namespace Manufactures.Application.GarmentCuttingOuts.Queries
{
	public class GetXlsCuttingQueryHandler : IQueryHandler<GetXlsCuttingQuery, MemoryStream>
	{
		protected readonly IHttpClientService _http;
		private readonly IStorage _storage;
		private readonly IGarmentCuttingOutRepository garmentCuttingOutRepository;
		private readonly IGarmentCuttingOutItemRepository garmentCuttingOutItemRepository;
		private readonly IGarmentCuttingInRepository garmentCuttingInRepository;
		private readonly IGarmentCuttingInItemRepository garmentCuttingInItemRepository;
		private readonly IGarmentCuttingInDetailRepository garmentCuttingInDetailRepository;
		private readonly IGarmentAvalComponentRepository garmentAvalComponentRepository;
		private readonly IGarmentAvalComponentItemRepository garmentAvalComponentItemRepository;
		private readonly IGarmentPreparingRepository garmentPreparingRepository;
		private readonly IGarmentPreparingItemRepository garmentPreparingItemRepository;

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
            else
            {
                var err = await httpResponse.Content.ReadAsStringAsync();

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
		class ViewFC
		{
			public string RO { get; internal set; }
			public double FC { get; internal set; }
			public int Count { get; internal set; }
		}
		class ViewBasicPrices
		{
			public string RO { get; internal set; }
			public decimal BasicPrice { get; internal set; }
			public int Count { get; internal set; }
		}
		public async Task<MemoryStream> Handle(GetXlsCuttingQuery request, CancellationToken cancellationToken)
		{
			DateTimeOffset dateFrom = new DateTimeOffset(request.dateFrom, new TimeSpan(7, 0, 0));
			DateTimeOffset dateTo = new DateTimeOffset(request.dateTo, new TimeSpan(7, 0, 0));


			var QueryRoCuttingOut = (from a in garmentCuttingOutRepository.Query
									 where a.UnitId == request.unit && a.CuttingOutDate <= dateTo
									 select a.RONo).Distinct();
			var QueryRoCuttingIn = (from a in garmentCuttingInRepository.Query
									where a.UnitId == request.unit && a.CuttingInDate <= dateTo
									select a.RONo).Distinct();
			var QueryRoAvalComp = (from a in garmentAvalComponentRepository.Query
								   where a.UnitId == request.unit && a.Date <= dateTo
								   select a.RONo).Distinct();
			var QueryRo = QueryRoCuttingOut.Union(QueryRoCuttingIn).Union(QueryRoAvalComp).Distinct();
			List<string> _ro = new List<string>();
			foreach (var item in QueryRo)
			{
				_ro.Add(item);
			}
			CostCalculationGarmentDataProductionReport costCalculation = await GetDataCostCal(_ro, request.token);
			var _unitName = (from a in garmentCuttingOutRepository.Query
							 where a.UnitId == request.unit
							 select a.UnitName).FirstOrDefault();
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
			var sumFCs = (from a in garmentCuttingInRepository.Query
						  where /*(request.ro == null || (request.ro != null && request.ro != "" && a.RONo == request.ro)) && */ a.CuttingType == "Main Fabric" &&
						 a.UnitId == request.unit && a.CuttingInDate <= dateTo
						  select new { a.FC, a.RONo })
						 .GroupBy(x => new { x.RONo }, (key, group) => new ViewFC
						 {
							 RO = key.RONo,
							 FC = group.Sum(s => s.FC),
							 Count = group.Count()
						 });


			var QueryCuttingIn = from a in (from aa in garmentCuttingInRepository.Query
											where aa.UnitId == request.unit && aa.CuttingInDate <= dateTo
											select aa)
								 join b in garmentCuttingInItemRepository.Query on a.Identity equals b.CutInId
								 join c in garmentCuttingInDetailRepository.Query on b.Identity equals c.CutInItemId

								 select new monitoringView { buyerCode = (from cost in costCalculation.data where cost.ro == a.RONo select cost.buyerCode).FirstOrDefault(), price = Convert.ToDecimal((from aa in sumbasicPrice where aa.RO == a.RONo select aa.BasicPrice / aa.Count).FirstOrDefault()), fc = (from cost in sumFCs where cost.RO == a.RONo select cost.FC / cost.Count).FirstOrDefault(), cuttingQtyMeter = 0, remainQty = 0, stock = a.CuttingInDate < dateFrom ? c.CuttingInQuantity : 0, cuttingQtyPcs = a.CuttingInDate >= dateFrom ? c.CuttingInQuantity : 0, roJob = a.RONo, article = a.Article, productCode = c.ProductCode, qtyOrder = (from cost in costCalculation.data where cost.ro == a.RONo select cost.qtyOrder).FirstOrDefault(), style = (from cost in costCalculation.data where cost.ro == a.RONo select cost.comodityName).FirstOrDefault(), hours = (from cost in costCalculation.data where cost.ro == a.RONo select cost.hours).FirstOrDefault(), expenditure = 0 };

			var QueryCuttingOut = from a in (from aa in garmentCuttingOutRepository.Query
											 where aa.UnitFromId == request.unit && aa.CuttingOutDate <= dateTo
											 select aa)
								  join b in garmentCuttingOutItemRepository.Query on a.Identity equals b.CutOutId
								  select new monitoringView { buyerCode = (from cost in costCalculation.data where cost.ro == a.RONo select cost.buyerCode).FirstOrDefault(), price = Convert.ToDecimal((from aa in sumbasicPrice where aa.RO == a.RONo select aa.BasicPrice / aa.Count).FirstOrDefault()), fc = (from cost in sumFCs where cost.RO == a.RONo select cost.FC / cost.Count).FirstOrDefault(), cuttingQtyMeter = 0, remainQty = 0, stock = a.CuttingOutDate < dateFrom ? -b.TotalCuttingOut : 0, cuttingQtyPcs = 0, roJob = a.RONo, article = a.Article, productCode = b.ProductCode, qtyOrder = (from cost in costCalculation.data where cost.ro == a.RONo select cost.qtyOrder).FirstOrDefault(), style = (from cost in costCalculation.data where cost.ro == a.RONo select cost.comodityName).FirstOrDefault(), hours = (from cost in costCalculation.data where cost.ro == a.RONo select cost.hours).FirstOrDefault(), expenditure = a.CuttingOutDate >= dateFrom ? b.TotalCuttingOut : 0 };

			var QueryAvalComp = from a in (from aa in garmentAvalComponentRepository.Query
										   where aa.UnitId == request.unit && aa.Date <= dateTo
										   select aa)
								join b in garmentAvalComponentItemRepository.Query on a.Identity equals b.AvalComponentId
								select new monitoringView { buyerCode = (from cost in costCalculation.data where cost.ro == a.RONo select cost.buyerCode).FirstOrDefault(), price = Convert.ToDecimal((from aa in sumbasicPrice where aa.RO == a.RONo select aa.BasicPrice / aa.Count).FirstOrDefault()), fc = (from cost in sumFCs where cost.RO == a.RONo select cost.FC / cost.Count).FirstOrDefault(), cuttingQtyMeter = 0, remainQty = 0, stock = a.Date < dateFrom ? -b.Quantity : 0, cuttingQtyPcs = 0, roJob = a.RONo, article = a.Article, productCode = b.ProductCode, qtyOrder = (from cost in costCalculation.data where cost.ro == a.RONo select cost.qtyOrder).FirstOrDefault(), style = (from cost in costCalculation.data where cost.ro == a.RONo select cost.comodityName).FirstOrDefault(), hours = (from cost in costCalculation.data where cost.ro == a.RONo select cost.hours).FirstOrDefault(), expenditure = a.Date >= dateFrom ? b.Quantity : 0 };

			 
			var queryNow = QueryCuttingIn.Union(QueryCuttingOut).Union(QueryAvalComp);

			var querySum = queryNow.ToList().GroupBy(x => new { x.price, x.fc, x.buyerCode, x.qtyOrder, x.roJob, x.article, x.productCode, x.style, x.hours }, (key, group) => new
			{
				QtyOrder = key.qtyOrder,
				RoJob = key.roJob,
				Fc = key.fc,
				Stock = group.Sum(s => s.stock),
				ProductCode = key.productCode,
				buyer = key.buyerCode,
				bPrice = key.price,
				Article = key.article,
				Style = key.style,
				CuttingQtyPcs = group.Sum(s => s.cuttingQtyPcs),
				CuttingQtyMeter = group.Sum(s => s.cuttingQtyMeter),
				Expenditure = group.Sum(s => s.expenditure),
				Hours = key.hours
			}).OrderBy(s => s.RoJob);
			GarmentMonitoringCuttingListViewModel listViewModel = new GarmentMonitoringCuttingListViewModel();
			List<GarmentMonitoringCuttingDto> monitoringCuttingDtos = new List<GarmentMonitoringCuttingDto>();
			foreach (var item in querySum)
			{
				GarmentMonitoringCuttingDto cuttingDto = new GarmentMonitoringCuttingDto
				{
					roJob = item.RoJob,
					article = item.Article,
					productCode = item.ProductCode,
					style = item.Style,
					hours = item.Hours,
					qtyOrder = item.QtyOrder,
					cuttingQtyPcs = Math.Round(item.CuttingQtyPcs,2),
					expenditure = Math.Round(item.Expenditure,2),
					stock = Math.Round(item.Stock,2),
					remainQty = Math.Round(item.Stock + item.CuttingQtyPcs - item.Expenditure,2),
					fc = Math.Round(item.Fc, 2),
					cuttingQtyMeter = Math.Round(item.Fc * item.CuttingQtyPcs, 2),
					price = Math.Round(Convert.ToDecimal(item.bPrice)),
					buyerCode = item.buyer,
					nominal =  item.bPrice * Convert.ToDecimal( Math.Round(item.Stock + item.CuttingQtyPcs - item.Expenditure, 2))

				};
				monitoringCuttingDtos.Add(cuttingDto);
			}
			var data = from a in monitoringCuttingDtos
					   where a.stock > 0 || a.expenditure > 0 || a.cuttingQtyPcs > 0 || a.remainQty > 0
					   select a;
			double stocks = 0;
			double cuttingQtyPcs = 0;
			double expenditure = 0;
			decimal nominals = 0;
			foreach (var item in data)
			{
				stocks += item.stock;
				cuttingQtyPcs += item.cuttingQtyPcs;
				expenditure += item.expenditure;
				nominals += item.nominal;
			}
			monitoringCuttingDtos = data.ToList();
			GarmentMonitoringCuttingDto cuttingDtos = new GarmentMonitoringCuttingDto
			{
				roJob = "",
				article = "",
				productCode = "",
				style = "",
				hours = 0,
				qtyOrder = 0,
				cuttingQtyPcs = cuttingQtyPcs,
				expenditure = expenditure,
				stock = stocks,
				remainQty = stocks + cuttingQtyPcs - expenditure,
				fc = 0,
				cuttingQtyMeter = 0,
				price = 0,
				buyerCode = "",
				nominal = nominals

			};
			monitoringCuttingDtos.Add(cuttingDtos);
			listViewModel.garmentMonitorings = monitoringCuttingDtos;


			var reportDataTable = new DataTable();
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "RO JOB", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Article", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kode Buyer", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Qty Order", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Style", DataType = typeof(string) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "FC", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Hours", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Hasil Potong (M)", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Harga (M)", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Stock Awal", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Hasil Potong", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Barang Keluar", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Sisa", DataType = typeof(double) });
			reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Sisa Nominal", DataType = typeof(double) });
			int counter = 5;
			
			if (listViewModel.garmentMonitorings.Count > 0)
			{
				foreach (var report in listViewModel.garmentMonitorings)
				{
					reportDataTable.Rows.Add(report.roJob, report.article, report.productCode, report.buyerCode, report.qtyOrder, report.style,Math.Round( report.fc), report.hours, report.cuttingQtyMeter,Math.Round( report.price,2), report.stock, report.cuttingQtyPcs, report.expenditure, report.remainQty,report.nominal);
					counter++;
					
				}
			}
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
			 
				worksheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
				worksheet.Cells["E" + 2 + ":E" + counter + ""].Style.Numberformat.Format = "#,##0.00";
				worksheet.Cells["G" + 6 + ":O" + counter + ""].Style.Numberformat.Format = "#,##0.00";
				worksheet.Cells["G" + 6 + ":O" + counter + ""].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
				worksheet.Cells["A" + 5 + ":O" + counter + ""].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
				worksheet.Cells["A" + 5 + ":O" + counter + ""].Style.Border.Top.Style = ExcelBorderStyle.Thin;
				worksheet.Cells["A" + 5 + ":O" + counter + ""].Style.Border.Left.Style = ExcelBorderStyle.Thin;
				worksheet.Cells["A" + 5 + ":O" + counter + ""].Style.Border.Right.Style = ExcelBorderStyle.Thin;
				worksheet.Cells["I" + (counter) + ":O" + (counter) + ""].Style.Font.Bold = true;
				worksheet.Cells["A" + 1 + ":O" + 1 + ""].Style.Font.Bold = true;
				worksheet.Cells["A1"].Value = "Report Cutting";
				worksheet.Cells["A" + 1 + ":O" + 1 + ""].Merge = true;
				worksheet.Cells["A2"].Value = "Periode " + dateFrom.ToString("dd-MM-yyyy") + " s/d " + dateTo.ToString("dd-MM-yyyy");
				worksheet.Cells["A3"].Value = "Konfeksi " + _unitName;
				worksheet.Cells["A" + 1 + ":O" + 1 + ""].Merge = true;
				worksheet.Cells["A" + 2 + ":O" + 2 + ""].Merge = true;
				worksheet.Cells["A" + 3 + ":O" + 3 + ""].Merge = true;
				worksheet.Cells["A" + 1 + ":O" + 3 + ""].Style.Font.Size = 15;
				worksheet.Cells["A" + 1 + ":O" + 5 + ""].Style.Font.Bold = true;
				worksheet.Cells["A" + 1 + ":O" + 5 + ""].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				worksheet.Cells["A" + 1 + ":O" + 5 + ""].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				worksheet.Cells["A5"].LoadFromDataTable(reportDataTable, true);
				var stream = new MemoryStream();
				if (request.type != "bookkeeping")
				{
					worksheet.Column(4).Hidden = true;
					worksheet.Column(10).Hidden = true;
					worksheet.Cells["A" + (counter) + ":i" + (counter) + ""].Merge = true;
				}else
				{
					worksheet.Cells["A" + (counter) + ":J" + (counter) + ""].Merge = true;
				}
				package.SaveAs(stream);

				return stream;
			}
		}
		class monitoringView
		{
			public string roJob { get; set; }
			public string article { get; set; }
			public string productCode { get; set; }
			public string buyerCode { get; set; }
			public double qtyOrder { get; set; }
			public string style { get; set; }
			public double fc { get; set; }
			public double hours { get; set; }
			public double cuttingQtyMeter { get; set; }
			public double stock { get; set; }
			public double cuttingQtyPcs { get; set; }
			public double expenditure { get; set; }
			public double remainQty { get; set; }
			public decimal price { get; set; }
		}
		public GetXlsCuttingQueryHandler(IStorage storage, IServiceProvider serviceProvider)
		{
			_storage = storage;
			garmentCuttingInRepository = storage.GetRepository<IGarmentCuttingInRepository>();
			garmentCuttingInItemRepository = storage.GetRepository<IGarmentCuttingInItemRepository>();
			garmentCuttingInDetailRepository = storage.GetRepository<IGarmentCuttingInDetailRepository>();
			garmentCuttingOutRepository = storage.GetRepository<IGarmentCuttingOutRepository>();
			garmentCuttingOutItemRepository = storage.GetRepository<IGarmentCuttingOutItemRepository>();
			garmentPreparingRepository = storage.GetRepository<IGarmentPreparingRepository>();
			garmentPreparingItemRepository = storage.GetRepository<IGarmentPreparingItemRepository>();
			garmentAvalComponentRepository = storage.GetRepository<IGarmentAvalComponentRepository>();
			garmentAvalComponentItemRepository = storage.GetRepository<IGarmentAvalComponentItemRepository>();
			_http = serviceProvider.GetService<IHttpClientService>();


		}
	}
}

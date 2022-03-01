using Barebone.Tests;
using FluentAssertions;
using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using Manufactures.Application.GarmentExpenditureGoods.Queries.GetMutationExpenditureGoods;
using Manufactures.Domain.GarmentAdjustments.ReadModels;
using Manufactures.Domain.GarmentAdjustments.Repositories;
using Manufactures.Domain.GarmentCuttingIns;
using Manufactures.Domain.GarmentCuttingIns.ReadModels;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentCuttingOuts;
using Manufactures.Domain.GarmentCuttingOuts.ReadModels;
using Manufactures.Domain.GarmentCuttingOuts.Repositories;
using Manufactures.Domain.GarmentDeliveryReturns.ValueObjects;
using Manufactures.Domain.GarmentExpenditureGoodReturns;
using Manufactures.Domain.GarmentExpenditureGoodReturns.ReadModels;
using Manufactures.Domain.GarmentExpenditureGoodReturns.Repositories;
using Manufactures.Domain.GarmentExpenditureGoods.ReadModels;
using Manufactures.Domain.GarmentExpenditureGoods.Repositories;
using Manufactures.Domain.GarmentFinishingIns;
using Manufactures.Domain.GarmentFinishingIns.ReadModels;
using Manufactures.Domain.GarmentFinishingIns.Repositories;
using Manufactures.Domain.GarmentFinishingOuts;
using Manufactures.Domain.GarmentFinishingOuts.ReadModels;
using Manufactures.Domain.GarmentFinishingOuts.Repositories;
using Manufactures.Domain.GarmentPreparings;
using Manufactures.Domain.GarmentPreparings.ReadModels;
using Manufactures.Domain.GarmentPreparings.Repositories;
using Manufactures.Domain.GarmentSewingOuts.Repositories;
using Manufactures.Domain.MonitoringProductionStockFlow;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Manufactures.Tests.Queries.GarmentExpenditureGoods.GarmentReceiptFinishedGood
{
    public class XlsReceiptFinishedGoodQueryHandlerTest : BaseCommandUnitTest
    {
        private readonly Mock<IGarmentBalanceMonitoringProductionStockFlowRepository> _mockgarmentBalanceMonitoringProductionStockFlowRepository;
        private readonly Mock<IGarmentAdjustmentRepository> _mockgarmentAdjustmentRepository;
        private readonly Mock<IGarmentAdjustmentItemRepository> _mockgarmentAdjustmentItemRepository;
        private readonly Mock<IGarmentExpenditureGoodRepository> _mockgarmentExpenditureGoodRepository;
        private readonly Mock<IGarmentExpenditureGoodItemRepository> _mockgarmentExpenditureGoodItemRepository;
        private readonly Mock<IGarmentExpenditureGoodReturnRepository> _mockgarmentExpenditureGoodReturnRepository;
        private readonly Mock<IGarmentExpenditureGoodReturnItemRepository> _mockgarmentExpenditureGoodReturnItemRepository;
        private readonly Mock<IGarmentFinishingOutRepository> _mockgarmentFinishingOutRepository;
        private readonly Mock<IGarmentFinishingOutItemRepository> _mockgarmentFinishingOutItemRepository;
        private readonly Mock<IGarmentFinishingInRepository> _mockgarmentFinishingInRepository;
        private readonly Mock<IGarmentFinishingInItemRepository> _mockgarmentFinishingInItemRepository;
        private readonly Mock<IGarmentCuttingOutRepository> _mockgarmentCuttingOutRepository;
        private readonly Mock<IGarmentCuttingOutItemRepository> _mockgarmentCuttingOutItemRepository;
        private readonly Mock<IGarmentCuttingOutDetailRepository> _mockgarmentCuttingOutDetailRepository;
        private readonly Mock<IGarmentCuttingInRepository> _mockgarmentCuttingInRepository;
        private readonly Mock<IGarmentCuttingInItemRepository> _mockgarmentCuttingInItemRepository;
        private readonly Mock<IGarmentCuttingInDetailRepository> _mockgarmentCuttingInDetailRepository;
        private readonly Mock<IGarmentPreparingRepository> _mockgarmentPreparingRepository;
        private readonly Mock<IGarmentPreparingItemRepository> _mockgarmentPreparingItemRepository;
        private Mock<IServiceProvider> serviceProviderMock;

        public XlsReceiptFinishedGoodQueryHandlerTest()
        {
            _mockgarmentBalanceMonitoringProductionStockFlowRepository = CreateMock<IGarmentBalanceMonitoringProductionStockFlowRepository>();
            _MockStorage.SetupStorage(_mockgarmentBalanceMonitoringProductionStockFlowRepository);

            _mockgarmentAdjustmentRepository = CreateMock<IGarmentAdjustmentRepository>();
            _mockgarmentAdjustmentItemRepository = CreateMock<IGarmentAdjustmentItemRepository>();
            _MockStorage.SetupStorage(_mockgarmentAdjustmentRepository);
            _MockStorage.SetupStorage(_mockgarmentAdjustmentItemRepository);

            _mockgarmentExpenditureGoodRepository = CreateMock<IGarmentExpenditureGoodRepository>();
            _mockgarmentExpenditureGoodItemRepository = CreateMock<IGarmentExpenditureGoodItemRepository>();
            _MockStorage.SetupStorage(_mockgarmentExpenditureGoodRepository);
            _MockStorage.SetupStorage(_mockgarmentExpenditureGoodItemRepository);

            _mockgarmentExpenditureGoodReturnRepository = CreateMock<IGarmentExpenditureGoodReturnRepository>();
            _mockgarmentExpenditureGoodReturnItemRepository = CreateMock<IGarmentExpenditureGoodReturnItemRepository>();
            _MockStorage.SetupStorage(_mockgarmentExpenditureGoodReturnRepository);
            _MockStorage.SetupStorage(_mockgarmentExpenditureGoodReturnItemRepository);

            _mockgarmentFinishingOutRepository = CreateMock<IGarmentFinishingOutRepository>();
            _mockgarmentFinishingOutItemRepository = CreateMock<IGarmentFinishingOutItemRepository>();
            _MockStorage.SetupStorage(_mockgarmentFinishingOutRepository);
            _MockStorage.SetupStorage(_mockgarmentFinishingOutItemRepository);

            _mockgarmentFinishingInRepository = CreateMock<IGarmentFinishingInRepository>();
            _mockgarmentFinishingInItemRepository = CreateMock<IGarmentFinishingInItemRepository>();
            _MockStorage.SetupStorage(_mockgarmentFinishingInRepository);
            _MockStorage.SetupStorage(_mockgarmentFinishingInItemRepository);

            _mockgarmentCuttingOutRepository = CreateMock<IGarmentCuttingOutRepository>();
            _mockgarmentCuttingOutItemRepository = CreateMock<IGarmentCuttingOutItemRepository>();
            _mockgarmentCuttingOutDetailRepository = CreateMock<IGarmentCuttingOutDetailRepository>();
            _MockStorage.SetupStorage(_mockgarmentCuttingOutRepository);
            _MockStorage.SetupStorage(_mockgarmentCuttingOutItemRepository);
            _MockStorage.SetupStorage(_mockgarmentCuttingOutDetailRepository);

            _mockgarmentCuttingInRepository = CreateMock<IGarmentCuttingInRepository>();
            _mockgarmentCuttingInItemRepository = CreateMock<IGarmentCuttingInItemRepository>();
            _mockgarmentCuttingInDetailRepository = CreateMock<IGarmentCuttingInDetailRepository>();
            _MockStorage.SetupStorage(_mockgarmentCuttingInRepository);
            _MockStorage.SetupStorage(_mockgarmentCuttingInItemRepository);
            _MockStorage.SetupStorage(_mockgarmentCuttingInDetailRepository);

            _mockgarmentPreparingRepository = CreateMock<IGarmentPreparingRepository>();
            _mockgarmentPreparingItemRepository = CreateMock<IGarmentPreparingItemRepository>();
            _MockStorage.SetupStorage(_mockgarmentPreparingRepository);
            _MockStorage.SetupStorage(_mockgarmentPreparingItemRepository);

            serviceProviderMock = new Mock<IServiceProvider>();
        }

        private GetXlsReceiptFinishedGoodsQueryHandler CreateGetXlsReceiptQueryHandler()
        {
            return new GetXlsReceiptFinishedGoodsQueryHandler(_MockStorage.Object, serviceProviderMock.Object);
        }

        [Fact]
        public async Task Handle_StateUnderTest_ExpectedBehavior()
        {
            GetXlsReceiptFinishedGoodsQueryHandler unitUnderTest = CreateGetXlsReceiptQueryHandler();
            CancellationToken cancellationToken = CancellationToken.None;

            Guid guidAdjustment = Guid.NewGuid();
            Guid guidAdjustmentItem = Guid.NewGuid();
            Guid guidExpenditureGood = Guid.NewGuid();
            Guid guidExpenditureGoodItem = Guid.NewGuid();
            Guid guidExpenditureGoodReturn = Guid.NewGuid();
            Guid guidExpenditureGoodReturnItem = Guid.NewGuid();
            Guid guidFinishingOut = Guid.NewGuid();
            Guid guidFinishingOutItem = Guid.NewGuid();
            Guid guidFinishingIn = Guid.NewGuid();
            Guid guidFinishingInItem = Guid.NewGuid();
            Guid guidSewingOutDetail = Guid.NewGuid();
            Guid guidSewingOutItem = Guid.NewGuid();
            Guid guidSewingOut = Guid.NewGuid();
            Guid guidCuttingOut = Guid.NewGuid();
            Guid guidCuttingOutItem = Guid.NewGuid();
            Guid guidCuttingOutDetail = Guid.NewGuid();
            Guid guidCuttingIn = Guid.NewGuid();
            Guid guidCuttingInItem = Guid.NewGuid();
            Guid guidCuttingInDetail = Guid.NewGuid();
            Guid guidPreparing = Guid.NewGuid();
            Guid guidPreparingItem = Guid.NewGuid();
            Guid guidbalance = Guid.NewGuid();

            GetXlsReceiptFinishedGoodsQuery getXlsReceipt = new GetXlsReceiptFinishedGoodsQuery(1, 25, "{}", DateTime.Now, DateTime.Now.AddDays(5), "token");

            _mockgarmentBalanceMonitoringProductionStockFlowRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentBalanceMonitoringProductionStockReadModel> {
                    new GarmentBalanceMonitoringProductionStocFlow(new GarmentBalanceMonitoringProductionStockReadModel(Guid.NewGuid())).GetReadModel()
                }.AsQueryable());

            _mockgarmentAdjustmentItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentAdjustmentItemReadModel>
                {
                    new Domain.GarmentAdjustments.GarmentAdjustmentItem(guidAdjustmentItem, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Domain.Shared.ValueObjects.SizeId(1), "", new Domain.Shared.ValueObjects.ProductId(1), "","","customsCategory","",0,0,new Domain.Shared.ValueObjects.UomId(1),"","",0).GetReadModel()
                }.AsQueryable());

            _mockgarmentAdjustmentRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentAdjustmentReadModel>
                {
                    new Domain.GarmentAdjustments.GarmentAdjustment(guidAdjustment,"","","","",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","",DateTimeOffset.Now,new Domain.Shared.ValueObjects.GarmentComodityId(1),"","","").GetReadModel()
                }.AsQueryable());

            _mockgarmentExpenditureGoodItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentExpenditureGoodItemReadModel>
                {
                    new Domain.GarmentExpenditureGoods.GarmentExpenditureGoodItem(guidExpenditureGoodItem,guidExpenditureGood,Guid.NewGuid(),"customsCategory",new Domain.Shared.ValueObjects.SizeId(1),"",1,0,new Domain.Shared.ValueObjects.UomId(1),"","",0,0).GetReadModel()
                }.AsQueryable());

            _mockgarmentExpenditureGoodRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentExpenditureGoodReadModel>
                {
                    new Domain.GarmentExpenditureGoods.GarmentExpenditureGood(guidExpenditureGood, "","",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","","213","",new Domain.Shared.ValueObjects.GarmentComodityId(1),"BR","",new Domain.Shared.ValueObjects.BuyerId(1),"","",DateTimeOffset.Now,"","",0,"",false,0).GetReadModel()
                }.AsQueryable());

            _mockgarmentExpenditureGoodReturnItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentExpenditureGoodReturnItemReadModel>
                {
                    new GarmentExpenditureGoodReturnItem(new Guid(),guidExpenditureGoodReturnItem,guidExpenditureGood,new Guid(),new Guid(),"customsCategory", new Domain.Shared.ValueObjects.SizeId(1),"",100,new Domain.Shared.ValueObjects.UomId(1),"","",100,100).GetReadModel()
                }.AsQueryable());

            _mockgarmentExpenditureGoodReturnRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentExpenditureGoodReturnReadModel>
                {
                    new GarmentExpenditureGoodReturn(guidExpenditureGoodReturn,"np","SAMPLE",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","","ro","article",new Domain.Shared.ValueObjects.GarmentComodityId(1),"","",new Domain.Shared.ValueObjects.BuyerId(1),"","",DateTimeOffset.Now,"","").GetReadModel()
                }.AsQueryable());

            _mockgarmentFinishingOutItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentFinishingOutItemReadModel>
                {
                    new GarmentFinishingOutItem(guidFinishingOutItem,guidFinishingOut,new Guid(),new Guid(),new Domain.Shared.ValueObjects.ProductId(1),"","","customsCategory","",new Domain.Shared.ValueObjects.SizeId(1),"",10, new Domain.Shared.ValueObjects.UomId(1),"","",10,10,10).GetReadModel()
                }.AsQueryable());

            _mockgarmentFinishingOutRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentFinishingOutReadModel>
                {
                    new GarmentFinishingOut(guidFinishingOut,"",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","","GUDANG JADI",DateTimeOffset.Now,"ro","article",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","",new Domain.Shared.ValueObjects.GarmentComodityId(1),"","",false).GetReadModel()
                }.AsQueryable());

            _mockgarmentFinishingInItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentFinishingInItemReadModel>
                {
                    new GarmentFinishingInItem(guidFinishingInItem, guidFinishingIn,new Guid(),new Guid(),new Guid(),new Domain.Shared.ValueObjects.SizeId(1),"",new Domain.Shared.ValueObjects.ProductId(1),"","","customsCategory","",10,10, new Domain.Shared.ValueObjects.UomId(1),"","",10,10).GetReadModel()
                }.AsQueryable());

            _mockgarmentFinishingInRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentFinishingInReadModel>
                {
                    new GarmentFinishingIn(guidFinishingIn, "", "SEWING",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","","ro","article",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","",DateTimeOffset.Now,new Domain.Shared.ValueObjects.GarmentComodityId(1),"","",0,"").GetReadModel()
                }.AsQueryable());

            _mockgarmentCuttingOutDetailRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentCuttingOutDetailReadModel>
                {
                    new GarmentCuttingOutDetail(guidCuttingOutDetail, guidCuttingOutItem,new Domain.Shared.ValueObjects.SizeId(1),"","",10,10,new Domain.Shared.ValueObjects.UomId(1),"",10,10).GetReadModel()
                }.AsQueryable());

            _mockgarmentCuttingOutItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentCuttingOutItemReadModel>
                {
                    new GarmentCuttingOutItem(guidCuttingOutItem, guidCuttingIn, guidCuttingInDetail, guidCuttingOut, new Domain.Shared.ValueObjects.ProductId(1),"","","","",10).GetReadModel()
                }.AsQueryable());

            _mockgarmentCuttingOutRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentCuttingOutReadModel>
                {
                     new GarmentCuttingOut(guidCuttingOut, "", "SEWING",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","",DateTime.Now,"ro","article",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","",new Domain.Shared.ValueObjects.GarmentComodityId(1),"cm","cmo",false).GetReadModel()
                }.AsQueryable());

            _mockgarmentCuttingInDetailRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentCuttingInDetailReadModel>
                {
                    new GarmentCuttingInDetail(guidCuttingInDetail,guidCuttingInItem, guidPreparingItem, guidSewingOutItem, guidSewingOutDetail,new Domain.Shared.ValueObjects.ProductId(1),"","","","","",10,new Domain.Shared.ValueObjects.UomId(1),"",10,new Domain.Shared.ValueObjects.UomId(1),"",10,10,10,2.0,"").GetReadModel()
                }.AsQueryable());

            _mockgarmentCuttingInItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentCuttingInItemReadModel>
                {
                    new GarmentCuttingInItem(guidCuttingInItem, guidCuttingIn, guidPreparing, 1, "", guidSewingOut, "").GetReadModel()
                }.AsQueryable());

            _mockgarmentCuttingInRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentCuttingInReadModel>
                {
                    new GarmentCuttingIn(guidCuttingIn,"","Main Fabric","PREPARING","ro","article",new Domain.Shared.ValueObjects.UnitDepartmentId(1),"","",DateTimeOffset.Now,2.0).GetReadModel()
                }.AsQueryable());

            _mockgarmentPreparingItemRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentPreparingItemReadModel>
                {
                    new GarmentPreparingItem(guidPreparingItem,0,new Domain.GarmentPreparings.ValueObjects.ProductId(1),"","","",10,new Domain.GarmentPreparings.ValueObjects.UomId(1),"","Main Fabric",10,10,guidPreparing,"ro","IMPORT FASILITAS").GetReadModel()
                }.AsQueryable());

            _mockgarmentPreparingRepository
                .Setup(s => s.Query)
                .Returns(new List<GarmentPreparingReadModel>
                {
                    new GarmentPreparing(guidPreparing,0,"",new Domain.GarmentPreparings.ValueObjects.UnitDepartmentId(1),"","",DateTimeOffset.Now,"ro","article",true,new Domain.Shared.ValueObjects.BuyerId(1),"","").GetReadModel()
                }.AsQueryable());

            var result = await unitUnderTest.Handle(getXlsReceipt, cancellationToken);

            // Assert
            result.Should().NotBeNull();
        }
    }
}

using ExtCore.Data.Abstractions;
using Infrastructure.Domain.Commands;
using Manufactures.Domain.GarmentCuttingIns;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentSewingIns;
using Manufactures.Domain.GarmentSewingIns.Repositories;
using Manufactures.Domain.GarmentSewingOuts;
using Manufactures.Domain.GarmentSewingOuts.Commands;
using Manufactures.Domain.GarmentSewingOuts.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Manufactures.Application.GarmentSewingOuts.CommandHandlers
{
    public class RemoveGarmentSewingOutCommandHandler : ICommandHandler<RemoveGarmentSewingOutCommand, GarmentSewingOut>
    {
        private readonly IStorage _storage;
        private readonly IGarmentSewingOutRepository _garmentSewingOutRepository;
        private readonly IGarmentSewingOutItemRepository _garmentSewingOutItemRepository;
        private readonly IGarmentSewingOutDetailRepository _garmentSewingOutDetailRepository;
        private readonly IGarmentSewingInItemRepository _garmentSewingInItemRepository;
        private readonly IGarmentCuttingInRepository _garmentCuttingInRepository;
        private readonly IGarmentCuttingInItemRepository _garmentCuttingInItemRepository;
        private readonly IGarmentCuttingInDetailRepository _garmentCuttingInDetailRepository;

        public RemoveGarmentSewingOutCommandHandler(IStorage storage)
        {
            _storage = storage;
            _garmentSewingOutRepository = storage.GetRepository<IGarmentSewingOutRepository>();
            _garmentSewingOutItemRepository = storage.GetRepository<IGarmentSewingOutItemRepository>();
            _garmentSewingOutDetailRepository = storage.GetRepository<IGarmentSewingOutDetailRepository>();
            _garmentSewingInItemRepository = storage.GetRepository<IGarmentSewingInItemRepository>();
            _garmentCuttingInRepository = storage.GetRepository<IGarmentCuttingInRepository>();
            _garmentCuttingInItemRepository = storage.GetRepository<IGarmentCuttingInItemRepository>();
            _garmentCuttingInDetailRepository = storage.GetRepository<IGarmentCuttingInDetailRepository>();
        }

        public async Task<GarmentSewingOut> Handle(RemoveGarmentSewingOutCommand request, CancellationToken cancellationToken)
        {
            var sewOut = _garmentSewingOutRepository.Query.Where(o => o.Identity == request.Identity).Select(o => new GarmentSewingOut(o)).Single();

            if (sewOut.SewingTo == "CUTTING")
            {
                Guid cutInId = _garmentCuttingInItemRepository.Query.Where(a => a.SewingOutId == sewOut.Identity).First().CutInId;

                var cutIn = _garmentCuttingInRepository.Query.Where(a => a.Identity == cutInId).Select(a => new GarmentCuttingIn(a)).Single();

                _garmentCuttingInItemRepository.Find(o => o.CutInId == cutIn.Identity).ForEach(async cutInItem =>
                {
                    _garmentCuttingInDetailRepository.Find(o => o.CutInItemId == cutInItem.Identity).ForEach(async cutInDetail =>
                    {

                        cutInDetail.Remove();
                        await _garmentCuttingInDetailRepository.Update(cutInDetail);
                    });

                    cutInItem.Remove();
                    await _garmentCuttingInItemRepository.Update(cutInItem);
                });

                cutIn.Remove();
                await _garmentCuttingInRepository.Update(cutIn);
            }

            Dictionary<Guid, double> sewInItemToBeUpdated = new Dictionary<Guid, double>();

            _garmentSewingOutItemRepository.Find(o => o.SewingOutId == sewOut.Identity).ForEach(async sewOutItem =>
            {
                if (sewOut.IsDifferentSize)
                {
                    _garmentSewingOutDetailRepository.Find(o => o.SewingOutItemId == sewOutItem.Identity).ForEach(async sewOutDetail =>
                    {
                        if (sewInItemToBeUpdated.ContainsKey(sewOutItem.SewingInItemId))
                        {
                            sewInItemToBeUpdated[sewOutItem.SewingInItemId] += sewOutDetail.Quantity;
                        }
                        else
                        {
                            sewInItemToBeUpdated.Add(sewOutItem.SewingInItemId, sewOutDetail.Quantity);
                        }

                        sewOutDetail.Remove();
                        await _garmentSewingOutDetailRepository.Update(sewOutDetail);
                    });
                }
                else
                {
                    if (sewInItemToBeUpdated.ContainsKey(sewOutItem.SewingInItemId))
                    {
                        sewInItemToBeUpdated[sewOutItem.SewingInItemId] += sewOutItem.Quantity;
                    }
                    else
                    {
                        sewInItemToBeUpdated.Add(sewOutItem.SewingInItemId, sewOutItem.Quantity);
                    }
                }


                sewOutItem.Remove();
                await _garmentSewingOutItemRepository.Update(sewOutItem);
            });

            foreach (var sewingInItem in sewInItemToBeUpdated)
            {
                var garmentSewInItem = _garmentSewingInItemRepository.Query.Where(x => x.Identity == sewingInItem.Key).Select(s => new GarmentSewingInItem(s)).Single();
                garmentSewInItem.SetRemainingQuantity(garmentSewInItem.RemainingQuantity + sewingInItem.Value);
                garmentSewInItem.Modify();
                await _garmentSewingInItemRepository.Update(garmentSewInItem);
            }

            sewOut.Remove();
            await _garmentSewingOutRepository.Update(sewOut);

            _storage.Save();

            return sewOut;
        }
    }
}

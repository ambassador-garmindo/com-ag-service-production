using System;
using System.Collections.Generic;
using System.Text;

namespace Manufactures.Application.GarmentSewingOuts.Queries.GetGarmentSewingOutsDynamic
{
    public class GarmentSewingOutsDynamicViewModel
    {
        public int count { get; private set; }
        public List<dynamic> data { get; private set; }

        public GarmentSewingOutsDynamicViewModel(int count, List<dynamic> data)
        {
            this.count = count;
            this.data = data;
        }
    }
}

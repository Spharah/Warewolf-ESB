﻿using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Interfaces;

namespace Dev2.Studio
{
    public class BaseActivityDesigner : ActivityDesigner
    {
        private const Int32 MinSize = 2;
        private const Int32 MinBlanks = 1;


        private IList<ModelItem> ItemList
        {
            get
            {
                //use dynamics to get strongly typed list of items
                return ((dynamic)ModelItem).FieldsCollection as ModelItemCollection;
            }
        }

        private IEnumerable<int> BlankIndexes
        {
            get
            {
                var blankList = (from ModelItem dto in ItemList
                                 let currentVal = dto.GetCurrentValue() as IDev2TOFn
                                 where currentVal != null
                                 where currentVal.CanRemove()
                                 select currentVal.IndexNumber).ToList();
                return blankList;
            }
        }

        public void RemoveRow()
        {
            //do nothing if smaller or equal than 2 (which is minimum size)
            if (ItemList == null || ItemList.Count() <= MinSize ||  
                 //never remove the last blank item
                BlankIndexes == null || BlankIndexes.Count() <= MinBlanks)
            {
                return;
            }

            //remove all the other blank items
            var firstIdxToRemove = BlankIndexes.First() - 1;
            ItemList.RemoveAt(firstIdxToRemove);
            for (var i = firstIdxToRemove; i < ItemList.Count; i++)
            {
                dynamic tmp = ItemList[i];
                tmp.IndexNumber = i + 1;
            }
        }
    }
}

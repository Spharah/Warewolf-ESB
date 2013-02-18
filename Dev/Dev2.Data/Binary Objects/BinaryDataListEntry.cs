﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects.Structs;

namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    internal class BinaryDataListEntry : IBinaryDataListEntry
    {
        #region Fields

        readonly IDictionary<string, int> _strToColIdx = new Dictionary<string, int>(GlobalConstants.DefaultColumnSizeLvl1);
        SBinaryDataListEntry _internalObj;

        #endregion Fields

        #region Properties

        public Guid DataListKey
        {
            get
            {
                return _internalObj.DataListKey;
            }
            private set
            {
                _internalObj.DataListKey = value;
            }
        }

        public bool IsEditable
        {
            get
            {
                return _internalObj.IsEditable;
            }
            private set
            {
                _internalObj.IsEditable = value;
            }
        }

        public enDev2ColumnArgumentDirection ColumnIODirection
        {
            get
            {
                return _internalObj.ColumnIODirection;
            }
            private set
            {
                _internalObj.ColumnIODirection = value;
            }
        }

        public bool IsRecordset
        {
            get
            {
                return _internalObj.IsRecordset;
            }
            private set
            {
                _internalObj.IsRecordset = value;
            }
        }

        public bool IsEvaluationScalar
        {
            get
            {
                return _internalObj.IsEvaluationScalar;
            }
            private set
            {
                _internalObj.IsEvaluationScalar = value;
            }
        }

        public bool IsManagmentServicePayload
        {
            get
            {
                return _internalObj.IsManagmentServicePayload;
            }
            private set
            {
                _internalObj.IsManagmentServicePayload = value;
            }
        }

        public IList<Dev2Column> Columns
        {
            get
            {
                return _internalObj.Columns;
            }
            private set
            {
                _internalObj.Columns = value;
            }
        }

        public string Namespace
        {
            get
            {
                return _internalObj.Namespace;
            }
            private set
            {
                _internalObj.Namespace = value;
            }
        }

        public string Description
        {
            get
            {
                return _internalObj.Description;
            }
            private set
            {
                _internalObj.Description = value;
            }
        }

        #endregion Properties

        #region Ctors

        internal BinaryDataListEntry(string nameSpace, string description, IList<Dev2Column> cols)
            : this(nameSpace, description, cols, true, enDev2ColumnArgumentDirection.None, Guid.NewGuid())
        {
        }

       BinaryDataListEntry(string nameSpace, string description, IList<Dev2Column> cols, Guid dataListKey)
            : this(nameSpace, description, cols, true, enDev2ColumnArgumentDirection.None, dataListKey)
        {
        }

        internal BinaryDataListEntry(string nameSpace, string description)
            : this(nameSpace, description, true, enDev2ColumnArgumentDirection.None, Guid.NewGuid())
        {
        }

        internal BinaryDataListEntry(string nameSpace, string description, Guid dataListKey)
            : this(nameSpace, description, true, enDev2ColumnArgumentDirection.None, dataListKey)
        {
        }

        internal BinaryDataListEntry(string nameSpace, string description, IList<Dev2Column> cols, bool isEditable, enDev2ColumnArgumentDirection ioDir, Guid dataListKey)
        {
            IsRecordset = true;
            Columns = cols;
            Namespace = nameSpace;
            Description = description;
            IsEditable = isEditable;
            ColumnIODirection = ioDir;
            DataListKey = Guid.NewGuid();
            _internalObj._appendIndex = -1;
            _internalObj.Init();
        }

        internal BinaryDataListEntry(string nameSpace, string description, bool isEditable, enDev2ColumnArgumentDirection ioDir, Guid dataListKey)
        {
            Namespace = nameSpace;
            Description = description;
            IsEditable = isEditable;
            ColumnIODirection = ioDir;
            DataListKey = Guid.NewGuid();
            _internalObj._appendIndex = -1;
            _internalObj.Init();
        }

        #endregion Ctors

        #region Methods

        /// <summary>
        ///     Fetch the number of records present
        /// </summary>
        /// <returns></returns>
        public int ItemCollectionSize()
        {
            return _internalObj.Count;
        }

        /// <summary>
        ///     Fetches the record at.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public IList<IBinaryDataListItem> FetchRecordAt(int idx, out string error)
        {
            return FetchRecordAt(idx, null, out error);
        }

        public void TryPutRecordItemAtIndex(IBinaryDataListItem item, int idx, out string error)
        {
            error = "Is not recordset";
            int myIdx = idx;

            if(IsRecordset)
            {
                IList<IBinaryDataListItem> dummy;

                if(idx <= FetchLastRecordsetIndex() && _internalObj.TryGetValue(myIdx, out dummy) && dummy.Any())
                {
                    int colIdx = InternalFetchColumnIndex(item.FieldName);
                    if(colIdx >= 0)
                    {
                        // entry already exist, so remove it
                        var binaryDataListItems = _internalObj[myIdx];
                        binaryDataListItems.RemoveAt(colIdx);
                        binaryDataListItems.Insert(colIdx, item);
                        _internalObj[myIdx] = binaryDataListItems;
                        error = string.Empty;
                        _internalObj.IsEmtpy = false;
                    }
                    else
                    {
                        error = "Mapping error: Column not Found" + item.FieldName;
                    }
                }
                else
                {
                    int colIdx = InternalFetchColumnIndex(item.FieldName);
                    if(colIdx >= 0)
                    {
                        string ns = item.Namespace == string.Empty ? Namespace : item.Namespace;
                        IList<IBinaryDataListItem> cols = new List<IBinaryDataListItem>(Columns.Count);
                        var theCols = Columns.ToArray();
                        var binaryDataListItem = new BinaryDataListItem(item.TheValue, ns, item.FieldName, myIdx);
                        for(int i = 0; i < theCols.Length; i++)
                        {
                            IBinaryDataListItem newItem;

                            if(i != colIdx)
                            {
                                //we also need to check for existing data and act differently
                                var myItem = binaryDataListItem.Clone();
                                myItem.UpdateValue(string.Empty);
                                myItem.UpdateField(theCols[i].ColumnName);
                                newItem = myItem;
                            }
                            else
                            {
                                // We need to account for resetting the index vs what was sent in :(
                                binaryDataListItem.UpdateValue(item.TheValue);
                                binaryDataListItem.UpdateField(item.FieldName);
                                newItem = binaryDataListItem;
                            }
                            cols.Add(newItem);
                        }

                        _internalObj[myIdx] = cols;
                        _internalObj.IsEmtpy = false;
                        error = string.Empty;
                    }
                    else
                    {
                        error = "Mapping error: Column not found " + item.FieldName;
                    }
                }
            }
        }

        public void TryAppendRecordItem(IBinaryDataListItem item, out string error)
        {
            error = "Is not recordset";
            if(IsRecordset)
            {
                Dev2Column colToFind = Columns.FirstOrDefault(c => c.ColumnName == item.FieldName);
                IList<IBinaryDataListItem> cols = new List<IBinaryDataListItem>(Columns.Count);
                if(colToFind != null)
                {
                    int colIdx = Columns.IndexOf(colToFind);
                    cols.Insert(colIdx, item);
                    _internalObj.Add(ItemCollectionSize(), cols);
                    error = string.Empty;
                    _internalObj.IsEmtpy = false;
                }
                else
                {
                    error = "Mapping error: Column not found " + item.FieldName;
                }
            }
        }

        public IBinaryDataListItem FetchScalar()
        {
            IBinaryDataListItem result = null;
            if(IsRecordset)
            {
                string error;
                return TryFetchLastIndexedRecordsetUpsertPayload(out error);
            }
            else if(_internalObj.Count > 0)
            {
                var binaryDataListItems = _internalObj[0];
                if(binaryDataListItems.Count > 0)
                {
                    result = binaryDataListItems[0];
                }
            }

            if(result == null)
            {
                result = new BinaryDataListItem(string.Empty, Namespace);
                // place miss into the collection
                _internalObj[0] = new List<IBinaryDataListItem> { result };
            }

            return result;
        }

        public void TryPutScalar(IBinaryDataListItem item, out string error)
        {
            error = "Not a scalar";
            if(!IsRecordset)
            {
                // set evaluation scalar
                if(item.FieldName == GlobalConstants.EvalautionScalar)
                {
                    IsEvaluationScalar = true;
                }
                else
                {
                    IsEvaluationScalar = false;
                }

                // set managment service payload
                if(item.FieldName == GlobalConstants.ManagementServicePayload)
                {
                    IsManagmentServicePayload = true;
                }
                else
                {
                    IsManagmentServicePayload = false;
                }

                IList<IBinaryDataListItem> value;

                if(_internalObj.TryGetValue(0, out value))
                {
                    //                    if (value.Count > 0)
                    //                    {
                    //                        _internalObj.Remove(0);    
                    //                    }
                    if(value.Count > 0)
                    {
                        value.RemoveAt(0);
                    }
                    value.Add(item);
                    _internalObj[0] = value;
                }
                else
                {
                    value = new List<IBinaryDataListItem>();
                    value.Add(item);
                    _internalObj[0] = value;
                }
                error = string.Empty;
            }
        }

        public IBinaryDataListEntry Clone(enTranslationDepth depth, out string error)
        {
            error = string.Empty;
            BinaryDataListEntry result;
            if(Columns != null)
            {
                // clone the columns
                IList<Dev2Column> cols = new List<Dev2Column>(Columns.Count);
                foreach(Dev2Column c in Columns)
                {
                    cols.Add(new Dev2Column(c.ColumnName, c.ColumnDescription));
                }
                result = new BinaryDataListEntry(Namespace, Description, cols, DataListKey);
            }
            else
            {
                result = new BinaryDataListEntry(Namespace, Description, DataListKey);
            }

            if(depth == enTranslationDepth.Data || depth == enTranslationDepth.Data_With_Blank_OverWrite)
            {
                // clone _items

                if(IsRecordset)
                {
                    IIndexIterator ii = _internalObj.Keys;
                    bool isEmtpy = _internalObj.IsEmtpy;
                    result._internalObj.IsEmtpy = isEmtpy;
                    while(ii.HasMore())
                    {
                        int next = ii.FetchNextIndex();
                        // clone the data

                        // Travis.Frisinger - Only do this if there is data present, else leave it as it is

                        // TODO : Internal Object will not have data, we just need to expose this fact!

                        if(!isEmtpy)
                        {
                            IList<IBinaryDataListItem> items = _internalObj[next];
                            IList<IBinaryDataListItem> clone = new List<IBinaryDataListItem>();
                            // Bug 8725
                            if(items != null)
                            {
                                foreach(IBinaryDataListItem itm in items)
                                {
                                    clone.Add(itm.Clone());
                                }
                            }

                            // now push back clone
                            result._internalObj[next] = clone;
                        }
                    }
                }
                else
                {
                    IList<IBinaryDataListItem> items = _internalObj[0];
                    IList<IBinaryDataListItem> clone = new List<IBinaryDataListItem>();
                    foreach(IBinaryDataListItem itm in items)
                    {
                        clone.Add(itm.Clone());
                    }

                    // now push back clone
                    result._internalObj[0] = clone;
                }
            }
            else // only wanted the shape cloned
            {
                // clone _items
                IList<IBinaryDataListItem> blankItems = new List<IBinaryDataListItem>();
                if(_internalObj.Count > 0)
                {
                    if(IsRecordset)
                    {
                        //int firstKey = _internalObj.Keys.First();
                        int firstKey = _internalObj.Keys.MinIndex();
                        int listLen = _internalObj[firstKey].Count;
                        for(int i = 0; i < listLen; i++)
                        {
                            blankItems.Add(Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, Namespace, Columns[i].ColumnName, (i + 1)));
                        }

                        result._internalObj[firstKey] = blankItems;
                    }
                    else
                    {
                        IBinaryDataListItem binaryDataListItem = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, Namespace);
                        blankItems.Add(binaryDataListItem);
                        result._internalObj[0] = blankItems;
                    }
                }
            }

            return result;
        }

        public void Merge(IBinaryDataListEntry toMerge, out string error)
        {
            error = string.Empty;
            if(IsRecordset && toMerge.IsRecordset)
            {
                IIndexIterator ii = toMerge.FetchRecordsetIndexes();
                while(ii.HasMore())
                {
                    int next = ii.FetchNextIndex();
                    // merge toMerge into this
                    foreach(IBinaryDataListItem item in toMerge.FetchRecordAt(next, out error))
                    {
                        TryAppendRecordItem(item, out error);
                    }
                }
            }
            else if(!IsRecordset && !toMerge.IsRecordset)
            {
                TryPutScalar(toMerge.FetchScalar(), out error); // over write this with toMerge
            }
            else
            {
                error = "Type mis-match, one side is Recordset while the other is a scalar";
            }
        }

        /// <summary>
        ///     Fetches the recordset indexs.
        /// </summary>
        /// <returns></returns>
        public IIndexIterator FetchRecordsetIndexes()
        {
            IIndexIterator result = Dev2BinaryDataListFactory.CreateIndexIterator();

            if(IsRecordset)
            {
                result = _internalObj.Keys;
            }

            return result;
        }

        /// <summary>
        ///     Fetches the last index of the recordset.
        /// </summary>
        /// <returns></returns>
        public int FetchLastRecordsetIndex()
        {
            int result = 1;

            if(IsRecordset)
            {
                result = _internalObj.Keys.MaxIndex();
            }

            return result;
        }

        public int FetchAppendRecordsetIndex()
        {
            int result = FetchLastRecordsetIndex();
            if(result >= 1 && _internalObj._appendIndex > 0)
            {
                result++; // inc for insert if data already present
            }
            else if(result == 1 && _internalObj._appendIndex == -1)
            {
                _internalObj._appendIndex = 2; // first pass
                if(!_internalObj.IsEmtpy)
                {
                    result++;
                }
            }
            else if(result > 1)
            {
                result++;
            }
            return result;
        }

        /// <summary>
        ///     Makes the recordset evaluate ready.
        /// </summary>
        /// <param name="keepIdx">The keep idx.</param>
        /// <param name="keepCol">The keep col.</param>
        /// <param name="error">The error.</param>
        public void MakeRecordsetEvaluateReady(int keepIdx, string keepCol, out string error)
        {
            error = string.Empty;

            // use only wants a specific column retained, not the entire row
            if(keepCol != null)
            {
                IList<Dev2Column> newCols = new List<Dev2Column>(Columns.Count) { new Dev2Column(keepCol, string.Empty) };
                // remove values
                Columns = newCols;
            }
            if(keepIdx == GlobalConstants.AllIndexes)
            {
                return;
            }
            if(!_internalObj.IsEmtpy)
            {
                var loopedIndexIterator = new LoopedIndexIterator(keepIdx, 1);
                _internalObj.Keys = loopedIndexIterator;
            }
        }

        public IBinaryDataListItem TryFetchRecordsetColumnAtIndex(string field, int idx, out string error)
        {
            error = string.Empty;
            IList<IBinaryDataListItem> cols = FetchRecordAt(idx, field, out error);
            IBinaryDataListItem result = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);

            if(cols == null || cols.Count == 0)
            {
                error = "Index [ " + idx + " ] is out of bounds";
            }
            else
            {
                result = cols.FirstOrDefault(c => c.FieldName == field);
            }

            return result;
        }

        public IBinaryDataListItem TryFetchLastIndexedRecordsetUpsertPayload(out string error)
        {
            //            error = string.Empty;
            //            IBinaryDataListItem result = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);
            // in this case there is a single row, with a single column's data to extract
            int idx = FetchLastRecordsetIndex();

            return InternalFetchIndexedRecordsetUpsertPayload(idx, out error);
        }

        public IBinaryDataListItem TryFetchIndexedRecordsetUpsertPayload(int idx, out string error)
        {
            //            error = string.Empty;
            //            IBinaryDataListItem result = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);

            return InternalFetchIndexedRecordsetUpsertPayload(idx, out error);
        }

        public void BlankRecordSetData(string colName)
        {
            IIndexIterator ii = _internalObj.Keys;

            if(colName != null)
            {
                Dev2Column cc = Columns.FirstOrDefault(c => c.ColumnName == colName);
                string error = string.Empty;

                if(cc != null)
                {
                    int colIdx = Columns.IndexOf(cc);

                    while(ii.HasMore())
                    {
                        int next = ii.FetchNextIndex();
                        // now blank all values at this location
                        IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, Namespace, cc.ColumnName, next);
                        TryPutRecordItemAtIndex(itm, next, out error);
                    }
                }
                else
                {
                    ClearAll(ii);
                }
            }
            else
            {
                ClearAll(ii);
            }
        }

        public bool HasColumns(IList<Dev2Column> cols)
        {
            bool result = true;
            IList<Dev2Column> myCols = Columns;
            int i = 0;

            while(i < cols.Count && result)
            {
                if(!myCols.Contains(cols[i]))
                {
                    result = false;
                }
                i++;
            }

            return result;
        }

        public bool HasField(string field)
        {
            bool result = false;

            if(IsRecordset)
            {
                if(Columns.FirstOrDefault(c => c.ColumnName == field) != null)
                {
                    result = true;
                }
            }

            return result;
        }

        public void Sort(string field, bool desc, out string error)
        {
            // INFO : http://stackoverflow.com/questions/925471/c-sharp-help-sorting-a-list-of-objects-in-c-sharp
            error = string.Empty;
            Dev2Column col = Columns.FirstOrDefault(c => c.ColumnName == field);
            IDictionary<int, IList<IBinaryDataListItem>> toSort = _internalObj.FetchSortData();
            IDictionary<int, IList<IBinaryDataListItem>> sortedData = null;

            if(col != null)
            {
                int colIdx = Columns.IndexOf(col);

                // Port of per DLS approach -- Technical debt
                // Int
                try
                {
                    sortedData = IntSort(toSort, field, colIdx, desc);
                }
                catch(Exception)
                {
                    // DateTime
                    try
                    {
                        sortedData = DateTimeSort(toSort, field, colIdx, desc);
                    }
                    catch(Exception)
                    {
                        // String
                        try
                        {
                            sortedData = StringSort(toSort, field, colIdx, desc);
                        }
                        catch(Exception)
                        {
                            // Very naughty thing have happened....
                            error = "Invalid format for sorting on field [ " + field + " ] ";
                        }
                    }
                }

                // apply the update ;)
                _internalObj.ApplySortAction(sortedData);
            }
        }

        public bool IsEmpty()
        {
            return _internalObj.IsEmtpy;
        }

        public bool TryDeleteRows(string index)
        {
            bool result = false;
            if(IsRecordset && index != null)
            {
                int numericIndex = 0;
                if(!Int32.TryParse(index, out numericIndex))
                {
                    if(string.IsNullOrEmpty(index))
                    {
                        result = DeleteLastRow();
                    }
                    else if(index == "*")
                    {
                        result = DeleteAllRows();
                    }
                }
                else
                {
                    result = DeleteRowAtIndex(numericIndex);
                }
            }
            return result;
        }

        /// <summary>
        ///     Fetches the record at.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="field">The exact column needed to be fetched</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public IList<IBinaryDataListItem> FetchRecordAt(int idx, string field, out string error)
        {
            IList<IBinaryDataListItem> result = null;
            error = string.Empty;

            if(idx != 0 && !_internalObj.TryGetValue(idx, out result))
            {
                // row miss, create it and return it ;)
                result = CorrectRecordsetFetchMiss(idx, out error);
            }
            if(result == null)
            {
                return null;
            }
            IEnumerable<string> colNames;
            if(String.IsNullOrEmpty(field))
            {
                colNames = Columns.Select(column => column.ColumnName);
            }
            else
            {
                colNames = new List<string> { field };
            }
            IList<IBinaryDataListItem> resultList = new List<IBinaryDataListItem>();
            foreach(var colName in colNames)
            {
                List<IBinaryDataListItem> binaryDataListItems = result.Where(item => item.FieldName == colName).ToList();
                if(binaryDataListItems.Count > 0)
                {
                    resultList.Add(binaryDataListItems[0]);
                }
            }
            if(resultList.Count == 0)
            {
                resultList = CorrectRecordsetFetchMiss(idx, out error);
            }
            return resultList;
        }

        int InternalFetchColumnIndex(string column)
        {
            int result = -1;
            if(!_strToColIdx.TryGetValue(column, out result))
            {
                Dev2Column colToFind = Columns.FirstOrDefault(c => c.ColumnName == column);

                if(colToFind != null)
                {
                    result = Columns.IndexOf(colToFind);
                    _strToColIdx[column] = result; // save to cache ;)
                }
                else
                {
                    result = -1; // it failed, default back to non-valid index
                }
            }

            return result;
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        ///     Deletes all the rows.
        /// </summary>
        /// <returns></returns>
        bool DeleteAllRows()
        {
            SBinaryDataListEntry tmp = new SBinaryDataListEntry();
            tmp.Init();
            tmp.IsRecordset = _internalObj.IsRecordset;
            tmp.Columns = _internalObj.Columns;
            tmp.Namespace = _internalObj.Namespace;
            tmp.DataListKey = _internalObj.DataListKey;
            tmp.Description = _internalObj.Description;
            tmp.IsEditable = _internalObj.IsEditable;
            tmp.ColumnIODirection = _internalObj.ColumnIODirection;
            tmp._appendIndex = _internalObj._appendIndex;
            _internalObj = tmp;
            return true;
        }

        /// <summary>
        ///     Deletes the last row.
        /// </summary>
        /// <returns></returns>
        bool DeleteLastRow()
        {
            int lastRowIndex = FetchLastRecordsetIndex();

            // Bug 8725
            if(!_internalObj.IsEmtpy)
            {
                _internalObj.Remove(lastRowIndex);
            }

            return true;
        }

        /// <summary>
        ///     Deletes the row at a specific index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        bool DeleteRowAtIndex(int index)
        {
            bool result = false;
            int lastIndex = FetchLastRecordsetIndex();
            if(index <= lastIndex && index > 0)
            {
                if(index != lastIndex)
                {
                    while(index < lastIndex)
                    {
                        for(int i = 0; i < Columns.Count; i++)
                        {
                            IBinaryDataListItem nextItem = _internalObj[index + 1][i];
                            _internalObj[index].Remove(_internalObj[index][i]);

                            _internalObj[index].Insert(i,
                                Dev2BinaryDataListFactory.CreateBinaryItem(nextItem.TheValue,
                                    nextItem.Namespace,
                                    nextItem.FieldName,
                                    nextItem
                                        .ItemCollectionIndex -
                                    1));
                        }
                        index++;
                    }
                }
                DeleteLastRow();
                result = true;
            }
            return result;
        }

        /// <summary>
        ///     Fetches the index iterator.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        IIndexIterator FetchIndexIterator(string index)
        {
            int numericIndex = 0;
            IIndexIterator indexIterator = null;
            if(!Int32.TryParse(index, out numericIndex))
            {
                if(string.IsNullOrEmpty(index))
                {
                    indexIterator = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(FetchLastRecordsetIndex(), 1);
                }
                else if(index == "*")
                {
                    indexIterator = FetchRecordsetIndexes();
                }
            }
            else
            {
                indexIterator = new LoopedIndexIterator(numericIndex, 1);
            }
            return indexIterator;
        }

        /// <summary>
        ///     Ints the sort.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="desc">
        ///     if set to <c>true</c> [desc].
        /// </param>
        IDictionary<int, IList<IBinaryDataListItem>> IntSort(IDictionary<int, IList<IBinaryDataListItem>> toSort, string field, int colIdx, bool desc)
        {
            IDictionary _toSwap = new Dictionary<int, IList<IBinaryDataListItem>>();

            if(!desc)
            {
                var data = toSort.OrderBy(x =>
                {
                    int val;
                    string tmpVal = x.Value[colIdx].TheValue;
                    if(string.IsNullOrWhiteSpace(tmpVal))
                    {
                        val = int.MinValue;
                    }
                    else
                    {
                        if(!int.TryParse(tmpVal, out val))
                        {
                            throw new Exception();
                        }
                    }
                    return val;
                }).ToList();
                int idx = 1;
                foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    _toSwap[idx] = tmp.Value;
                    idx++;
                }
            }
            else
            {
                var data = toSort.OrderByDescending(x =>
                {
                    int val;
                    string tmpVal = x.Value[colIdx].TheValue;
                    if(string.IsNullOrWhiteSpace(tmpVal))
                    {
                        val = int.MinValue;
                    }
                    else
                    {
                        if(!int.TryParse(tmpVal, out val))
                        {
                            throw new Exception();
                        }
                    }
                    return val;
                }).ToList();
                int idx = 1;
                foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    _toSwap[idx] = tmp.Value;
                    idx++;
                }
            }

            toSort.Clear();

            // make the swap
            foreach(int k in _toSwap.Keys)
            {
                toSort[k] = (IList<IBinaryDataListItem>)_toSwap[k];
            }

            return toSort;
        }

        /// <summary>
        ///     Dates the time sort.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="colIdx">The col idx.</param>
        /// <param name="desc">
        ///     if set to <c>true</c> [desc].
        /// </param>
        IDictionary<int, IList<IBinaryDataListItem>> DateTimeSort(IDictionary<int, IList<IBinaryDataListItem>> toSort, string field, int colIdx, bool desc)
        {
            IDictionary _toSwap = new Dictionary<int, IList<IBinaryDataListItem>>();

            if(!desc)
            {
                var data = toSort.OrderBy(x =>
                {
                    DateTime val;
                    string tmpVal = x.Value[colIdx].TheValue;
                    if(string.IsNullOrWhiteSpace(tmpVal))
                    {
                        val = DateTime.MinValue;
                    }
                    else
                    {
                        if(!DateTime.TryParse(x.Value[colIdx].TheValue, out val))
                        {
                            throw new Exception();
                        }
                    }
                    return val;
                }).ToList();
                int idx = 1;
                foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    _toSwap[idx] = tmp.Value;
                    idx++;
                }
            }
            else
            {
                var data = toSort.OrderByDescending(x =>
                {
                    DateTime val;
                    string tmpVal = x.Value[colIdx].TheValue;
                    if(string.IsNullOrWhiteSpace(tmpVal))
                    {
                        val = DateTime.MinValue;
                    }
                    else
                    {
                        if(!DateTime.TryParse(x.Value[colIdx].TheValue, out val))
                        {
                            throw new Exception();
                        }
                    }
                    return val;
                }).ToList();
                int idx = 1;
                foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    _toSwap[idx] = tmp.Value;
                    idx++;
                }
            }

            toSort.Clear();

            // make the swap
            foreach(int k in _toSwap.Keys)
            {
                toSort[k] = (IList<IBinaryDataListItem>)_toSwap[k];
            }

            return toSort;
        }

        /// <summary>
        ///     Strings the sort.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="colIdx">The col idx.</param>
        /// <param name="desc">
        ///     if set to <c>true</c> [desc].
        /// </param>
        IDictionary<int, IList<IBinaryDataListItem>> StringSort(IDictionary<int, IList<IBinaryDataListItem>> toSort, string field, int colIdx, bool desc)
        {
            IDictionary _toSwap = new Dictionary<int, IList<IBinaryDataListItem>>();

            if(!desc)
            {
                var data = toSort.OrderBy(x => x.Value[colIdx].TheValue).ToList();
                int idx = 1;
                foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    _toSwap[idx] = tmp.Value;
                    idx++;
                }
            }
            else
            {
                var data = toSort.OrderByDescending(x => x.Value[colIdx].TheValue).ToList();
                int idx = 1;
                foreach(KeyValuePair<int, IList<IBinaryDataListItem>> tmp in data)
                {
                    _toSwap[idx] = tmp.Value;
                    idx++;
                }
            }

            toSort.Clear();

            // make the swap
            foreach(int k in _toSwap.Keys)
            {
                toSort[k] = (IList<IBinaryDataListItem>)_toSwap[k];
            }

            return toSort;
        }

        /// <summary>
        ///     Tries the fetch indexed recordset upsert payload.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IBinaryDataListItem InternalFetchIndexedRecordsetUpsertPayload(int idx, out string error)
        {
            error = string.Empty;
            IBinaryDataListItem result = Dev2BinaryDataListFactory.CreateBinaryItem(string.Empty, string.Empty);
            // in this case there is a single row, with a single column's data to extract
            Dev2Column col = Columns.FirstOrDefault();
            result = TryFetchRecordsetColumnAtIndex(col.ColumnName, idx, out error);
            return result;
        }

        /// <summary>
        ///     Clears all.
        /// </summary>
        /// <param name="keys">The keys.</param>
        void ClearAll(IIndexIterator idxItr)
        {
            // miss, clear it all out ;)
            while(idxItr.HasMore())
            {
                int next = idxItr.FetchNextIndex();
                _internalObj.Remove(next);
            }
        }

        /// <summary>
        ///     Corrects the recordset fetch miss.
        /// </summary>
        /// <param name="idx">The idx.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        IList<IBinaryDataListItem> CorrectRecordsetFetchMiss(int idx, out string error)
        {
            IList<IBinaryDataListItem> result = new List<IBinaryDataListItem>();
            error = string.Empty;
            bool isEmtpy = _internalObj[0].Count == 0;
            foreach(Dev2Column c in Columns)
            {
                IBinaryDataListItem tmp = new BinaryDataListItem(string.Empty, Namespace, c.ColumnName, idx.ToString());
                if(error == string.Empty)
                {
                    if(!isEmtpy)
                    {
                        TryPutRecordItemAtIndex(tmp, idx, out error);
                    }
                    result.Add(tmp);
                }
            }

            return result;
        }

        #endregion Private Methods


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

         ~BinaryDataListEntry()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (!disposing) return;
            _internalObj.Dispose();
        }
    }
}
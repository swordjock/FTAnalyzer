using System;
using System.Collections.ObjectModel;
using System.Data;
using GeoAPI;
using GeoAPI.Geometries;
using SharpMap.Base;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Abstract base provider that handles geometry factory based on SRID
    /// </summary>
    [Serializable]
    public abstract class BaseProvider : DisposableObject, IProvider
    {
        private int _srid;
        private bool _isOpen;

        /// <summary>
        /// Event raised when <see cref="SRID"/> has changed
        /// </summary>
        public event EventHandler SridChanged;

        protected IGeometryFactory Factory { get; set; }

        protected BaseProvider()
            :this(0)
        {
        }

        protected BaseProvider(int srid)
        {
            ConnectionID = string.Empty;
            SRID = srid;
            Factory = GeometryServiceProvider.Instance.CreateGeometryFactory(SRID);
        }

        protected override void ReleaseManagedResources()
        {
            Factory = null;
            base.ReleaseManagedResources();
        }

        #region Implementation of IProvider

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        public string ConnectionID { get; protected set; }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public virtual bool IsOpen { get { return _isOpen; } }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _srid; }
            set
            {
                if (value != _srid)
                {
                    _srid = value;
                    OnSridChanged(EventArgs.Empty);
                }
            }
        }

        protected virtual void OnSridChanged(EventArgs eventArgs)
        {
            Factory = GeometryServiceProvider.Instance.CreateGeometryFactory(SRID);
            
            if (SridChanged != null)
                SridChanged(this, eventArgs);
        }

        /// <summary>
        /// Gets the features within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="GeoAPI.Geometries.Envelope"/></returns>
        public abstract Collection<IGeometry> GetGeometriesInView(Envelope bbox);

        /// <summary>
        /// Returns all objects whose <see cref="GeoAPI.Geometries.Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplifed by their <see cref="GeoAPI.Geometries.Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
        public abstract Collection<uint> GetObjectIDsInView(Envelope bbox);

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public abstract IGeometry GetGeometryByID(uint oid);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            OnBeginExecuteIntersectionQuery(geom);
            OnExecuteIntersectionQuery(geom, ds);
            OnEndExecuteIntersectionQuery();
        }

        protected virtual void OnBeginExecuteIntersectionQuery(IGeometry geom)
        {
            // ToDo: Do we need events raised?
        }

        // ToDo: Do we need events raised?
        protected abstract void OnExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds);

        protected virtual void OnEndExecuteIntersectionQuery()
        {
            // ToDo: Do we need events raised?
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public abstract void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds);

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public abstract int GetFeatureCount();

        /// <summary>
        /// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on a RowID
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns>datarow</returns>
        public abstract FeatureDataRow GetFeature(uint rowId);

        /// <summary>
        /// <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public abstract Envelope GetExtents();

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public virtual void Open()
        {
            _isOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public virtual void Close()
        {
            _isOpen = false;
        }

        #endregion

        protected static FeatureDataTable CloneTableStructure(FeatureDataTable baseTable)
        {
            var res = new FeatureDataTable(baseTable);
            var cols = res.Columns;
            foreach (DataColumn column in baseTable.Columns)
            {
                cols.Add(new DataColumn(column.ColumnName, column.DataType, column.Expression, column.ColumnMapping)
                    
                /*{AllowDBNull = column.AllowDBNull, AutoIncrement = column.AutoIncrement, AutoIncrementSeed = column.AutoIncrementSeed,
                    AutoIncrementStep = column.AutoIncrementStep, Caption = column.Caption}*/);
            }
            /*
            var constraints = res.Constraints;
            foreach (var constraint in baseTable.Constraints)
            {
                var uc = constraint as UniqueConstraint;
                if (uc != null)
                {
                }
            }
            */
            return res;
        }
    }
}
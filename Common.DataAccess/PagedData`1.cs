using System.Collections.Generic;

namespace Common.DataAccess {

	/// <summary>
	/// Represents a "page" of data from a list (usually a database table).
	/// </summary>
	/// <remarks>
	/// The data itself is not retrieved or controlled in any way by this class. This class merely
	/// assists a consuming layer (such as a User-Interface or service-layer) to understand the
	/// current page and the total number of pages.
	/// <para>
	/// It is assumed that all collections are zero-based.
	/// </para>
	/// </remarks>
	/// <typeparam name="TEntity">A typical POCO object (usually an Entity generated from an EF T4 template)</typeparam>
	public class PagedData<TEntity> : PagedData where TEntity : class {

		/// <param name="totalRecords">Populates <see cref="PagedData.TotalRecords"/></param>
		/// <param name="pageSize">Populates <see cref="PagedData.PageSize"/></param>
		/// <param name="startIndex">Populates <see cref="PagedData.StartIndex"/></param>
		/// <param name="data">Populates <see cref="Data"/></param>
		public PagedData(long totalRecords, int pageSize, long startIndex, IEnumerable<TEntity> data)
			: base(totalRecords, pageSize, startIndex, data) {
		}

		/// <summary>
		/// Contains the current page's data.
		/// </summary>
		public new IEnumerable<TEntity> Data {
			get {
				return (IEnumerable<TEntity>)base.Data;
			}
		}

	}

}

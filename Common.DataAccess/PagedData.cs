using System;
using System.Collections;

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
	public class PagedData {

		/// <param name="totalRecords">Populates <see cref="TotalRecords"/></param>
		/// <param name="pageSize">Populates <see cref="PageSize"/></param>
		/// <param name="startIndex">Populates <see cref="StartIndex"/></param>
		/// <param name="data">Populates <see cref="Data"/></param>
		protected PagedData(long totalRecords, int pageSize, long startIndex, IEnumerable data) {
			if (totalRecords < 0) {
				throw new Exceptions.PagedData_InvalidTotalRecordsException();
			}
			if (pageSize < 1) {
				throw new Exceptions.PagedData_InvalidPageSizeException();
			}
			if (startIndex < 0) {
				throw new Exceptions.PagedData_InvalidStartIndexException();
			}

			this.PageSize = pageSize;
			this.Data = data;
			if (startIndex >= totalRecords && totalRecords != 0) {
				throw new Exceptions.PagedData_StartIndexIsGreaterThanTotalRecordsException();
			}
			if (totalRecords == 0) {
				this.TotalRecords = 0;
				this.StartIndex = 0;

				this.TotalPages = 0;
				this.CurrentPage = 0;
			} else {
				this.TotalRecords = totalRecords;
				this.StartIndex = startIndex;

				this.TotalPages = Convert.ToInt32(Math.Ceiling((double)TotalRecords / PageSize));
				this.CurrentPage = StartIndex / PageSize;
			}
		}

		/// <summary>
		/// Indicates the total number of records in the list.
		/// </summary>
		/// <remarks>
		/// For records 15-29 from a list of 125 records, the TotalRecords would be 125.
		/// </remarks>
		public long TotalRecords { get; private set; }

		/// <summary>
		/// Indicates the number of records which were requested from the data-store when
		/// populating this page.
		/// </summary>
		/// <remarks>
		/// For records 15-29 from a list of 125 records, the PageSize would be 15.
		/// </remarks>
		public int PageSize { get; private set; }

		/// <summary>
		/// Indicates the index within the list at which this page starts.
		/// </summary>
		/// <remarks>
		/// For records 15-29 from a list of 125 records, the StartIndex would be 15.
		/// </remarks>
		public long StartIndex { get; private set; }

		/// <summary>
		/// Indicates the current zero-based page index.
		/// </summary>
		/// <remarks>
		/// For records 15-29 from a list of 125 records, the CurrentPage would be 1.
		/// <para>
		/// Calculated by <c><see cref="StartIndex"/> / <see cref="PageSize"/></c>
		/// </para>
		/// </remarks>
		public long CurrentPage { get; private set; }

		/// <summary>
		/// Indicates the total number of pages.
		/// </summary>
		/// <remarks>
		/// For records 15-29 from a list of 125 records, the TotalPages would be 9.
		/// <para>
		/// Calculated by <c>Convert.ToInt32(Math.Ceiling((<see langword="double"/>)<see cref="TotalRecords"/> / <see cref="PageSize"/>))</c>
		/// </para>
		/// </remarks>
		public long TotalPages { get; private set; }

		/// <summary>
		/// Contains the current page's data.
		/// </summary>
		public virtual IEnumerable Data { get; private set; }

	}

}

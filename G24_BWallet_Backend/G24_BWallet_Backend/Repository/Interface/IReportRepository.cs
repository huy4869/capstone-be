using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IReportRepository
    {
        Task<Report> GetReportByID(int reportID);
        Task<List<ReportReturn>> GetListReport(int eventId);
        Task<List<ReportReturn>> GetSolvedReports(int eventId);
        Task<Report> createReport(int receiptID, int userID, string reason);
        Task<Report> responeReport(int reportId, int status);
    }
}

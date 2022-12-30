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
        Task<List<ReportReturn>> GetYourReports(int eventId, int userId);
        Task<string> createReport(int receiptID, int userID, string reason);
        Task<string> responeReport(int reportId, int status,int userId);
    }
}

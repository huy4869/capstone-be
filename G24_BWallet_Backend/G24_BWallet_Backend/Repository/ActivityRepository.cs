using Amazon.S3.Model;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly MyDBContext context;

        public ActivityRepository(MyDBContext myDB)
        {
            this.context = myDB;
        }

        public async Task AddActivity(int userId, string content, string iconType)
        {
            try
            {
                Activity activity = new Activity();
                activity.UserID = userId;
                activity.Content = content;
                activity.CreatedAt = System.DateTime.Now;
                activity.UpdatedAt = System.DateTime.Now;
                if (iconType != null)
                {
                    ActivityIcon activityIcon = await context.ActivityIcons
                            .FirstOrDefaultAsync(a => a.Type.Equals(iconType.Trim()));
                    if (activityIcon != null)
                        activity.ActivityIconId = activityIcon.ID;
                }
                await context.Activities.AddAsync(activity);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<List<ActivityScreen>> GetActivity(int userId)
        {
            List<ActivityScreen> list = new List<ActivityScreen>();
            var activities = await context.Activities.Include(a => a.ActivityIcon)
                .Where(a => a.UserID == userId).ToListAsync();
            foreach (var activity in activities)
            {
                ActivityScreen activityScreen = new ActivityScreen();
                if (activity.ActivityIcon != null)
                    activityScreen.Link = activity.ActivityIcon.Link;
                activityScreen.Content = activity.Content.ToString();
                activityScreen.Date = activity.CreatedAt;
                list.Add(activityScreen);
            }
            return list;
        }

        public async Task InspectorReceiptApproveActivity(int status, int userId,
            string receiptName, string eventName)
        {
            string statuss = (status == 2) ? "phê duyệt" : "từ chối";
            string content = "Bạn đã " + statuss + " chứng từ [" + receiptName + "] nhóm [" + eventName + "]";
            await AddActivity(userId, content, "receipt");
        }

        public async Task CreatorReceiptApproveActivity(int status, int userId,
            string receiptName, string eventName)
        {
            string statuss = (status == 2) ? "được phê duyệt" : "bị từ chối";
            string content = string.Format("Chứng từ [{0}] trong nhóm [{1}] của bạn đã {2}", receiptName,
                eventName, statuss);
            await AddActivity(userId, content, "receipt");
        }

        public async Task AddReceiptActivity(int userID, string receiptName, int eventID)
        {
            Event e = await context.Events.FirstOrDefaultAsync(e => e.ID == eventID);
            string content = string.Format("Chứng từ [{0}] trong nhóm [{1}]" +
                " bạn mới thêm đang chờ duyệt", receiptName, e.EventName);
            await AddActivity(userID, content, "receipt");
        }
    }
}
